using Enderlook.Pools;

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
        public static JobHandle Schedule<T>(this T job, JobHandle dependsOn = default)
            where T : IManagedJob
        {
            if (typeof(T).IsValueType)
            {
                StrongBox<T> box = ObjectPool<StrongBox<T>>.Shared.Rent();
#if UNITY_WEBGL
                box.Value = job;
                return new JobWithKey<T>(GlobalDictionary<StrongBox<T>>.Store(box)).Schedule(dependsOn);
#else
                box.Value = job;
                return new JobWithHandle<T>(GCHandle.Alloc(box, GCHandleType.Pinned)).Schedule(dependsOn);
#endif
            }
            else
            {
#if UNITY_WEBGL
                return new JobWithKey(GlobalDictionary<IManagedJob>.Store(job)).Schedule(dependsOn);
#else
                return new JobWithHandle(GCHandle.Alloc(job, GCHandleType.Pinned)).Schedule(dependsOn);
#endif
            }
        }

        /// <summary>
        /// Wrap a job into a managed job.
        /// </summary>
        /// <typeparam name="T">Type of job to wrap.</typeparam>
        /// <param name="job">Job to wrap.</param>
        /// <returns>Wrapped job.</returns>
        public static ManagedJob<T> AsManaged<T>(this T job) where T : IJob
            => new ManagedJob<T>(job);

        /// <inheritdoc cref="IJobExtensions.Run{T}(T, JobHandle)"/>
        public static void Run<T>(this T job)
            where T : IManagedJob
        {
            if (typeof(T).IsValueType)
            {
#if UNITY_WEBGL
                StrongBox<T> box = ObjectPool<StrongBox<T>>.Shared.Rent();
                box.Value = job;
                new JobWithKey<T>(GlobalDictionary<StrongBox<T>>.Store(box)).Run();
#else
                StrongBox<T> box = ObjectPool<StrongBox<T>>.Shared.Rent();
                box.Value = job;
                new JobWithHandle<T>(GCHandle.Alloc(box, GCHandleType.Pinned)).Run();
#endif
            }
            else
            {
#if UNITY_WEBGL
                new JobWithKey(GlobalDictionary<IManagedJob>.Store(job)).Run();
#else
                new JobWithHandle(GCHandle.Alloc(job, GCHandleType.Pinned)).Run();
#endif
            }
        }

#if !UNITY_WEBGL
        private readonly struct JobWithHandle : IJob
        {
            private readonly GCHandle handle;

            public JobWithHandle(GCHandle handle) => this.handle = handle;

            public void Execute()
            {
                object target = handle.Target;
                Debug.Assert(target is IManagedJob);
                IManagedJob job = Unsafe.As<IManagedJob>(target);
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
                object target = handle.Target;
                Debug.Assert(target is StrongBox<T>);
                StrongBox<T> box = Unsafe.As<StrongBox<T>>(target);
                handle.Free();
                T job = box.Value;
                ObjectPool<StrongBox<T>>.Shared.Return(box);
                job.Execute();
            }
        }
#else
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
                ObjectPool<StrongBox<T>>.Shared.Return(box);
                job.Execute();
            }
        }
#endif
    }
}