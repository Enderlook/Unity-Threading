using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using UnityEngine;

namespace Enderlook.Unity.Threading.Coroutines
{
    /// <summary>
    /// Provides a pool of waiters for coroutines.
    /// </summary>
    public static class Wait
    {
        // https://github.com/svermeulen/Unity3dAsyncAwaitUtil/blob/master/UnityProject/Assets/Plugins/AsyncAwaitUtil/Source/Awaiters.cs

        public static readonly WaitForEndOfFrame ForEndOfFrame = new WaitForEndOfFrame();

        public static readonly WaitForFixedUpdate ForFixedUpdate = new WaitForFixedUpdate();

        public static readonly WaitforUpdate ForUpdate = new WaitforUpdate();

        private static readonly Dictionary<float, WaitForSeconds> waitForSeconds = new Dictionary<float, WaitForSeconds>();

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

        public static WaitForSecondsRealtime ForSecondsRealTime(float seconds)
        {
            if (!waitForSecondsRealTime.TryGetValue(seconds, out WaitForSecondsRealtime wait))
            {
                wait = new WaitForSecondsRealtime(seconds);
                waitForSecondsRealTime[seconds] = wait;
            }
            return wait;
        }

        // TODO: shall we pool WaitUntil and WaitWhile?

        private static readonly ConditionalWeakTable<Func<bool>, WaitUntil> waitUntils = new ConditionalWeakTable<Func<bool>, WaitUntil>();

        private static readonly ConditionalWeakTable<Func<bool>, WaitWhile> waitWhiles = new ConditionalWeakTable<Func<bool>, WaitWhile>();

        public static WaitUntil Until(Func<bool> predicate)
        {
            if (waitUntils.TryGetValue(predicate, out WaitUntil value))
                return value;
            value = new WaitUntil(predicate);
            waitUntils.Add(predicate, value);
            return value;
        }

        public static WaitWhile While(Func<bool> predicate)
        {
            if (waitWhiles.TryGetValue(predicate, out WaitWhile value))
                return value;
            value = new WaitWhile(predicate);
            waitWhiles.Add(predicate, value);
            return value;
        }
    }
}
