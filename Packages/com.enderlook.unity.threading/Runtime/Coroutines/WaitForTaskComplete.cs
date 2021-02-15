using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

using UnityEngine;

namespace Enderlook.Unity.Threading.Coroutines
{
    /// <summary>
    /// Wait until the task is completed.
    /// </summary>
    public sealed class WaitForTaskComplete : CustomYieldInstruction
    {
        private readonly Task task;

        /// <summary>
        /// Wait for the task to complete.
        /// </summary>
        /// <param name="task">Task to wait.</param>
        public WaitForTaskComplete(Task task) => this.task = task;

        public override bool keepWaiting {
            get {
                if (task.IsCompleted)
                {
                    if (task.IsFaulted)
                        ExceptionDispatchInfo.Capture(task.Exception).Throw();
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Wrap a <see cref="Task"/>.
        /// </summary>
        /// <param name="task">Handle to wrap.</param>
        public static implicit operator WaitForTaskComplete(Task task)
            => new WaitForTaskComplete(task);
    }
}
