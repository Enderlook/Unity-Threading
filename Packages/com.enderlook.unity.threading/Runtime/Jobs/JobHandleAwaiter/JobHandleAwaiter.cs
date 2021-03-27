using Enderlook.Threading;
using Enderlook.Unity.Coroutines;

using System;
using System.Collections;
using System.Runtime.CompilerServices;

using Unity.Jobs;

namespace Enderlook.Unity.Jobs
{
    /// <summary>
    /// An awaiter for <see cref="JobHandle"/>
    /// </summary>
    public struct JobHandleAwaiter : INotifyCompletion
    {
        // https://gist.github.com/distantcam/64cf44d84441e5c45e197f7d90c6df3e

        internal readonly JobHandle jobHandle;

        /// <summary>
        /// Creates an awaiter for the given <see cref="JobHandle"/>.
        /// </summary>
        /// <param name="jobHandle">Job handle to await for.</param>
        public JobHandleAwaiter(JobHandle jobHandle)
        {
            jobHandle.WatchCompletition();
            this.jobHandle = jobHandle;
        }

        /// <summary>
        /// Return self.
        /// </summary>
        /// <returns>Self.</returns>
        public JobHandleAwaiter GetAwaiter() => this;

        /// <summary>
        /// Whenever the job handle has completed or not.
        /// </summary>
        public bool IsCompleted => jobHandle.IsCompleted;

        /// <summary>
        /// Determines the action to run after the job handle has completed.
        /// </summary>
        /// <param name="continuation">Action to run.</param>
        public void OnCompleted(Action continuation) => jobHandle.OnComplete(continuation);

        /// <summary>
        /// Determines the action to run after the job handle has completed.
        /// </summary>
        /// <typeparam name="T">Type of action to execute.</typeparam>
        /// <param name="continuation">Action to run.</param>
        public void OnCompleted<T>(T continuation) where T : IAction => jobHandle.OnComplete(continuation);

        /// <summary>
        /// Does nothing.
        /// </summary>
        public void GetResult() { }

        /// <summary>
        /// Convert a this awaiter into an enumerator.
        /// </summary>
        /// <returns>Enumerator which wraps the awaiter.</returns>
        public IEnumerator AsCoroutine()
        {
            while (!jobHandle.IsCompleted)
                yield return null;

            jobHandle.Complete();
        }

        /// <inheritdoc cref="WaitForJobComplete.Create(JobHandle)"/>
        public WaitForJobComplete Wait()
            => WaitForJobComplete.Create(jobHandle);
    }
}