using Enderlook.Pools;

using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    /// <summary>
    /// Suspend the coroutine execution until the supplied task is completed.
    /// </summary>
    public sealed class WaitForValueTaskComplete : CustomYieldInstruction
    {
        private ValueTask task;

        public override bool keepWaiting {
            get {
                if (task.IsCompleted)
                {
                    ValueTask task = this.task;
                    this.task = default;
                    ObjectPool<WaitForValueTaskComplete>.Shared.Return(this);
                    if (task.IsFaulted)
                        ExceptionDispatchInfo.Capture(task.AsTask().Exception).Throw();
                    return false;
                }
                return true;
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Unity Editor Only.
        /// </summary>
        internal static int Count => ObjectPool<WaitForValueTaskComplete>.Shared.ApproximateCount();
#endif

        /// <summary>
        /// Suspend the coroutine execution until the supplied task is completed.<br/>
        /// Object is drawn from a pool.
        /// </summary>
        /// <param name="task">Task to wait.</param>
        /// <returns>A waiter.</returns>
        internal static WaitForValueTaskComplete Create(ValueTask task)
        {
            WaitForValueTaskComplete waiter = ObjectPool<WaitForValueTaskComplete>.Shared.Rent();
            waiter.task = task;
            return waiter;
        }

        /// <inheritdoc cref="Create(ValueTask)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator WaitForValueTaskComplete(ValueTask task) => Create(task);
    }
}
