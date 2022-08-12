using Enderlook.Pools;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    /// <summary>
    /// Suspend the coroutine execution until the supplied seconds in realtime has passed.
    /// </summary>
    public sealed class WaitForSecondsRealtimePooled : CustomYieldInstruction
    {
        internal float waitUntil;

        public override bool keepWaiting {
            get {
                if (Time.realtimeSinceStartup >= waitUntil)
                {
                    ObjectPool<WaitForSecondsRealtimePooled>.Shared.Return(this);
                    return false;
                }
                return true;
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Unity Editor Only.
        /// </summary>
        internal static int Count => ObjectPool<WaitForSecondsRealtimePooled>.Shared.ApproximateCount();
#endif

        /// <summary>
        /// Suspend the coroutine execution until the supplied seconds in realtime has passed.<br/>
        /// The instance is draw from a pool.
        /// </summary>
        /// <param name="seconds">Time in seconds to wait.</param>
        /// <returns>A waiter.</returns>
        internal static WaitForSecondsRealtimePooled Create(float seconds)
        {
            WaitForSecondsRealtimePooled waiter = ObjectPool<WaitForSecondsRealtimePooled>.Shared.Rent();
            waiter.waitUntil = seconds + Time.realtimeSinceStartup;
            return waiter;
        }
    }
}
