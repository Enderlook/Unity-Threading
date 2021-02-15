using Unity.Jobs;

using UnityEngine;

namespace Enderlook.Unity.Threading.Coroutines
{
    /// <summary>
    /// Wait until a Unity job is completed.
    /// </summary>
    public sealed class WaitForJobComplete : CustomYieldInstruction
    {
        private readonly JobHandle handle;

        /// <summary>
        /// Wait for a job to complete.
        /// </summary>
        /// <param name="handle">Job to wait.</param>
        public WaitForJobComplete(JobHandle handle) => this.handle = handle;

        public override bool keepWaiting {
            get {
                if (handle.IsCompleted)
                {
                    handle.Complete();
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Wrap a <see cref="JobHandle"/>.
        /// </summary>
        /// <param name="handle">Handle to wrap.</param>
        public static implicit operator WaitForJobComplete(JobHandle handle)
            => new WaitForJobComplete(handle);
    }
}
