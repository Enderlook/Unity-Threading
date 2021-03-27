using Enderlook.Collections.LowLevel;

using UnityEngine;

namespace Enderlook.Unity.Threading.Coroutines
{
    /// <summary>
    /// Suspend the coroutine execution until the supplied seconds in realtime has passed.
    /// </summary>
    public sealed class WaitForSecondsRealtimePooled : CustomYieldInstruction
    {
        private static readonly RawStack<WaitForSecondsRealtimePooled> pool = RawStack<WaitForSecondsRealtimePooled>.Create(Wait.POOL_CAPACITY);

        private float waitUntil;

        private WaitForSecondsRealtimePooled(float waitFor) => waitUntil = waitFor + Time.realtimeSinceStartup;

        public override bool keepWaiting {
            get {
                if (Time.realtimeSinceStartup >= waitUntil)
                {
                    if (pool.Count < Wait.POOL_CAPACITY)
                        pool.Push(this);
                    return false;
                }
                return true;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        internal static void Clear() => pool.Clear();

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void Initialize() => UnityEditor.EditorApplication.playModeStateChanged += (_) => Clear();

        /// <summary>
        /// Unity Editor Only.
        /// </summary>
        internal static int Count => pool.Count;
#endif

        /// <summary>
        /// Suspend the coroutine execution until the supplied seconds in realtime has passed.<br/<
        /// The instance is draw from a pool.
        /// </summary>
        /// <param name="seconds">Time in seconds to wait.</param>
        /// <returns>A waiter.</returns>
        internal static WaitForSecondsRealtimePooled Create(float seconds)
        {
            if (pool.TryPop(out WaitForSecondsRealtimePooled result))
            {
                result.waitUntil = seconds + Time.realtimeSinceStartup;
                return result;
            }
            return new WaitForSecondsRealtimePooled(seconds);
        }
    }
}
