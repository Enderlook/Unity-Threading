using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Enderlook.Unity
{
    internal static class ConcurrentPool<T> where T : class, new()
    {
        // Not use ConcurrentStack because it produces a runtime error in WebGL even if not used.

        private static readonly Stack<T> pool = new Stack<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Rent()
        {
            lock (pool)
            {
                if (pool.TryPop(out T result))
                    return result;
                return new T();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return(T value)
        {
            lock (pool)
            {
                pool.Push(value);
            }
        }
    }
}