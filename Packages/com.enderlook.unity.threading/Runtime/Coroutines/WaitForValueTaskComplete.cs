using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

using UnityEngine;

namespace Enderlook.Unity.Threading.Coroutines
{
    /// <summary>
    /// Wait until the task is completed.
    /// </summary>
    public sealed class WaitForValueTaskComplete : CustomYieldInstruction
    {
        private readonly ValueTask task;

        /// <summary>
        /// Wait for the task to complete.
        /// </summary>
        /// <param name="task">Task to wait.</param>
        public WaitForValueTaskComplete(ValueTask task) => this.task = task;

        public override bool keepWaiting {
            get {
                if (task.IsCompleted)
                {
                    if (task.IsFaulted)
                        ExceptionDispatchInfo.Capture(task.AsTask().Exception).Throw();
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Wrap a <see cref="ValueTask"/>.
        /// </summary>
        /// <param name="task">Handle to wrap.</param>
        public static implicit operator WaitForValueTaskComplete(ValueTask task)
            => new WaitForValueTaskComplete(task);
    }
}
