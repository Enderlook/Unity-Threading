using Enderlook.Pools;
using Enderlook.Unity.Threading;

using System;
using System.Threading;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    public readonly partial struct ValueCoroutine
    {
        internal class Handle
        {
            private Action continuation;
            private int isAdding;
            private bool isCompleted;

            public uint Generation { get; private set; }

            public void Complete()
            {
#if UNITY_EDITOR
                Debug.Assert(!isCompleted);
#endif
                isCompleted = true;

                while (isAdding == 1) ;

                Action action = continuation;
                if (!(action is null))
                {
                    // Complete can be called outside main thread, so we must to ensure we are in the appropiate thread.
                    if (UnityThread.IsMainThread)
                        action();
                    else
                        UnityThread.RunLater(action);
                }

                Generation++;
                ObjectPool<Handle>.Shared.Return(this);
            }

            public bool IsCompleted(uint generation)
            {
                bool isCompleted = this.isCompleted;
                if (Generation != generation)
                    return true;
                return isCompleted;
            }

            public void OnCompleted(Action continuation, uint generation)
            {
                if (Generation != generation)
                {
                    Run();
                    return;
                }

                while (Interlocked.Exchange(ref isAdding, 1) == 0) ;

                if (Generation != generation)
                {
                    Run();
                    return;
                }

                if (isCompleted)
                {
                    Interlocked.Exchange(ref isAdding, 0);
                    Run();
                    return;
                }

                try
                {
                    this.continuation += continuation;
                }
                finally
                {
                    Interlocked.Exchange(ref isAdding, 0);
                }

                void Run()
                {
                    // Complete can be called outside main thread, so we must to ensure we are in the appropiate thread.
                    if (UnityThread.IsMainThread)
                        continuation();
                    else
                        UnityThread.RunLater(continuation);
                }
            }
        }
    }
}