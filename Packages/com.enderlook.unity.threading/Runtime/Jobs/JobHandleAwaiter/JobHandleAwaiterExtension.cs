using Enderlook.Unity.Threading.Coroutines;

using System.Collections;

using Unity.Jobs;

namespace Enderlook.Unity.Threading.Jobs
{
    /// <summary>
    /// Allow get awaiter from <see cref="JobHandle"/>.
    /// </summary>
    public static class JobHandleAwaiterExtension
    {
        /// <summary>
        /// Wrap a job handle as a task.
        /// </summary>
        /// <param name="jobHandle">Job handle to wrap.</param>
        /// <returns>Task wrapper of the job handle.</returns>
        public static JobHandleAwaiter GetAwaiter(this JobHandle jobHandle) => new JobHandleAwaiter(jobHandle);

        /// <summary>
        /// Convert a job handle into an enumerator.
        /// </summary>
        /// <param name="jobHandle">Job handle to convert.</param>
        /// <returns>Enumerator which wraps the job handle.</returns>
        public static IEnumerator AsCoroutine(this JobHandle jobHandle)
        {
            while (!jobHandle.IsCompleted)
                yield return null;

            jobHandle.Complete();
        }

        /// <inheritdoc cref="WaitForJobComplete.Create(JobHandle)"/>
        public static WaitForJobComplete Wait(this JobHandle handle)
            => WaitForJobComplete.Create(handle);
    }
}