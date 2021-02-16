using Enderlook.Collections.LowLevel;
using Enderlook.Threading;

using System;

using Unity.Jobs;

namespace Enderlook.Unity.Threading.Jobs
{
    public static partial class JobManager
    {
        private class JobTasksManager
        {
            private DynamicArray<JobTask> jobTasks = DynamicArray<JobTask>.Create();

            public void Add(JobHandle jobHandle, Action onJobComplete, bool canCompleteImmediately = true)
            {
                JobTask jobTask = new JobTask(jobHandle, onJobComplete);

                if (jobTask.IsCompleted && canCompleteImmediately)
                {
                    jobTask.Complete();
                    return;
                }

                jobTasks.Add(jobTask);
            }

            public void Update()
            {
                for (int i = jobTasks.Count - 1; i > 0; i--)
                {
                    JobTask jobTask = jobTasks[i];
                    if (jobTask.IsCompleted)
                    {
                        jobTask.Complete();
                        jobTasks.RemoveAt(i);
                    }
                }
            }
        }

        private class JobTasksManager<TAction> where TAction : IAction
        {
            private DynamicArray<JobTask<TAction>> jobTasks = DynamicArray<JobTask<TAction>>.Create();

            public void Add(JobHandle jobHandle, TAction onJobComplete, bool canCompleteImmediately = true)
            {
                JobTask<TAction> jobTask = new JobTask<TAction>(jobHandle, onJobComplete);

                if (jobTask.IsCompleted && canCompleteImmediately)
                {
                    jobTask.Complete();
                    return;
                }

                jobTasks.Add(jobTask);
            }

            public void Update()
            {
                for (int i = jobTasks.Count - 1; i > 0; i--)
                {
                    JobTask<TAction> jobTask = jobTasks[i];
                    if (jobTask.IsCompleted)
                    {
                        jobTask.Complete();
                        jobTasks.RemoveAt(i);
                    }
                }
            }
        }
    }
}
