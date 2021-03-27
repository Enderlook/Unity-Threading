using Unity.Jobs;

namespace Enderlook.Unity.Jobs
{
    /// <summary>
    /// Represent an <see cref="IJobParallelFor"/> that contains managed data.
    /// </summary>
    public interface IManagedJobParallelFor
    {
        /// <summary>
        /// Action to execute.
        /// </summary>
        /// <param name="index">Index of element to execute</param>
        void Execute(int index);
    }
}