﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) => TryExecuteTaskMainThread(task);

        private bool TryExecuteTaskMainThread(Task task)
        {
            if (Switch.IsInMainThread)
            {
#if UNITY_EDITOR
                Debug.Log("Already in main thread, this won't do anything meaningful.");
#endif
                return TryExecuteTask(task);
            }
            else
                return SwitchInSite(task).GetAwaiter().GetResult();
        }

        private async ValueTask<bool> SwitchInSite(Task task)
        {
            await Switch.ToUnity;
            bool result = TryExecuteTask(task);
            await Switch.ToBackground;
            return result;
        }

        protected override IEnumerable<Task> GetScheduledTasks() => Enumerable.Empty<Task>();
    }
}