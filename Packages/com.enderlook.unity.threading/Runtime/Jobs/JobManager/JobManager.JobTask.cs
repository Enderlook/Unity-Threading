using Enderlook.Threading;

using System;

using Unity.Jobs;

namespace Enderlook.Unity.Threading.Jobs
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
        }

        private struct JobTask<TAction> where TAction : IAction
        {
            public JobHandle JobHandle { get; set; }

            public TAction OnJobComplete { get; set; }

            public bool IsCompleted => JobHandle.IsCompleted;

            public void Complete()
            {
                JobHandle.Complete();
                OnJobComplete.Invoke();
            }
        }
    }
}
