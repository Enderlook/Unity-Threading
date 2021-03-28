using Enderlook.Collections.LowLevel;
using Enderlook.Threading;

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

        private static class JobTasksManager<TAction> where TAction : IAction
        {
            private static RawList<JobTask<TAction>> jobTasks = RawList<JobTask<TAction>>.Create();

            static JobTasksManager()
            {
                updaters.Add(Update);
#if UNITY_EDITOR
                jobTasksManagers.Add(() => jobTasks.Count);
#endif
            }

            public static void Add(JobHandle jobHandle, TAction onJobComplete, bool canCompleteImmediately = true)
            {
                JobTask<TAction> jobTask = new JobTask<TAction>(jobHandle, onJobComplete);

                if (jobTask.IsCompleted && canCompleteImmediately)
                {
                    jobTask.Complete();
                    return;
                }

                jobTasks.Add(jobTask);
            }

            public static void Update()
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
