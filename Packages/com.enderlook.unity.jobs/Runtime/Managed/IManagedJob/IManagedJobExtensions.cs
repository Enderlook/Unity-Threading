using Enderlook.Unity.Threading;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Unity.Jobs;

using UnityEngine;

namespace Enderlook.Unity.Jobs
{
    /// <summary>
    /// Helper methods for <see cref="IManagedJob"/>.
    /// </summary>
    public static partial class IManagedJobExtensions
    {
        /// <inheritdoc cref="IJobExtensions.Schedule{T}(T, JobHandle)"/>
        public static JobHandle Schedule(this IManagedJob job, JobHandle dependsOn = default)
        {
#if UNITY_WEBGL
            return new JobWithKey(GlobalDictionary<IManagedJob>.Store(job)).Schedule(dependsOn);
#else
            return new JobWithHandle(GCHandle.Alloc(job, GCHandleType.Pinned)).Schedule(dependsOn);
#endif
        }

        /// <inheritdoc cref="IJobExtensions.Schedule{T}(T, JobHandle)"/>
        public static JobHandle Schedule<T>(this T job, JobHandle dependsOn = default)
            where T : struct, IManagedJob
        {
#if UNITY_WEBGL
            StrongBox<T> box = ConcurrentPool.Rent<StrongBox<T>>();
            box.Value = job;
            return new JobWithKey<T>(GlobalDictionary<StrongBox<T>>.Store(box)).Schedule(dependsOn);
#else
            StrongBox<T> box = ConcurrentPool.Rent<StrongBox<T>>();
            box.Value = job;
            return new JobWithHandle<T>(GCHandle.Alloc(box, GCHandleType.Pinned)).Schedule(dependsOn);
#endif
        }

        /// <summary>
        /// Wrap a job into a managed job.
        /// </summary>
        /// <typeparam name="T">Type of job to wrap.</typeparam>
        /// <param name="job">Job to wrap.</param>
        /// <returns>Wrapped job.</returns>
        public static ManagedJob<T> AsManaged<T>(this T job) where T : IJob
            => new ManagedJob<T>(job);

        /// <inheritdoc cref="IJobExtensions.Schedule{T}(T, JobHandle)"/>
        public static void Run(this IManagedJob job)
        {
#if UNITY_WEBGL
            new JobWithKey(GlobalDictionary<IManagedJob>.Store(job)).Run();
#else
            new JobWithHandle(GCHandle.Alloc(job, GCHandleType.Pinned)).Run();
#endif
        }

        /// <inheritdoc cref="IJobExtensions.Run{T}(T, JobHandle)"/>
        public static void Run<T>(this T job)
            where T : struct, IManagedJob
        {
#if UNITY_WEBGL
            StrongBox<T> box = ConcurrentPool.Rent<StrongBox<T>>();
            box.Value = job;
            new JobWithKey<T>(GlobalDictionary<StrongBox<T>>.Store(box)).Run();
#else
            StrongBox<T> box = ConcurrentPool.Rent<StrongBox<T>>();
            box.Value = job;
            new JobWithHandle<T>(GCHandle.Alloc(box, GCHandleType.Pinned)).Run();
#endif
        }

        private readonly struct JobWithHandle : IJob
        {
            private readonly GCHandle handle;

            public JobWithHandle(GCHandle handle) => this.handle = handle;

            public void Execute()
            {
                IManagedJob job = (IManagedJob)handle.Target;
                handle.Free();
                job.Execute();
            }
        }

        private readonly struct JobWithHandle<T> : IJob where T : IManagedJob
        {
            private readonly GCHandle handle;

            public JobWithHandle(GCHandle handle) => this.handle = handle;

            public void Execute()
            {
                StrongBox<T> box = (StrongBox<T>)handle.Target;
                handle.Free();
                T job = box.Value;
                ConcurrentPool.Return(box);
                job.Execute();
            }
        }

        private readonly struct JobWithKey : IJob
        {
            private readonly long key;

            public JobWithKey(long key) => this.key = key;

            public void Execute() => GlobalDictionary<IManagedJob>.Drain(key).Execute();
        }

        private readonly struct JobWithKey<T> : IJob where T : IManagedJob
        {
            private readonly long key;

            public JobWithKey(long key) => this.key = key;

            public void Execute()
            {
                StrongBox<T> box = GlobalDictionary<StrongBox<T>>.Drain(key);
                T job = box.Value;
                ConcurrentPool.Return(box);
                job.Execute();
            }
        }
    }
}