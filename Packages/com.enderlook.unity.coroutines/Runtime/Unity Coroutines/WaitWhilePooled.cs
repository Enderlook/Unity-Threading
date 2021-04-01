using Enderlook.Collections.LowLevel;

using System;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    /// <summary>
    /// Suspend the coroutine execution until the supplied delegate evaluates to <see langword="true"/>.
    /// </summary>
    public sealed class WaitWhilePooled : CustomYieldInstruction
    {
        private static RawStack<WaitWhilePooled> pool = RawStack<WaitWhilePooled>.Create(Wait.POOL_CAPACITY);

        private Func<bool> predicate;

        /// <inheritdoc cref="CustomYieldInstruction.keepWaiting"/>
        public override bool keepWaiting {
            get {
                if (!predicate())
                {
                    predicate = null;
                    if (pool.Count < Wait.POOL_CAPACITY)
                        pool.Push(this);
                    return false;
                }
                return true;
            }
        }

        private WaitWhilePooled(Func<bool> predicate) => this.predicate = predicate;

        internal static void Clear() => pool.Clear();

#if UNITY_EDITOR
        /// <summary>
        /// Unity Editor Only.
        /// </summary>
        internal static int Count => pool.Count;
#endif

        /// <summary>
        /// Suspend the coroutine execution until the supplied delegate evaluates to <see langword="false"/>.<br/>
        /// Object is drawn from a pool.
        /// </summary>
        /// <param name="predicate">Delegate to evaluate.</param>
        /// <returns>A waiter.</returns>
        internal static WaitWhilePooled Create(Func<bool> predicate)
        {
            if (pool.TryPop(out WaitWhilePooled result))
            {
                result.predicate = predicate;
                return result;
            }
            return new WaitWhilePooled(predicate);
        }
    }
}
