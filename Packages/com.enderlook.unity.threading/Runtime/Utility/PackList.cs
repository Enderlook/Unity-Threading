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

#if !UNITY_WEBGL
        public ConcurrentBag<T> Concurrent { get; private set; }
#endif

#if UNITY_WEBGL
        public int Count => List.Count;
#else
        public int Count => List.Count + Concurrent.Count;
#endif

        public static PackList<T> Create() => new PackList<T>()
        {
            List = RawList<T>.Create(),
#if !UNITY_WEBGL
            Concurrent = new ConcurrentBag<T>(),
#endif
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

#if !UNITY_WEBGL
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ConcurrentAdd(T element)
        {
#if DEBUG
            if (UnityThread.IsMainThread)
                Debug.LogWarning("This function was executed in the main thread. This is not an error, thought it's more perfomant to call the non-concurrent version instead.");
#endif
            Concurrent.Add(element);
        }
#endif
}

    internal static class PackList
    {
        public static void Dispose<T>(this ref PackList<T> pack) where T : IDisposable
        {
            RawList<T> list = pack.List;
            for (int i = 0; i < list.Count; i++)
                list[i].Dispose();
            pack.List.Clear();

#if !UNITY_WEBGL
            ConcurrentBag<T> bag = pack.Concurrent;
            while (bag.TryTake(out T result))
                result.Dispose();
#endif
        }

        public static void Dispose<T, U>(this ref PackList<(U, T)> pack) where T : IDisposable
        {
            RawList<(U, T)> list = pack.List;
            for (int i = 0; i < list.Count; i++)
                list[i].Item2.Dispose();
            pack.List.Clear();

#if !UNITY_WEBGL
            ConcurrentBag<(U, T)> bag = pack.Concurrent;
            while (bag.TryTake(out (U, T) result))
                result.Item2.Dispose();
#endif
        }
    }
}