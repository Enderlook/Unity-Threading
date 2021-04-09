using Enderlook.Collections.LowLevel;

using System;
using System.Runtime.CompilerServices;

using Unity.Jobs;

namespace Enderlook.Unity.Jobs
{
    public static partial class JobManager
    {
        private static class JobTasksManager
        {
            private static RawList<JobTask> jobTasks = RawList<JobTask>.Create();

#if UNITY_EDITOR
            /// <summary>
            /// Unity Editor Only.
            /// </summary>
            internal static int Count => jobTasks.Count;
#endif

            public static void Add(JobHandle jobHandle, Action onJobComplete, bool canCompleteImmediately = true)
            {
                if (onJobComplete is null)
                    ThrowArgumentNullException();

                JobTask jobTask = new JobTask(jobHandle, onJobComplete);

                if (jobTask.IsCompleted && canCompleteImmediately)
                {
                    jobTask.Complete();
                    return;
                }

                jobTasks.Add(jobTask);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static void ThrowArgumentNullException() => throw new ArgumentNullException("onJobComplete");

            public static void Update()
            {
                int j = 0;
                for (int i = 0; i < jobTasks.Count; i++)
                {
                    JobTask jobTask = jobTasks[i];
                    if (jobTask.IsCompleted)
                    {
                        jobTask.Complete();
                        continue;
                    }
                    jobTasks[j++] = jobTask;
                }
                jobTasks = RawList<JobTask>.From(jobTasks.UnderlyingArray, j);
            }
        }
    }
}
