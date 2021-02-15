using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

using UnityEngine;

namespace Enderlook.Unity.Threading.Coroutines
{
    /// <summary>
    /// Wait until the task is completed.
    /// </summary>
    public sealed class WaitForTaskComplete<T> : CustomYieldInstruction
    {
        private readonly Task<T> task;

        /// <summary>
        /// Wait for the task to complete.
        /// </summary>
        /// <param name="task">Task to wait.</param>
        public WaitForTaskComplete(Task<T> task) => this.task = task;

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
        /// Wrap a <see cref="Task{T}"/>.
        /// </summary>
        /// <param name="task">Handle to wrap.</param>
        public static implicit operator WaitForTaskComplete<T>(Task<T> task)
            => new WaitForTaskComplete<T>(task);
    }
}
