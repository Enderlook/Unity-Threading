using Enderlook.Unity.Threading;

using System;
using System.Threading.Tasks;

using Unity.Jobs;

namespace Enderlook.Unity.Jobs
{
    public static class AsyncHelpers
    {
        public static JobHandle RunInJob(Func<Task> task)
        {
            if (UnityThread.IsMainThread)
                return new Job(task).Schedule();
            return UnityThread.RunNow(() => new Job(task).Schedule());
        }

        private readonly struct Job : IManagedJob
        {
            private readonly Func<Task> func;

            public Job(Func<Task> func) => this.func = func;

            public void Execute() => ExclusiveSynchronizationContext.Run(func);
        }
    }
}
