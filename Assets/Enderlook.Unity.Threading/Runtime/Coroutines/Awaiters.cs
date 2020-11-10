using System;
using System.Collections.Generic;

using UnityEngine;

namespace Enderlook.Unity.Threading.Coroutines
{
    /// <summary>
    /// Provides a pool of waiters for coroutines.
    /// </summary>
    public static class Awaiters
    {
        // https://github.com/svermeulen/Unity3dAsyncAwaitUtil/blob/master/UnityProject/Assets/Plugins/AsyncAwaitUtil/Source/Awaiters.cs

        public static WaitForEndOfFrame WaitForEndOfFrame { get; } = new WaitForEndOfFrame();

        public static WaitForFixedUpdate WaitForFixedUpdate { get; } = new WaitForFixedUpdate();

        public static WaitforUpdate WaitForUpdate { get; } = new WaitforUpdate();

        private static readonly Dictionary<float, WaitForSeconds> waitForSeconds = new Dictionary<float, WaitForSeconds>();

        public static WaitForSeconds WaitForSeconds(float seconds)
        {
            if (!waitForSeconds.TryGetValue(seconds, out WaitForSeconds wait))
            {
                wait = new WaitForSeconds(seconds);
                waitForSeconds[seconds] = wait;
            }
            return wait;
        }

        private static readonly Dictionary<float, WaitForSecondsRealtime> waitForSecondsRealTime = new Dictionary<float, WaitForSecondsRealtime>();

        public static WaitForSecondsRealtime WaitForSecondsRealTime(float seconds)
        {
            if (!waitForSecondsRealTime.TryGetValue(seconds, out WaitForSecondsRealtime wait))
            {
                wait = new WaitForSecondsRealtime(seconds);
                waitForSecondsRealTime[seconds] = wait;
            }
            return wait;
        }

        // TODO: shall we pool WaitUntil and WaitWhile?

        public static WaitUntil Until(Func<bool> predicate) => new WaitUntil(predicate);

        public static WaitWhile While(Func<bool> predicate) => new WaitWhile(predicate);
    }
}
