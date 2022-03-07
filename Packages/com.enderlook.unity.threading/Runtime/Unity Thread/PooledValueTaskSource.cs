using Enderlook.Pools;

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

using UnityEngine;

namespace Enderlook.Unity.Threading
{
    /// <summary>
    /// A pooled implementation of <see cref="IValueTaskSource{T}"/>.
    /// </summary>
    /// <typeparam name="T">Return type.</typeparam>
    public sealed class PooledValueTaskSource<T> : IValueTaskSource<T>
    {
        // https://github.com/kkokosa/PooledValueTaskSource/blob/master/src/PooledValueTaskSource/FileReadingPooledValueTaskSource.cs

        private static readonly ContextCallback contextCallback = e =>
        {
            Debug.Assert(e is Tuple3<PooledValueTaskSource<T>, Action<object>, object>);
            Tuple3<PooledValueTaskSource<T>, Action<object>, object> tuple = Unsafe.As<Tuple3<PooledValueTaskSource<T>, Action<object>, object>>(e);
            PooledValueTaskSource<T> taskSource = tuple.Item1;
            Action<object> continuation = tuple.Item2;
            object state = tuple.Item3;
            tuple.Return();
            taskSource.InvokeContinuation(continuation, state, forceAsync: false);
        };
        private static readonly SendOrPostCallback sendOrPostCallback = e =>
        {
            Debug.Assert(e is Tuple2<Action<object>, object>);
            Tuple2<Action<object>, object> tuple = Unsafe.As<Tuple2<Action<object>, object>>(e);
            Action<object> continuation = tuple.Item1;
            object state = tuple.Item2;
            tuple.Return();
            continuation(state);
        };
        private static readonly WaitCallback waitCallback = e =>
        {
            Debug.Assert(e is Tuple2<Action<object>, object>);
            Tuple2<Action<object>, object> tuple = Unsafe.As<Tuple2<Action<object>, object>>(e);
            Action<object> continuation = tuple.Item1;
            object state = tuple.Item2;
            tuple.Return();
            continuation(state);
        };

        /// <summary>
        /// Sentinel object used to indicate that the operation has completed prior to OnCompleted being called.
        /// </summary>
        private static readonly Action<object> CallbackCompleted = _ => Debug.Assert(false, "Should not be invoked.");
        private Action<object> continuation;
        private T result;
        private Exception exception;
        /// <summary>Current token value given to a ValueTask and then verified against the value it passes back to us.</summary>
        /// <remarks>
        /// This is not meant to be a completely reliable mechanism, doesn't require additional synchronization, etc.
        /// It's purely a best effort attempt to catch misuse, including awaiting for a value task twice and after
        /// it's already being reused by someone else.
        /// </remarks>
        private short token;
        private object state;

        private ExecutionContext executionContext;
        private object scheduler;

        public static PooledValueTaskSource<T> Rent() => ObjectPool<PooledValueTaskSource<T>>.Shared.Rent();

        public T GetResult(short token)
        {
            if (token != this.token)
                ThrowMultipleContinuations();
            Exception exception = this.exception;
            T result = this.result;
            Release();
            return !(exception is null) ? throw exception : result;
        }

        public ValueTaskSourceStatus GetStatus(short token)
        {
            if (token != this.token)
                ThrowMultipleContinuations();
            if (!ReferenceEquals(continuation, CallbackCompleted))
                return ValueTaskSourceStatus.Pending;
            return !(exception is null) ? ValueTaskSourceStatus.Succeeded : ValueTaskSourceStatus.Faulted;
        }

