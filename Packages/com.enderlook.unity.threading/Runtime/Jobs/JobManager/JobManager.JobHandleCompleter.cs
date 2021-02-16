using Enderlook.Collections.LowLevel;

using Unity.Jobs;

namespace Enderlook.Unity.Threading.Jobs
{
    public static partial class JobManager
    {
        private static class JobHandleCompleter
        {
            private static DynamicPooledArray<JobHandle> jobHandles = DynamicPooledArray<JobHandle>.Create();

            public static void Add(JobHandle jobHandle)
            {
                if (jobHandle.IsCompleted)
                    jobHandle.Complete();
                else
                    jobHandles.Add(jobHandle);
            }

            public static void Update()
            {
                for (int i = jobHandles.Count - 1; i > 0; i--)
                {
                    JobHandle jobHandle = jobHandles[i];
                    if (jobHandle.IsCompleted)
                    {
                        jobHandle.Complete();
                        jobHandles.RemoveAt(i);
                    }
                }
            }
        }
    }
}
