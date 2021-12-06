using Enderlook.Pools;

using System;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    /// <summary>
    /// Suspend the coroutine execution until the supplied delegate evaluates to <see langword="true"/>.
    /// </summary>
    public sealed class WaitUntilPooled : CustomYieldInstruction
    {
        private Func<bool> predicate;

        /// <inheritdoc cref="CustomYieldInstruction.keepWaiting"/>
        public override bool keepWaiting {
            get {
                if (predicate())
                {
                    predicate = null;
                    ObjectPool<WaitUntilPooled>.Shared.Return(this);
                    return false;
                }
                return true;
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Unity Editor Only.
        /// </summary>
        internal static int Count => ObjectPool<WaitUntilPooled>.Shared.ApproximateCount();
#endif

        /// <summary>
        /// Suspend the coroutine execution until the supplied delegate evaluates to <see langword="true"/>.<br/>
        /// Object is drawn from a pool.
        /// </summary>
        /// <param name="predicate">Delegate to evaluate.</param>
        /// <returns>A waiter.</returns>
        internal static WaitUntilPooled Create(Func<bool> predicate)
        {
            WaitUntilPooled waiter = ObjectPool<WaitUntilPooled>.Shared.Rent();
            waiter.predicate = predicate;
            return waiter;
        }
    }
}
