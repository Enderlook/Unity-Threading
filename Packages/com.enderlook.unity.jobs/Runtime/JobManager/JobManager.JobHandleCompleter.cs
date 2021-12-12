using Enderlook.Collections.LowLevel;
using Enderlook.Unity.Threading;

using Unity.Jobs;

namespace Enderlook.Unity.Jobs
{
    public static partial class JobManager
    {
        private static class JobHandleCompleter
        {
            private static RawList<JobHandle> jobHandles = RawList<JobHandle>.Create();

#if UNITY_EDITOR
            /// <summary>
            /// Unity Editor Only.
            /// </summary>
            internal static int Count => jobHandles.Count;
#endif

            public static void Add(JobHandle jobHandle)
            {
                if (UnityThread.IsMainThread && jobHandle.IsCompleted)
                    jobHandle.Complete();
                else
                    jobHandles.Add(jobHandle);
            }

            public static void Update()
            {
                int j = 0;
                for (int i = 0; i < jobHandles.Count; i++)
                {
                    JobHandle jobHandle = jobHandles[i];
                    if (jobHandle.IsCompleted)
                    {
                        jobHandle.Complete();
                        continue;
                    }
                    jobHandles[j++] = jobHandle;
                }
                jobHandles = RawList<JobHandle>.From(jobHandles.UnderlyingArray, j);
            }
        }
    }
}
