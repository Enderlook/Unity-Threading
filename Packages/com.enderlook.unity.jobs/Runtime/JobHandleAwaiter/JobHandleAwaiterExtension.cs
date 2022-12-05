using Unity.Jobs;

namespace Enderlook.Unity.Jobs
{
    /// <summary>
    /// Allows to get an awaiter from <see cref="JobHandle"/>.
    /// </summary>
    public static class JobHandleAwaiterExtension
    {
        /// <summary>
        /// Wrap a job handle as a task.
        /// </summary>
        /// <param name="jobHandle">Job handle to wrap.</param>
        /// <returns>Task wrapper of the job handle.</returns>
        public static JobHandleAwaiter GetAwaiter(this JobHandle jobHandle) => new JobHandleAwaiter(jobHandle);
    }
}