using Enderlook.Unity.Threading;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    /// <summary>
    /// Represent the handler of a value coroutine.
    /// </summary>
    public readonly struct ValueCoroutine : INotifyCompletion
    {
        internal readonly Handle handle;

        internal ValueCoroutine(Handle handle) => this.handle = handle;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ValueCoroutine Start<T, U>(U cancellator, T routine)
            where T : IEnumerator<ValueYieldInstruction>
            where U : ICancellable
        {
            Handle handler = new Handle();
            CoroutineManagers.Start(cancellator, new Enumerator<T>(handler, routine));
            return new ValueCoroutine(handler);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ValueCoroutine StartThreadSafe<T, U>(U cancellator, T routine)
            where T : IEnumerator<ValueYieldInstruction>
            where U : ICancellable
        {
            Handle handler = new Handle();
            CoroutineManagers.StartThreadSafe(cancellator, new Enumerator<T>(handler, routine));
            return new ValueCoroutine(handler);
        }

        /// <summary>
        /// Whenever the value coroutine has completed or not.
        /// </summary>
        public bool IsCompleted => handle.IsCompleted;

        /// <summary>
        /// Return self.
        /// </summary>
        /// <returns>Self.</returns>
        public ValueCoroutine GetAwaiter() => this;

        /// <summary>
        /// Determines the action to run after the coroutine has completed or was stopped.
        /// </summary>
        /// <param name="continuation">Action to run.</param>
        public void OnCompleted(Action continuation) => handle.OnCompleted(continuation);

        private struct Enumerator<T> : IEnumerator<ValueYieldInstruction>
            where T : IEnumerator<ValueYieldInstruction>
        {
            private readonly Handle handler;
            private T enumerator;

            public Enumerator(Handle handler, T enumerator)
            {
                this.handler = handler;
                this.enumerator = enumerator;
            }

            ValueYieldInstruction IEnumerator<ValueYieldInstruction>.Current => enumerator.Current;

            object IEnumerator.Current => enumerator.Current;

            void IDisposable.Dispose() => enumerator.Dispose();

            bool IEnumerator.MoveNext()
            {
                if (enumerator.MoveNext())
                    return true;
                handler.Complete();
                return false;
            }

            void IEnumerator.Reset() => enumerator.Reset();
        }

        internal class Handle
        {
            private static readonly SendOrPostCallback runContinuation = (e) => ((Action)e).Invoke();

            private Action continuation;
            private int isAdding;

            public bool IsCompleted { get; private set; }

            internal void Complete()
            {
#if UNITY_EDITOR
                Debug.Assert(!IsCompleted);
#endif
                IsCompleted = true;

                while (isAdding == 1) ;

                Action action = continuation;
                if (!(action is null))
                {
                    // Complete can be called outside main thread, so we must to ensure we are in the appropiate thread.
                    if (Switch.IsInMainThread)
                        action();
                    else
                        Switch.OnUnityLater(runContinuation, action);
                }
            }

            internal void OnCompleted(Action continuation)
            {
                while (Interlocked.Exchange(ref isAdding, 1) == 0) ;

                if (IsCompleted)
                {
                    Interlocked.Exchange(ref isAdding, 0);
                    // Complete can be called outside main thread, so we must to ensure we are in the appropiate thread.
                    if (Switch.IsInMainThread)
                        this.continuation();
                    else
                        Switch.OnUnityLater(runContinuation, this.continuation);
                }

                try
                {
                    this.continuation += continuation;
                }
                finally
                {
                    Interlocked.Exchange(ref isAdding, 0);
                }
            }
        }
    }
}