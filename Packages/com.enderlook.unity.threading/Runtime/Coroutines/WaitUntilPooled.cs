using Enderlook.Collections.LowLevel;

using System;

using UnityEngine;

namespace Enderlook.Unity.Threading.Coroutines
{
    /// <summary>
    /// Suspend the coroutine execution until the supplied delegate evaluates to <see langword="true"/>.
    /// </summary>
    public sealed class WaitUntilPooled : CustomYieldInstruction
    {
        private static readonly RawStack<WaitUntilPooled> pool = RawStack<WaitUntilPooled>.Create(Wait.POOL_CAPACITY);

        private Func<bool> predicate;

        /// <inheritdoc cref="CustomYieldInstruction.keepWaiting"/>
        public override bool keepWaiting {
            get {
                if (predicate())
                {
                    predicate = null;
                    if (pool.Count < Wait.POOL_CAPACITY)
                        pool.Push(this);
                    return false;
                }
                return true;
            }
        }

        private WaitUntilPooled(Func<bool> predicate) => this.predicate = predicate;

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
        /// Suspend the coroutine execution until the supplied delegate evaluates to <see langword="true"/>.<br/>
        /// Object is drawn from a pool.
        /// </summary>
        /// <param name="predicate">Delegate to evaluate.</param>
        /// <returns>A waiter.</returns>
        internal static WaitUntilPooled Create(Func<bool> predicate)
        {
            if (pool.TryPop(out WaitUntilPooled result))
            {
                result.predicate = predicate;
                return result;
            }
            return new WaitUntilPooled(predicate);
        }
    }
}
