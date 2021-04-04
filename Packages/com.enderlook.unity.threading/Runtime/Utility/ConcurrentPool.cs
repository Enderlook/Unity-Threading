using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using UnityEngine;

namespace Enderlook.Unity.Threading
{
    internal static class ConcurrentPool
    {
        private static event Action Clear_;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Clear() => Clear_?.Invoke();

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void Initialize() => UnityEditor.EditorApplication.playModeStateChanged += (_) => Clear_?.Invoke();

        private static readonly SortedSet<EditorPoolContainer> pools = new SortedSet<EditorPoolContainer>();
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Rent<T>() where T : class, new() => Pool<T>.Rent();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return<T>(T obj) where T : class, new() => Pool<T>.Return(obj);

        private static class Pool<T> where T : class, new()
        {
            // TODO: In .NET 6 Activator.CreateFactory<T>() may be added https://github.com/dotnet/runtime/issues/36194.

            private static readonly T[] array = new T[ARRAY_LENGTH];
            private const int ARRAY_LENGTH = 100;
            private static int count;

            static Pool()
            {
                Action clear = () =>
                {
                    count = 0;
                    Array.Clear(array, 0, ARRAY_LENGTH);
                };

                Clear_ += clear;

#if UNITY_EDITOR
                pools.Add(new EditorPoolContainer($"{nameof(ConcurrentPool)}.Rent<{typeof(T).Name}>()", () => count, clear));
#endif
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static T Rent()
            {
                while (true)
                {
                    int index = Interlocked.Decrement(ref count);
                    if (index < 0)
                        return new T();
                    else
                    {
                        if (index < ARRAY_LENGTH)
                        {
                            T obj = Interlocked.Exchange(ref array[index], null);
                            if (!(obj is null))
                                return obj;
                        }
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Return(T obj)
            {
                int index = Interlocked.Increment(ref count);
                if (index < 0)
                    array[0] = obj;
                else if (index < ARRAY_LENGTH)
                    array[index] = obj;
            }
        }
    }
}