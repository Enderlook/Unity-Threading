using Enderlook.Collections.LowLevel;

using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Xml;

using UnityEngine;

namespace Enderlook.Unity.Threading.Coroutines
{
    /// <summary>
    /// Suspend the coroutine execution until the suplied task is completed.
    /// </summary>
    public sealed class WaitForTaskComplete : CustomYieldInstruction
    {
        private static readonly DynamicStack<WaitForTaskComplete> pool = DynamicStack<WaitForTaskComplete>.Create(Wait.POOL_CAPACITY);

        private Task task;

        private WaitForTaskComplete(Task task) => this.task = task;

        public override bool keepWaiting {
            get {
                if (task.IsCompleted)
                {
                    Task task = this.task;
                    this.task = default;
                    if (pool.Count < Wait.POOL_CAPACITY)
                        pool.Push(this);
                    if (task.IsFaulted)
                        ExceptionDispatchInfo.Capture(task.Exception).Throw();
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Suspend the coroutine execution until the suplied task is completed.<br/>
        /// Object is drawn from a pool.
        /// </summary>
        /// <param name="task">Task to wait.</param>
        /// <returns>A waiter.</returns>
        internal static WaitForTaskComplete Create(Task task)
        {
            if (pool.TryPop(out WaitForTaskComplete result))
            {
                result.task = task;
                return result;
            }
            return new WaitForTaskComplete(task);
        }

        /// <inheritdoc cref="Create(Task)"/>
        public static implicit operator WaitForTaskComplete(Task task)
            => Create(task);
    }
}
