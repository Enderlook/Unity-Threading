using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;

namespace Enderlook.Unity.Jobs
{
    internal sealed class UnityJobTaskScheduler : TaskScheduler
    {
        public override int MaximumConcurrencyLevel => JobsUtility.MaxJobThreadCount;

        protected override IEnumerable<Task> GetScheduledTasks() => Enumerable.Empty<Task>();

        protected override void QueueTask(Task task)
        {
            JobHandle jobHandle = new Job(this, task).Schedule();
            jobHandle.WatchCompletion();
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            if (JobsUtility.IsExecutingJob)
                return TryExecuteTask(task);
            return false;
        }

        internal void TryExecuteTask_(Task task) => TryExecuteTask(task);

        private readonly struct Job : IManagedJob
        {
            private readonly Task task;
            private readonly UnityJobTaskScheduler scheduler;

            public Job(UnityJobTaskScheduler scheduler, Task task)
            {
                this.scheduler = scheduler;
                this.task = task;
            }

            public void Execute() => ExclusiveSynchronizationContext.Run(task, scheduler);
            /*{
                SynchronizationContext oldContext = SynchronizationContext.Current;
                UnityJobSyncronizationContext context = new UnityJobSyncronizationContext(oldContext);
                SynchronizationContext.SetSynchronizationContext(context);
                scheduler.TryExecuteTask(task);
                context.Run(task);
            }*/
        }
    }
}
