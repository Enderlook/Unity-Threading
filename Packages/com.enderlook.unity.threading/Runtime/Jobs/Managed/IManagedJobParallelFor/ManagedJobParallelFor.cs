using Unity.Jobs;

namespace Enderlook.Unity.Threading.Jobs
{
    /// <summary>
    /// Wrap a job into a managed job.
    /// </summary>
    /// <typeparam name="T">Type of job to wrap.</typeparam>
    public readonly struct ManagedJobParallelFor<T> : IManagedJobParallelFor where T : IJobParallelFor
    {
        private readonly T job;

        internal ManagedJobParallelFor(T job) => this.job = job;

        /// <inheritdoc cref="IManagedJobParallelFor.Execute"/>
        void IManagedJobParallelFor.Execute(int index) => job.Execute(index);
    }
}