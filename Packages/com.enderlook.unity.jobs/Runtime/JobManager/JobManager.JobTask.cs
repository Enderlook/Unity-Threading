using System;

using Unity.Jobs;

namespace Enderlook.Unity.Jobs
{
    public static partial class JobManager
    {
        private struct JobTask
        {
            public JobHandle JobHandle { get; set; }

            public Action OnJobComplete { get; set; }

            public bool IsCompleted => JobHandle.IsCompleted;

            public void Complete()
            {
                JobHandle.Complete();
                OnJobComplete();
            }

            public JobTask(JobHandle jobHandle, Action onJobComplete)
            {
                JobHandle = jobHandle;
                OnJobComplete = onJobComplete;
            }
        }
    }
}
