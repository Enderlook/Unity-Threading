using System;

using Unity.Jobs;

namespace Enderlook.Unity.Threading.Jobs
{
    public static partial class JobManager
    {
        private class JobHandleCompleter
        {
            private const int GROW_FACTOR = 2;
            private const int DEFAULT_CAPACITY = 4;
            private JobHandle[] jobHandles = new JobHandle[DEFAULT_CAPACITY];
            private int size;

            public void Add(JobHandle jobHandle)
            {
                if (jobHandle.IsCompleted)
                    jobHandle.Complete();
                else
                {
                    if (jobHandles.Length == size)
                    {
                        JobHandle[] newJobHandles = new JobHandle[jobHandles.Length * GROW_FACTOR];
                        Array.Copy(jobHandles, newJobHandles, jobHandles.Length);
                    }

                    int index = size++;
                    jobHandles[index] = jobHandle;
                }
            }

            public void Update()
            {
                int j = 0;
                for (int i = 0; i < size; i++)
                {
                    JobHandle jobHandle = jobHandles[i];
                    if (jobHandle.IsCompleted)
                        jobHandle.Complete();
                    else
                        jobHandles[j++] = jobHandle;
                }
                size = j;
            }
        }
    }
}
