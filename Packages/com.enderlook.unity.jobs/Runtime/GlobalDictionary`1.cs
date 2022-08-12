using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using UnityEngine;

namespace Enderlook.Unity.Jobs
{
    internal static class GlobalDictionary<T>
    {
        private static readonly ConcurrentDictionary<long, T> dictionary = new ConcurrentDictionary<long, T>();

        private static long counter;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long Store(T value)
        {
            long v = Interlocked.Increment(ref counter);
            if (!dictionary.TryAdd(v, value))
                Debug.LogError("Impossible state.");
            return v;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Drain(long key)
        {
            if (!dictionary.TryRemove(key, out T value))
                ThrowRequestedMoreThanOnce();
            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Remove(long key)
        {
            if (dictionary.TryRemove(key, out T _))
                ThrowRequestedMoreThanOnce();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Peek(long key)
        {
            if (!dictionary.TryGetValue(key, out T value))
                ThrowAlreadyDrain();
            return value;

            void ThrowAlreadyDrain() => throw new KeyNotFoundException("A key was already drained");
        }

        private static void ThrowRequestedMoreThanOnce() => throw new KeyNotFoundException("A key can't be requested more than once.");
    }
}