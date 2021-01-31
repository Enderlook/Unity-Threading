using Enderlook.Threading;

using System;

using Unity.Jobs;

namespace Enderlook.Unity.Threading.Jobs
{
    public static partial class JobManager
    {
        private class JobTasksManager
        {
            private const int GROW_FACTOR = 2;
            private const int DEFAULT_CAPACITY = 4;
            private JobTask[] jobTasks = new JobTask[DEFAULT_CAPACITY];
            private int size;

            public void Add(JobHandle jobHandle, Action onJobComplete, bool canCompleteImmediately = true)
            {
                JobTask jobTask = default;
                jobTask.JobHandle = jobHandle;
                jobTask.OnJobComplete = onJobComplete;

                if (jobTask.IsCompleted && canCompleteImmediately)
                {
                    jobTask.Complete();
                    return;
                }

                if (jobTasks.Length == size)
                {
                    JobTask[] newJobTasks = new JobTask[jobTasks.Length * GROW_FACTOR];
                    Array.Copy(jobTasks, newJobTasks, jobTasks.Length);
                }

                int index = size++;
                jobTasks[index] = jobTask;
            }

            public void Update()
            {
                int j = 0;
                for (int i = 0; i < size; i++)
                {
                    JobTask jobTask = jobTasks[i];
                    if (jobTask.IsCompleted)
                        jobTask.Complete();
                    else
                        jobTasks[j++] = jobTask;
                }
                size = j;
            }
        }

        private class JobTasksManager<TAction> where TAction : IAction
        {
            private const int GROW_FACTOR = 2;
            private const int DEFAULT_CAPACITY = 4;
            private JobTask<TAction>[] jobTasks = new JobTask<TAction>[DEFAULT_CAPACITY];
            private int size;

            public void Add(JobHandle jobHandle, TAction onJobComplete, bool canCompleteImmediately = true)
            {
                JobTask<TAction> jobTask = default;
                jobTask.JobHandle = jobHandle;
                jobTask.OnJobComplete = onJobComplete;

                if (jobTask.IsCompleted && canCompleteImmediately)
                {
                    jobTask.Complete();
                    return;
                }

                if (jobTasks.Length == size)
                {
                    JobTask<TAction>[] newJobTasks = new JobTask<TAction>[jobTasks.Length * GROW_FACTOR];
                    Array.Copy(jobTasks, newJobTasks, jobTasks.Length);
                }

                int index = size++;
                jobTasks[index] = jobTask;
            }

            public void Update()
            {
                int j = 0;
                for (int i = 0; i < size; i++)
                {
                    JobTask<TAction> jobTask = jobTasks[i];
                    if (jobTask.IsCompleted)
                        jobTask.Complete();
                    else
                        jobTasks[j++] = jobTask;
                }
                size = j;
            }
        }
    }
}
