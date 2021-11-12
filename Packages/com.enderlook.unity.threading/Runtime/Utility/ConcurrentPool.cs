using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;

using UnityEngine;

namespace Enderlook.Unity.Threading
{
    internal static class ConcurrentPool
    {
        private static readonly int count = Environment.ProcessorCount * 2;

        private static readonly ParameterExpression[] emptyParameter;

        private static event Action Clear_;

        static ConcurrentPool()
        {
            try
            {
                // Check if lambda compiling works.
                Expression.Lambda<Func<object>>(Expression.New(typeof(object)), emptyParameter).Compile();
                emptyParameter = new ParameterExpression[0];
            }
            catch { }

            _ = new GCCallback();
        }

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

            private static readonly Func<T> creator;
            private static readonly InvariantObject[] array = new InvariantObject[count];
            private static T firstElement;

            static Pool()
            {
                Action clear = () =>
                {
                    firstElement = null;
                    Array.Clear(array, 0, array.Length);
                };

                Clear_ += clear;

                if (!(emptyParameter is null))
                    creator = Expression.Lambda<Func<T>>(Expression.New(typeof(T)), emptyParameter).Compile();

#if UNITY_EDITOR
                pools.Add(new EditorPoolContainer($"{nameof(ConcurrentPool)}.Rent<{typeof(T).Name}>()", () => {
                    int i = firstElement != null ? 1 : 0;
                    foreach (InvariantObject e in array)
                        if (e.Value != null)
                            i++;
                    return i;
                }, clear));
#endif
            }

            public static T Rent()
            {
                T element = firstElement; // Intentionally not using Interlocked.
                if (element == null || element != Interlocked.CompareExchange(ref firstElement, null, element))
                    return Slow();
                return element;
            }

            private static T Slow()
            {
                InvariantObject[] items = array;

                for (int i = 0; i < items.Length; i++)
                {
                    T element = items[i].Value; // Intentionally not using Interlocked.
                    if (element != null && element == Interlocked.CompareExchange(ref items[i].Value, null, element))
                        return element;
                }

                return Create();
            }

            private static T Create() => !(emptyParameter is null) ? creator() : (T)Activator.CreateInstance(typeof(T));

            public static void Return(T obj)
            {
                if (firstElement == null)
                    firstElement = obj; // Intentionally not using Interlocked.
                else
                    Slow();

                void Slow()
                {
                    InvariantObject[] items = array;
                    for (int i = 0; i < items.Length; i++)
                    {
                        if (items[i].Value == null)
                        {
                            // Intentionally not using Interlocked.
                            items[i].Value = obj;
                            break;
                        }
                    }
                }
            }

            private struct InvariantObject // Prevent runtime covariant checks.
            {
                public T Value;
            }
        }

        private sealed class GCCallback
        {
            ~GCCallback()
            {
                Clear();
                GC.ReRegisterForFinalize(this);
            }
        }
    }
}