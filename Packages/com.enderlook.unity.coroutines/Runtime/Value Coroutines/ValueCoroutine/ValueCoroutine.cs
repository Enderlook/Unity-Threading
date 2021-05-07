using Enderlook.Unity.Threading;

using System;
using System.Runtime.CompilerServices;

namespace Enderlook.Unity.Coroutines
{
    /// <summary>
    /// Represent the handler of a value coroutine.
    /// </summary>
    public readonly partial struct ValueCoroutine : INotifyCompletion
    {
        internal readonly Handle handle;
        // Since the handle is pooled, we use generation to check if it's already used.
        internal readonly uint generation;

        private ValueCoroutine(Handle handle)
        {
            this.handle = handle;
            generation = handle.Generation;
        }

        internal ValueCoroutine(Handle handle, uint generation)
        {
            this.handle = handle;
            this.generation = generation;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ValueCoroutine Start<T>(CoroutineScheduler.Managers managers, T routine)
            where T : IValueCoroutineEnumerator
        {
            Handle handler = ConcurrentPool.Rent<Handle>();
            managers.Start(new Coroutine<T>(handler, routine));
            return new ValueCoroutine(handler);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ValueCoroutine ConcurrentStart<T>(CoroutineScheduler.Managers managers, T routine)
            where T : IValueCoroutineEnumerator
        {
            Handle handler = ConcurrentPool.Rent<Handle>();
            managers.ConcurrentStart(new Coroutine<T>(handler, routine));
            return new ValueCoroutine(handler);
        }

        /// <summary>
        /// Whenever the value coroutine has completed or not.
        /// </summary>
        public bool IsCompleted => handle.IsCompleted(generation);

        /// <summary>
        /// Return self.
        /// </summary>
        /// <returns>Self.</returns>
        public ValueCoroutine GetAwaiter() => this;

        /// <summary>
        /// Determines the action to run after the coroutine has completed or was stopped.
        /// </summary>
        /// <param name="continuation">Action to run.</param>
        public void OnCompleted(Action continuation) => handle.OnCompleted(continuation, generation);
    }
}