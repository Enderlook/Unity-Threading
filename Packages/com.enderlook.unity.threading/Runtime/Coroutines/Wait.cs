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

        /// <inheritdoc cref="WaitUntil"/>
        public static WaitUntil Until(Func<bool> predicate)
            => new WaitUntil(predicate);

        /// <inheritdoc cref="WaitWhile"/>
        public static WaitWhile While(Func<bool> predicate)
            => new WaitWhile(predicate);

        /// <inheritdoc cref="WaitForJobComplete"/>
        public static WaitForJobComplete For(JobHandle handle)
            => new WaitForJobComplete(handle);

        /// <inheritdoc cref="WaitForTaskComplete"/>
        public static WaitForTaskComplete For(Task task)
            => new WaitForTaskComplete(task);

        /// <inheritdoc cref="WaitForTaskComplete{T}"/>
        public static WaitForTaskComplete<T> For<T>(Task<T> task)
            => new WaitForTaskComplete<T>(task);

        /// <inheritdoc cref="WaitForValueTaskComplete"/>
        public static WaitForValueTaskComplete For(ValueTask task)
            => new WaitForValueTaskComplete(task);

        /// <inheritdoc cref="WaitForValueTaskComplete{T}"/>
        public static WaitForValueTaskComplete<T> For<T>(ValueTask<T> task)
            => new WaitForValueTaskComplete<T>(task);
    }
}
