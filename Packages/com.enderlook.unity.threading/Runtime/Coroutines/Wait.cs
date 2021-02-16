using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Unity.Jobs;

using UnityEngine;

namespace Enderlook.Unity.Threading.Coroutines
{
    /// <summary>
    /// Provides a pool of waiters for coroutines.
    /// </summary>
    public static class Wait
    {
        internal const int POOL_CAPACITY = 100;

        // https://github.com/svermeulen/Unity3dAsyncAwaitUtil/blob/master/UnityProject/Assets/Plugins/AsyncAwaitUtil/Source/Awaiters.cs

        /// <inheritdoc cref="WaitForEndOfFrame"/>
        public static readonly WaitForEndOfFrame ForEndOfFrame = new WaitForEndOfFrame();

        /// <inheritdoc cref="WaitForFixedUpdate"/>
        public static readonly WaitForFixedUpdate ForFixedUpdate = new WaitForFixedUpdate();

        /// <inheritdoc cref="WaitForUpdate"/>
        public static readonly WaitForUpdate ForUpdate = new WaitForUpdate();

        private static readonly Dictionary<float, WaitForSeconds> waitForSeconds = new Dictionary<float, WaitForSeconds>();

        /// <inheritdoc cref="WaitForSeconds"/>
        public static WaitForSeconds ForSeconds(float seconds)
        {
            if (!waitForSeconds.TryGetValue(seconds, out WaitForSeconds wait))
            {
                wait = new WaitForSeconds(seconds);
                waitForSeconds[seconds] = wait;
            }
            return wait;
        }

        private static readonly Dictionary<float, WaitForSecondsRealtime> waitForSecondsRealTime = new Dictionary<float, WaitForSecondsRealtime>();

        /// <inheritdoc cref="WaitForSecondsRealtime"/>
        public static WaitForSecondsRealtime ForSecondsRealTime(float seconds)
        {
            if (!waitForSecondsRealTime.TryGetValue(seconds, out WaitForSecondsRealtime wait))
            {
                wait = new WaitForSecondsRealtime(seconds);
                waitForSecondsRealTime[seconds] = wait;
            }
            return wait;
        }

        // TODO: shall we pool them?

        /// <inheritdoc cref="WaitUntilPooled.Create(Func{bool})"/>
        public static WaitUntilPooled Until(Func<bool> predicate)
            => WaitUntilPooled.Create(predicate);

        /// <inheritdoc cref="WaitWhile.Create(Func{bool})"/>
        public static WaitWhilePooled While(Func<bool> predicate)
            => WaitWhilePooled.Create(predicate);

        /// <inheritdoc cref="WaitForJobComplete.Create(JobHandle)"/>
        public static WaitForJobComplete For(JobHandle handle)
            => WaitForJobComplete.Create(handle);

        /// <inheritdoc cref="WaitForTaskComplete.Create(Task)"/>
        public static WaitForTaskComplete For(Task task)
            => WaitForTaskComplete.Create(task);

        /// <inheritdoc cref="WaitForTaskComplete{T}.Create(Task{T})"/>
        public static WaitForTaskComplete<T> For<T>(Task<T> task)
            => WaitForTaskComplete<T>.Create(task);

        /// <inheritdoc cref="WaitForValueTaskComplete.Create(ValueTask)"/>
        public static WaitForValueTaskComplete For(ValueTask task)
            => WaitForValueTaskComplete.Create(task);

        /// <inheritdoc cref="WaitForValueTaskComplete{T}.Create(ValueTask{T})"/>
        public static WaitForValueTaskComplete<T> For<T>(ValueTask<T> task)
            => WaitForValueTaskComplete<T>.Create(task);
    }
}
