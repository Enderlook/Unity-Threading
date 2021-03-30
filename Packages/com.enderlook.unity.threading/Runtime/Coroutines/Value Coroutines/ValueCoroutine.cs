﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Enderlook.Unity.Coroutines
{
    /// <summary>
    /// Represent the handler of a value coroutine.
    /// </summary>
    public readonly partial struct ValueCoroutine : INotifyCompletion
    {
        private readonly Handle handle;
        // Since the handle is pooled, we use generation to check if it's already used
        private readonly uint generation;

        private ValueCoroutine(Handle handle)
        {
            this.handle = handle;
            generation = handle.Generation;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ValueCoroutine Start<T, U>(U cancellator, T routine)
            where T : IEnumerator<ValueYieldInstruction>
            where U : ICancellable
        {
            Handle handler = ConcurrentPool<Handle>.Rent();
            CoroutineManagers.Start(cancellator, new Enumerator<T>(handler, routine));
            return new ValueCoroutine(handler);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ValueCoroutine StartThreadSafe<T, U>(U cancellator, T routine)
            where T : IEnumerator<ValueYieldInstruction>
            where U : ICancellable
        {
            Handle handler = ConcurrentPool<Handle>.Rent();
            CoroutineManagers.StartThreadSafe(cancellator, new Enumerator<T>(handler, routine));
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