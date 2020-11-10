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
    }
}