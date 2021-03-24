using Enderlook.Collections.LowLevel;

using Unity.Jobs;

namespace Enderlook.Unity.Threading.Jobs
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
