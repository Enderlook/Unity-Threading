using Enderlook.Collections.LowLevel;

using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    public partial struct CoroutineScheduler
    {
        internal partial class Managers
        {
            private struct PackQueue<T>
            {
                public RawQueue<T> Queue { get; set; }
                private ConcurrentQueue<T> queue;

                public ConcurrentQueue<T> Concurrent => queue;

                public static PackQueue<T> Create()
                {
                    PackQueue<T> packQueue = new PackQueue<T>()
                    {
                        Queue = RawQueue<T>.Create()
                    };
                    if (Application.platform == RuntimePlatform.WebGLPlayer)
                        packQueue.queue = new ConcurrentQueue<T>();
                    return packQueue;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public RawQueue<T> Swap(RawQueue<T> queue)
                {
                    // This is not atomic. However it does't matter since `Queue` and `Swap` are only used in the main thread.
                    RawQueue<T> old = queue;
                    Queue = queue;
                    return old;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void DrainConcurrent()
                {
                    ConcurrentQueue<T> queue = this.queue;
                    if (!(queue is null))
                    {
                        RawQueue<T> local = Queue;
                        while (queue.TryDequeue(out T routine))
                            local.Enqueue(routine);
                        Queue = local;
                    }
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Enqueue(T element) => Queue.Enqueue(element);

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void EnqueueConcurrent(T element) => Concurrent.Enqueue(element);
            }

            private static void Dispose<T>(ref PackQueue<T> pack) where T : IDisposable
            {
                RawQueue<T> queue = pack.Queue;
                while (queue.TryDequeue(out T result))
                    result.Dispose();
                pack.Queue = queue;

                ConcurrentQueue<T> concurrentQueue = pack.Concurrent;
                if (!(concurrentQueue is null))
                {
                    while (concurrentQueue.TryDequeue(out T result))
                        result.Dispose();
                }
            }
        }
    }
}