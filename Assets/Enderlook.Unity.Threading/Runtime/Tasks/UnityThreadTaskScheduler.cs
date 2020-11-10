using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Unity.Jobs.LowLevel.Unsafe;

using UnityEngine;

namespace Enderlook.Unity.Threading.Tasks
{
    /// <summary>
    /// Defines an shcheduler which runs all its tasks in the main thread.
    /// </summary>
    public sealed class UnityThreadTaskScheduler : TaskScheduler
    {
        public static UnityThreadTaskScheduler Instance = new UnityThreadTaskScheduler();

        public override int MaximumConcurrencyLevel => 1;

        protected override void QueueTask(Task task) => TryExecuteTaskMainThread(task);

        protected override bool TryExecuteTaskInline(
            Task task,
            bool taskWasPreviouslyQueued) => TryExecuteTaskMainThread(task);

        private bool TryExecuteTaskMainThread(Task task)
        {
            if (ThreadSwitcher.IsExecutingMainThread)
                return TryExecuteTask(task);
            else
            {
                if (JobsUtility.IsExecutingJob)
                {
                    if (Application.platform == RuntimePlatform.WebGLPlayer)
                    {
                        // We are in a problem. WebGL doesn't have multithreading, and so we can't run a task to run it.
                        Debug.LogWarning("Attempted to run in main thread from a Unity Job in WebGL platform. Due lack of multithreading a fallback will run this code and all subsequent code of the job in the main thread.");
                        return SwitchInSiteNoReturn(task).GetAwaiter().GetResult();
                    }
                    else
                        return Task.Run(async () =>
                        {
                            await ThreadSwitcher.ResumeUnityAsync;
                            return TryExecuteTask(task);
                        }).GetAwaiter().GetResult();
                }
                else
                    return SwitchInSite(task).GetAwaiter().GetResult();
            }
        }

        private async ValueTask<bool> SwitchInSiteNoReturn(Task task)
        {
            await ThreadSwitcher.ResumeUnityAsync;
            return TryExecuteTask(task);
        }

        private async ValueTask<bool> SwitchInSite(Task task)
        {
            await ThreadSwitcher.ResumeUnityAsync;
            bool result = TryExecuteTask(task);
            await ThreadSwitcher.ResumeTaskAsync;
            return result;
        }

        protected override IEnumerable<Task> GetScheduledTasks() => Enumerable.Empty<Task>();
    }
}