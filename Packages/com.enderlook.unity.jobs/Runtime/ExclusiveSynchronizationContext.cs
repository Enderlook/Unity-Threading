using Enderlook.Pools;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;

namespace Enderlook.Unity.Jobs
{
    internal sealed class ExclusiveSynchronizationContext : SynchronizationContext
    {
        private static readonly SendOrPostCallback startVoid = async e =>
        {
            Debug.Assert(e is ExclusiveSynchronizationContext);
            ExclusiveSynchronizationContext context = Unsafe.As<ExclusiveSynchronizationContext>(e);
            object func = context.container;
            context.container = null;
            try
            {
                Debug.Assert(func is Func<Task>);
                await Unsafe.As<Func<Task>>(func)();
            }
            catch (Exception exception)
            {
                context.InnerException = exception;
                throw;
            }
            finally
            {
                context.EndMessageLoop();
            }
        };

        private static readonly SendOrPostCallback startVoidWithScheduler = e =>
        {
            Debug.Assert(e is ExclusiveSynchronizationContext);
            ExclusiveSynchronizationContext context = Unsafe.As<ExclusiveSynchronizationContext>(e);
            object task = context.container;
            context.container = null;
            UnityJobTaskScheduler scheduler = context.scheduler;
            context.scheduler = null;
            try
            {
                Debug.Assert(task is Task);
                scheduler.TryExecuteTask_(Unsafe.As<Task>(task));
            }
            catch (Exception exception)
            {
                context.InnerException = exception;
                throw;
            }
            finally
            {
                context.EndMessageLoop();
            }
        };

        private static readonly SendOrPostCallback stop = e =>
        {
            Debug.Assert(e is ExclusiveSynchronizationContext);
            ExclusiveSynchronizationContext context = Unsafe.As<ExclusiveSynchronizationContext>(e);
            context.done = true;
        };

        private readonly AutoResetEvent workItemsWaiting = new AutoResetEvent(false);
        private readonly Queue<(SendOrPostCallback Continuation, object State)> items = new Queue<(SendOrPostCallback Continuation, object State)>();
        public Exception InnerException { get; set; }
        private bool done;
        // Prior execution, this can contain either Func<Task>, Func<Task<T>> or Task.
        // At the end, this can be null or StrongBox<T>.
        private object container;
        private UnityJobTaskScheduler scheduler;

        private static class Container<T>
        {
            public static readonly SendOrPostCallback start = async e =>
            {
                Debug.Assert(e is ExclusiveSynchronizationContext);
                ExclusiveSynchronizationContext context = Unsafe.As<ExclusiveSynchronizationContext>(e);
                object func = context.container;
                context.container = null;
                try
                {
                    Debug.Assert(func is Func<Task<T>>);
                    T result = await Unsafe.As<Func<Task<T>>>(func)();
                    StrongBox<T> box = ObjectPool<StrongBox<T>>.Shared.Rent();
                    box.Value = result;
                    context.container = box;
                }
                catch (Exception exception)
                {
                    context.InnerException = exception;
                    throw;
                }
                finally
                {
                    context.EndMessageLoop();
                }
            };
        }

        public static void Run(Func<Task> function)
        {
            SynchronizationContext oldContext = Current;
            ExclusiveSynchronizationContext context = ObjectPool<ExclusiveSynchronizationContext>.Shared.Rent();
            context.container = function;
            SetSynchronizationContext(context);
            context.Post(startVoid, context);
            context.BeginMessageLoop();
            SetSynchronizationContext(oldContext);
            ObjectPool<ExclusiveSynchronizationContext>.Shared.Return(context);
        }

        public static void Run(Task function, UnityJobTaskScheduler scheduler)
        {
            SynchronizationContext oldContext = Current;
            ExclusiveSynchronizationContext context = ObjectPool<ExclusiveSynchronizationContext>.Shared.Rent();
            context.container = function;
            context.scheduler = scheduler;
            SetSynchronizationContext(context);
            context.Post(startVoidWithScheduler, context);
            context.BeginMessageLoop();
            SetSynchronizationContext(oldContext);
            ObjectPool<ExclusiveSynchronizationContext>.Shared.Return(context);
        }

        public static T Run<T>(Func<Task<T>> task)
        {
            SynchronizationContext oldContext = Current;
            ExclusiveSynchronizationContext context = ObjectPool<ExclusiveSynchronizationContext>.Shared.Rent();
            context.container = task;
            SetSynchronizationContext(context);
            context.Post(Container<T>.start, context);
            context.BeginMessageLoop();
            SetSynchronizationContext(oldContext);
            object box_ = context.container;
            Debug.Assert(box_ is StrongBox<T>);
            StrongBox<T> box = Unsafe.As<StrongBox<T>>(box_);
            ObjectPool<ExclusiveSynchronizationContext>.Shared.Return(context);
            T result = box.Value;
            // TODO: On Net Standard 2.1 use RuntimeHelpers.IsReferenceOrContainsReferences<T>().
            box.Value = default;
            ObjectPool<StrongBox<T>>.Shared.Return(box);
            return result;
        }

        public override void Send(SendOrPostCallback continuation, object state)
            => throw new NotSupportedException("We cannot send to our same thread");

        public override void Post(SendOrPostCallback continuation, object state)
        {
            lock (items)
                items.Enqueue((continuation, state));
            workItemsWaiting.Set();
        }

        public void EndMessageLoop() => Post(stop, this);

        public void BeginMessageLoop()
        {
            while (!done)
            {
                bool found;
                (SendOrPostCallback Continuation, object State) tuple;
                start:
                lock (items)
                    found = items.TryDequeue(out tuple);

                if (found)
                {
                    tuple.Continuation(tuple.State);
                    if (InnerException != null) // The method threw an exeption
                        ThrowAggregateException();
                    goto start;
                }
                else
                    workItemsWaiting.WaitOne();
            }
        }

        private void ThrowAggregateException() => throw new AggregateException("Inner method threw an exception.", InnerException);

        public override SynchronizationContext CreateCopy() => this;
    }
}
