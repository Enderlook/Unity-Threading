using Enderlook.Collections.LowLevel;

using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

using UnityEngine;

namespace Enderlook.Unity.Threading
{
    internal struct PackQueue<T>
    {
        // This can't be a property because the struct would be copied instead of being mutated.
        // Instead if this is a field, mutations would be on the reference.
        public RawQueue<T> Queue;

        public ConcurrentQueue<T> Concurrent { get; private set; }

        public int Count => Queue.Count + Concurrent.Count;

        public static PackQueue<T> Create() => new PackQueue<T>()
        {
            Queue = RawQueue<T>.Create(),
            Concurrent = new ConcurrentQueue<T>(),
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RawQueue<T> Swap(RawQueue<T> queue)
        {
            // This is not atomic. However it does't matter since `Queue` and `Swap` are only used in the main thread.
            RawQueue<T> old = Queue;
            Queue = queue;
            return old;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DrainConcurrent()
        {
            ConcurrentQueue<T> queue = Concurrent;
            RawQueue<T> local = Queue;
            while (queue.TryDequeue(out T routine))
                local.Enqueue(routine);
            Queue = local;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(T element)
        {
#if DEBUG
            if (!UnityThread.IsMainThread)
                Debug.LogError("This function can only be executed in the Unity thread. This has produced undefined behaviour. This error will not shown on release.");
#endif
            Queue.Enqueue(element);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ConcurrentEnqueue(T element)
        {
#if DEBUG
#if UNITY_WEBGL
            Debug.LogWarning("This platform doesn't support multithreading. This doesn't mean that the function will fail (it works), but it would be more perfomant to call the non-concurrent version instead.");
#else
            if (UnityThread.IsMainThread)
                Debug.LogWarning("This function was executed in the main thread. This is not an error, thought it's more perfomant to call the non-concurrent version instead.");
#endif
#endif
            Concurrent.Enqueue(element);
        }
    }

    internal static class PackQueue
    {
        public static void Dispose<T>(this ref PackQueue<T> pack) where T : IDisposable
        {
            RawQueue<T> queue = pack.Queue;
            while (queue.TryDequeue(out T result))
                result.Dispose();
            pack.Queue = queue;

            ConcurrentQueue<T> concurrentQueue = pack.Concurrent;
            while (concurrentQueue.TryDequeue(out T result))
                result.Dispose();
        }
    }
}