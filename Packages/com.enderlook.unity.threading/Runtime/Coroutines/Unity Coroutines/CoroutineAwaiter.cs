using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Enderlook.Unity.Coroutines
{
    /// <summary>
    /// An awaiter for <see cref="UnityEngine.Coroutine"/>
    /// </summary>
    public readonly struct CoroutineAwaiter : INotifyCompletion
    {
        private readonly Handle handle;
        private readonly uint generation;

        private class Handle
        {
            public Action onCompleted;

            public bool isCompleted;

            public uint generation;
        }

        /// <summary>
        /// Creates an awaiter for the given <see cref="JobHandle"/>.
        /// </summary>
        /// <param name="coroutine">Job handle to await for.</param>
        public CoroutineAwaiter(UnityEngine.Coroutine coroutine)
        {
            Handle handle = ConcurrentPool.Rent<Handle>();
            this.handle = handle;
            generation = handle.generation;
            Coroutine.Unity.Start(Work());
            IEnumerator Work()
            {
                yield return coroutine;
                handle.isCompleted = true;
                handle.onCompleted?.Invoke();
                handle.generation++;
                ConcurrentPool.Return(handle);
            }
        }

        /// <summary>
        /// Return self.
        /// </summary>
        /// <returns>Self.</returns>
        public CoroutineAwaiter GetAwaiter() => this;

        /// <summary>
        /// Whenever the job handle has completed or not.
        /// </summary>
        public bool IsCompleted => generation != handle.generation || handle.isCompleted;

        /// <summary>
        /// Determines the action to run after the job handle has completed.
        /// </summary>
        /// <param name="continuation">Action to run.</param>
        public void OnCompleted(Action continuation)
        {
            if (generation != handle.generation)
                continuation();
            if (IsCompleted)
                continuation();
            else
                handle.onCompleted += continuation;
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public void GetResult() { }
    }
}