        /// <summary>Called on awaiting so:
        /// - If operation has not yet completed - Queues the provided continuation to be executed once the operation is completed.
        /// - If operation has completed.
        /// </summary>
        public void OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
        {
            if (token != this.token)
                ThrowMultipleContinuations();

            if ((flags & ValueTaskSourceOnCompletedFlags.FlowExecutionContext) != 0)
                executionContext = ExecutionContext.Capture();

            if ((flags & ValueTaskSourceOnCompletedFlags.UseSchedulingContext) != 0)
            {
                SynchronizationContext currentSyncronizationContext = SynchronizationContext.Current;
                if (!(currentSyncronizationContext is null) && currentSyncronizationContext.GetType() != typeof(SynchronizationContext))
                    scheduler = currentSyncronizationContext;
                else
                {
                    TaskScheduler taskScheduler = TaskScheduler.Current;
                    if (taskScheduler != TaskScheduler.Default)
                        scheduler = taskScheduler;
                }
            }

            // Remember current state
            this.state = state;
            // Remember continuation to be executed on completed (if not already completed, in case of which
            // continuation will be set to CallbackCompleted)
            Action<object> previousContinuation = Interlocked.CompareExchange(ref this.continuation, continuation, null);
            if (previousContinuation != null)
            {
                if (!ReferenceEquals(previousContinuation, CallbackCompleted))
                    ThrowMultipleContinuations();

                // Lost the race condition and the operation has now already completed.
                // We need to invoke the continuation, but it must be asynchronously to
                // avoid a stack dive. However, since all of the queueing mechanisms flow
                // ExecutionContext, and since we're still in the same context where we
                // captured it, we can just ignore the one we captured.
                executionContext = null;
                this.state = null; // We have the state in "state"; no need for the one in UserToken
                InvokeContinuation(continuation, state, forceAsync: true);
            }
        }

        public void NotifyAsyncWorkSuccess(T result)
        {
            this.result = result;
            NotifyAsyncWorkCompletion();
        }

        public void NotifyAsyncWorkException(Exception exception)
        {
            this.exception = exception;
            NotifyAsyncWorkCompletion();
        }

        private void NotifyAsyncWorkCompletion()
        {
            // Mark operation as completed
            Action<object> previousContinuation = Interlocked.CompareExchange(ref continuation, CallbackCompleted, null);
            if (!(previousContinuation is null))
            {
                // Async work completed, continue with the continuation
                ExecutionContext executionContext = this.executionContext;
                if (executionContext is null)
                    InvokeContinuation(previousContinuation, state, forceAsync: false);
                else
                {
                    // This case should be relatively rare, as the async Task/ValueTask method builders
                    // use the awaiter's UnsafeOnCompleted, so this will only happen with code that
                    // explicitly uses the awaiter's OnCompleted instead.
                    this.executionContext = null;
                    Tuple3<PooledValueTaskSource<T>, Action<object>, object> tuple = ObjectPool<Tuple3<PooledValueTaskSource<T>, Action<object>, object>>.Shared.Rent();
                    tuple.Item1 = this;
                    tuple.Item2 = previousContinuation;
                    tuple.Item3 = state;
                    ExecutionContext.Run(executionContext, contextCallback, tuple);
                }
            }
        }

        private void InvokeContinuation(Action<object> continuation, object state, bool forceAsync)
        {
            if (continuation == null)
                return;

            object scheduler = this.scheduler;
            this.scheduler = null;
            if (scheduler != null)
            {
                if (scheduler is SynchronizationContext sc)
                {
                    Tuple2<Action<object>, object> tuple = ObjectPool<Tuple2<Action<object>, object>>.Shared.Rent();
                    tuple.Item1 = continuation;
                    tuple.Item2 = state;
                    sc.Post(sendOrPostCallback, tuple);
                }
                else
                {
                    Debug.Assert(scheduler is TaskScheduler, $"Expected TaskScheduler, got {scheduler}");
                    Task.Factory.StartNew(continuation, state, CancellationToken.None, TaskCreationOptions.DenyChildAttach, (TaskScheduler)scheduler);
                }
            }
            else if (forceAsync)
            {
                Tuple2<Action<object>, object> tuple = ObjectPool<Tuple2<Action<object>, object>>.Shared.Rent();
                tuple.Item1 = continuation;
                tuple.Item2 = state;
                ThreadPool.QueueUserWorkItem(waitCallback, tuple);
            }
            else
                continuation(state);
        }

        private static void ThrowMultipleContinuations() => throw new InvalidOperationException("Multiple awaiters are not allowed");

        private void Release()
        {
            result = default; // TODO: In .Net Standard 2.1 use RuntimeHelpers.IsReferenceOrContainsReferences<T>
            exception = null;
            state = null;
            continuation = null;
            token++;
            ObjectPool<PooledValueTaskSource<T>>.Shared.Return(this);
        }
    }
}