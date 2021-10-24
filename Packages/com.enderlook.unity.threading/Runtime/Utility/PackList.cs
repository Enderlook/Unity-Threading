using Enderlook.Collections.LowLevel;

using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

using UnityEngine;

namespace Enderlook.Unity.Threading
{
    internal struct PackList<T>
    {
        // This can't be a property because the struct would be copied instead of being mutated.
        // Instead if this is a field, mutations would be on the reference.
        public RawList<T> List;

        public ConcurrentBag<T> Concurrent { get; private set; }

        public int Count => List.Count + Concurrent.Count;

        public static PackList<T> Create() => new PackList<T>()
        {
            List = RawList<T>.Create(),
            Concurrent = new ConcurrentBag<T>(),
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RawList<T> Swap(RawList<T> list)
        {
            // This is not atomic. However it does't matter since `List` and `Swap` are only used in the main thread.
            RawList<T> old = List;
            List = list;
            return old;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(T element)
        {
#if DEBUG
            if (!UnityThread.IsMainThread)
                Debug.LogError("This function can only be executed in the Unity thread. This has produced undefined behaviour.");
#endif
            List.Add(element);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ConcurrentAdd(T element)
        {
#if DEBUG
#if UNITY_WEBGL
            Debug.LogWarning("This platform doesn't support multithreading. This doesn't mean that the function will fail (it works), but it would be more perfomant to call the non-concurrent version instead.");
#else
            if (UnityThread.IsMainThread)
                Debug.LogWarning("This function was executed in the main thread. This is not an error, thought it's more perfomant to call the non-concurrent version instead.");
#endif
            #endif
            Concurrent.Add(element);
        }
    }

    internal static class PackList
    {
        public static void Dispose<T>(this ref PackList<T> pack) where T : IDisposable
        {
            RawList<T> list = pack.List;
            for (int i = 0; i < list.Count; i++)
                list[i].Dispose();
            pack.List.Clear();

            ConcurrentBag<T> bag = pack.Concurrent;
            while (bag.TryTake(out T result))
                result.Dispose();
        }

        public static void Dispose<T, U>(this ref PackList<(U, T)> pack) where T : IDisposable
        {
            RawList<(U, T)> list = pack.List;
            for (int i = 0; i < list.Count; i++)
                list[i].Item2.Dispose();
            pack.List.Clear();

            ConcurrentBag<(U, T)> bag = pack.Concurrent;
            while (bag.TryTake(out (U, T) result))
                result.Item2.Dispose();
        }
    }
}