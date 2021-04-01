using Unity.Jobs;

namespace Enderlook.Unity.Jobs
{
    /// <summary>
    /// Represent an <see cref="IJob"/> that contains managed data.
    /// </summary>
    public interface IManagedJob
    {
        /// <summary>
        /// Action to execute.
        /// </summary>
        void Execute();
    }
}