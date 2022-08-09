using Enderlook.Unity.Threading;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Unity.Jobs;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    /// <summary>
    /// Provides a pool of waiters for Unity coroutines.
    /// </summary>
    public static class Wait
    {
        // https://github.com/svermeulen/Unity3dAsyncAwaitUtil/blob/master/UnityProject/Assets/Plugins/AsyncAwaitUtil/Source/Awaiters.cs

        /// <inheritdoc cref="WaitForEndOfFrame"/>
        public static readonly WaitForEndOfFrame ForEndOfFrame = new WaitForEndOfFrame();

        /// <inheritdoc cref="WaitForFixedUpdate"/>
        public static readonly WaitForFixedUpdate ForFixedUpdate = new WaitForFixedUpdate();

        /// <inheritdoc cref="WaitForUpdate"/>
        public static readonly WaitForUpdate ForUpdate = new WaitForUpdate();

        private static readonly Dictionary<float, WaitForSeconds> waitForSeconds = new Dictionary<float, WaitForSeconds>();
        private static bool requestClear;
        private static float timeToNextClear;

        /// <inheritdoc cref="WaitForSeconds"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WaitForSeconds ForSeconds(float seconds)
        {
            if (!waitForSeconds.TryGetValue(seconds, out WaitForSeconds wait))
                return ForSecondsSlowPath(seconds);
            return wait;
        }

        static Wait()
        {
            Application.lowMemory += () => waitForSeconds.Clear();
            Manager.OnUpdate += () => {
                float realtimeSinceStartup = Time.realtimeSinceStartup;
                if (requestClear && realtimeSinceStartup > timeToNextClear)
                {
                    requestClear = false;
                    timeToNextClear = realtimeSinceStartup + 30;
                    waitForSeconds.Clear();
                }
            };
            GCCallback _ = new GCCallback();
        }

        private sealed class GCCallback
        {
            private readonly StrongBox<bool> isQuitting;

            public GCCallback()
            {
                StrongBox<bool> isQuitting = new StrongBox<bool>();
                Application.quitting += () => isQuitting.Value = true;
                this.isQuitting = isQuitting;
            }

            ~GCCallback()
            {
                requestClear = true;
                if (!isQuitting.Value)
                    GC.ReRegisterForFinalize(this);
            }
        }

        private static WaitForSeconds ForSecondsSlowPath(float seconds)
        {
            WaitForSeconds wait = new WaitForSeconds(seconds);
            waitForSeconds[seconds] = wait;
            return wait;
        }

        // TODO: shall we pool them?

        /// <inheritdoc cref="WaitUntilPooled.Create(Func{bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WaitUntilPooled Until(Func<bool> predicate)
            => WaitUntilPooled.Create(predicate);

        /// <inheritdoc cref="WaitWhile.Create(Func{bool})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WaitWhilePooled While(Func<bool> predicate)
            => WaitWhilePooled.Create(predicate);

        /// <inheritdoc cref="WaitForJobComplete.Create(JobHandle)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WaitForJobComplete For(JobHandle handle)
            => WaitForJobComplete.Create(handle);

        /// <inheritdoc cref="WaitForTaskComplete.Create(Task)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WaitForTaskComplete For(Task task)
            => WaitForTaskComplete.Create(task);

        /// <inheritdoc cref="WaitForTaskComplete{T}.Create(Task{T})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WaitForTaskComplete<T> For<T>(Task<T> task)
            => WaitForTaskComplete<T>.Create(task);

        /// <inheritdoc cref="WaitForValueTaskComplete.Create(ValueTask)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WaitForValueTaskComplete For(ValueTask task)
            => WaitForValueTaskComplete.Create(task);

        /// <inheritdoc cref="WaitForValueTaskComplete{T}.Create(ValueTask{T})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WaitForValueTaskComplete<T> For<T>(ValueTask<T> task)
            => WaitForValueTaskComplete<T>.Create(task);

        /// <inheritdoc cref="WaitForSecondsRealtimePooled.Create(float)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WaitForSecondsRealtimePooled ForRealtime(float seconds)
            => WaitForSecondsRealtimePooled.Create(seconds);

#if UNITY_EDITOR
        /// <summary>
        /// Unity Editor Only.
        /// </summary>
        internal static int ForSecondsCount => waitForSeconds.Count;

        /// <summary>
        /// Unity Editor Only.
        /// </summary>
        internal static void ForSecondsClear() => waitForSeconds.Clear();

        private static readonly SortedSet<EditorPoolContainer> waitForTaskComplete = new SortedSet<EditorPoolContainer>();

        /// <summary>
        /// Unity Editor Only.
        /// </summary>
        internal static IReadOnlyCollection<EditorPoolContainer> ForTaskAndValueTaskComplete => waitForTaskComplete;

        /// <summary>
        /// Unity Editor Only.
        /// </summary>
        internal static void AddWaitForTaskComplete(string name, Func<int> count)
            => waitForTaskComplete.Add(new EditorPoolContainer(name, count));
#endif
    }
}