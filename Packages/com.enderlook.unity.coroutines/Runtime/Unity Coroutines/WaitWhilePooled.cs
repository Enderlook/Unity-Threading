using Enderlook.Collections.LowLevel;
using Enderlook.Pools;

using System;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    /// <summary>
    /// Suspend the coroutine execution until the supplied delegate evaluates to <see langword="true"/>.
    /// </summary>
    public sealed class WaitWhilePooled : CustomYieldInstruction
    {
        private Func<bool> predicate;

        /// <inheritdoc cref="CustomYieldInstruction.keepWaiting"/>
        public override bool keepWaiting {
            get {
                if (!predicate())
                {
                    predicate = null;
                    ObjectPool<WaitWhilePooled>.Shared.Return(this);
                    return false;
                }
                return true;
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Unity Editor Only.
        /// </summary>
        internal static int Count => ObjectPool<WaitWhilePooled>.Shared.ApproximateCount();
#endif

        /// <summary>
        /// Suspend the coroutine execution until the supplied delegate evaluates to <see langword="false"/>.<br/>
        /// Object is drawn from a pool.
        /// </summary>
        /// <param name="predicate">Delegate to evaluate.</param>
        /// <returns>A waiter.</returns>
        internal static WaitWhilePooled Create(Func<bool> predicate)
        {
            WaitWhilePooled waiter = ObjectPool<WaitWhilePooled>.Shared.Rent();
            waiter.predicate = predicate;
            return waiter;
        }
    }
}
