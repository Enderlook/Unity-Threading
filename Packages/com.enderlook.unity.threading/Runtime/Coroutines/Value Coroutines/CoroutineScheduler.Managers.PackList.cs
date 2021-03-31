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
            private struct PackList<T>
            {
                public RawList<T> List { get; private set; }
                private ConcurrentBag<T> bag;

                public ConcurrentBag<T> Concurrent => bag;

                public static PackList<T> Create()
                {
                    PackList<T> packList = new PackList<T>() {
                        List = RawList<T>.Create()
                    };
                    if (Application.platform != RuntimePlatform.WebGLPlayer)
                        packList.bag = new ConcurrentBag<T>();
                    return packList;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public RawList<T> Swap(RawList<T> list)
                {
                    // This is not atomic. However it does't matter since `List` and `Swap` are only used in the main thread.
                    RawList<T> old = list;
                    List = list;
                    return old;
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Add(T element) => List.Add(element);

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void AddConcurrent(T element) => Concurrent.Add(element);
            }

            private static void Dispose<T>(ref PackList<T> pack) where T : IDisposable
            {
                RawList<T> list = pack.List;
                for (int i = 0; i < list.Count; i++)
                    list[i].Dispose();
                pack.List.Clear();

                ConcurrentBag<T> bag = pack.Concurrent;
                if (!(bag is null))
                {
                    while (bag.TryTake(out T result))
                        result.Dispose();
                }
            }

            private static void Dispose<T, U>(ref PackList<(U, T)> pack) where T : IDisposable
            {
                RawList<(U, T)> list = pack.List;
                for (int i = 0; i < list.Count; i++)
                    list[i].Item2.Dispose();
                pack.List.Clear();

                ConcurrentBag<(U, T)> bag = pack.Concurrent;
                if (!(bag is null))
                {
                    while (bag.TryTake(out (U, T) result))
                        result.Item2.Dispose();
                }
            }
        }
    }
}