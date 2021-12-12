using Enderlook.Unity.Threading;

using System;
using System.Collections;
using System.Runtime.CompilerServices;

using Unity.Jobs;

using UnityEngine;

namespace Enderlook.Unity.Jobs
{
    /// <summary>
    /// An awaiter for <see cref="JobHandle"/>
    /// </summary>
    public struct JobHandleAwaiter : INotifyCompletion
    {
        // https://gist.github.com/distantcam/64cf44d84441e5c45e197f7d90c6df3e

        private static readonly Func<JobHandle, bool> isCompleted = e => e.IsCompleted;

        internal readonly JobHandle jobHandle;

        /// <summary>
        /// Creates an awaiter for the given <see cref="JobHandle"/>.
        /// </summary>
        /// <param name="jobHandle">Job handle to await for.</param>
        public JobHandleAwaiter(JobHandle jobHandle)
        {
            jobHandle.WatchCompletion();
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
        public bool IsCompleted
        {
            get {
                if (UnityThread.IsMainThread)
                    return jobHandle.IsCompleted;
                Debug.Log("This method requires to be executed in the Unity thread but wasn't.\n" +
                    "As a fallback, it will forward the call to the Unity thread.\n" +
                    "However, this may produce deadlocks very easily.");
                return UnityThread.RunNow(isCompleted, jobHandle);
            }
        }

        /// <summary>
        /// Determines the action to run after the job handle has completed.
        /// </summary>
        /// <param name="continuation">Action to run.</param>
        public void OnCompleted(Action continuation) => jobHandle.OnComplete(continuation);

        /// <summary>
        /// Does nothing.
        /// </summary>
        public void GetResult()
        {
            // We could execute `jobHandle.Complete()` here in order to force finalization of the call.
            // However, that would require to block if this call is not in the Unity thread.
            // Also, we are already using JobManager.WatchCompletion(JobHandle) method so it's not necessary.
            // Just noting in case we may want in the future.
        }

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
    }
}