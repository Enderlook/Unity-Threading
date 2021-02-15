using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Unity.Jobs;

using UnityEngine;

namespace Enderlook.Unity.Threading.Jobs
{
    /// <summary>
    /// Helper methods for <see cref="IJobManaged"/>.
    /// </summary>
    public static partial class IJobManagedHelper
    {
        /// <inheritdoc cref="IJobExtensions.Schedule{T}(T, JobHandle)"/>
        /// <remarks>Useful when compiler finds ambiguity.</remarks>
        public static JobHandle ScheduleManaged(this IJobManaged job, JobHandle dependsOn = default)
            => Schedule(job, dependsOn);

        /// <inheritdoc cref="IJobExtensions.Schedule{T}(T, JobHandle)"/>
        public static JobHandle Schedule(this IJobManaged job, JobHandle dependsOn = default)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
                return new JobWithKey(GlobalDictionary<IJobManaged>.Store(job)).Schedule(dependsOn);
            else
                return new JobWithHandle(GCHandle.Alloc(job, GCHandleType.Pinned)).Schedule(dependsOn);
        }

        /// <inheritdoc cref="IJobExtensions.Schedule{T}(T, JobHandle)"/>
        /// <remarks>Useful when compiler finds ambiguity.</remarks>
        public static JobHandle ScheduleManaged<T>(this T job, JobHandle dependsOn = default)
            where T : struct, IJobManaged
            => Schedule(job, dependsOn);

        /// <inheritdoc cref="IJobExtensions.Schedule{T}(T, JobHandle)"/>
        public static JobHandle Schedule<T>(this T job, JobHandle dependsOn = default)
            where T : struct, IJobManaged
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                StrongBox<T> box = ConcurrentPool<StrongBox<T>>.Rent();
                box.Value = job;
                return new JobWithKey<T>(GlobalDictionary<StrongBox<T>>.Store(box)).Schedule(dependsOn);
            }
            else
            {
                StrongBox<T> box = ConcurrentPool<StrongBox<T>>.Rent();
                box.Value = job;
                return new JobWithHandle<T>(GCHandle.Alloc(box, GCHandleType.Pinned)).Schedule(dependsOn);
            }
        }

        private readonly struct JobWithHandle : IJob
        {
            private readonly GCHandle handle;

            public JobWithHandle(GCHandle handle) => this.handle = handle;

            public void Execute()
            {
                IJobManaged job = (IJobManaged)handle.Target;
                handle.Free();
                job.Execute();
            }
        }

        private readonly struct JobWithHandle<T> : IJob where T : IJobManaged
        {
            private readonly GCHandle handle;

            public JobWithHandle(GCHandle handle) => this.handle = handle;

            public void Execute()
            {
                StrongBox<T> box = (StrongBox<T>)handle.Target;
                handle.Free();
                T job = box.Value;
                ConcurrentPool<StrongBox<T>>.Return(box);
                job.Execute();
            }
        }

        private readonly struct JobWithKey : IJob
        {
            private readonly long key;

            public JobWithKey(long key) => this.key = key;

            public void Execute() => GlobalDictionary<IJobManaged>.Drain(key).Execute();
        }

        private readonly struct JobWithKey<T> : IJob where T : IJobManaged
        {
            private readonly long key;

            public JobWithKey(long key) => this.key = key;

            public void Execute()
            {
                StrongBox<T> box = GlobalDictionary<StrongBox<T>>.Drain(key);
                T job = box.Value;
                ConcurrentPool<StrongBox<T>>.Return(box);
                job.Execute();
            }
        }
    }
}