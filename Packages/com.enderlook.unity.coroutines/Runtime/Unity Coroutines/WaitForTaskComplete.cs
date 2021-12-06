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
    public sealed class WaitForTaskComplete : CustomYieldInstruction
    {
        private Task task;

        public override bool keepWaiting {
            get {
                if (task.IsCompleted)
                {
                    Task task = this.task;
                    this.task = default;
                    ObjectPool<WaitForTaskComplete>.Shared.Return(this);
                    if (task.IsFaulted)
                        ExceptionDispatchInfo.Capture(task.Exception).Throw();
                    return false;
                }
                return true;
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Unity Editor Only.
        /// </summary>
        internal static int Count => ObjectPool<WaitForTaskComplete>.Shared.ApproximateCount();
#endif

        /// <summary>
        /// Suspend the coroutine execution until the supplied task is completed.<br/>
        /// Object is drawn from a pool.
        /// </summary>
        /// <param name="task">Task to wait.</param>
        /// <returns>A waiter.</returns>
        internal static WaitForTaskComplete Create(Task task)
        {
            WaitForTaskComplete waiter = ObjectPool<WaitForTaskComplete>.Shared.Rent();
            waiter.task = task;
            return waiter;
        }

        /// <inheritdoc cref="Create(Task)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator WaitForTaskComplete(Task task) => Create(task);
    }
}
