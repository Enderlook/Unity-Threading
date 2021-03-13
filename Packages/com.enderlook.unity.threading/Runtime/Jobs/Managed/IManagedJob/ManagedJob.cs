using Unity.Jobs;

namespace Enderlook.Unity.Threading.Jobs
{
    /// <summary>
    /// Wrap a job into a managed job.
    /// </summary>
    /// <typeparam name="T">Type of job to wrap.</typeparam>
    public readonly struct ManagedJob<T> : IManagedJob where T : IJob
    {
        private readonly T job;

        internal ManagedJob(T job) => this.job = job;

        /// <inheritdoc cref="IManagedJob.Execute"/>
        void IManagedJob.Execute() => job.Execute();
    }
}