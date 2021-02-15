using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

using UnityEngine;

namespace Enderlook.Unity.Threading.Coroutines
{
    /// <summary>
    /// Wait until the task is completed.
    /// </summary>
    public sealed class WaitForValueTaskComplete<T> : CustomYieldInstruction
    {
        private readonly ValueTask<T> task;

        /// <summary>
        /// Wait for the task to complete.
        /// </summary>
        /// <param name="task">Task to wait.</param>
        public WaitForValueTaskComplete(ValueTask<T> task) => this.task = task;

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
        /// Wrap a <see cref="ValueTask{T}"/>.
        /// </summary>
        /// <param name="task">Handle to wrap.</param>
        public static implicit operator WaitForValueTaskComplete<T>(ValueTask<T> task)
            => new WaitForValueTaskComplete<T>(task);
    }
}
