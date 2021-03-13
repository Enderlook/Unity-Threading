﻿using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using Unity.Jobs;

using UnityEngine;

namespace Enderlook.Unity.Threading.Jobs
{
    /// <summary>
    /// Helper methods for <see cref="IManagedJobParallelFor"/>.
    /// </summary>
    public static class IManagedJobParallelForExtensions
    {
        /// <inheritdoc cref="IJobParallelForExtensions.Schedule{T}(T, int, int, JobHandle)"/>
        public static JobHandle Schedule(this IManagedJobParallelFor job, int arrayLength, int innerLoopBatchCount, JobHandle dependsOn = default)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                long key = GlobalDictionary<IManagedJobParallelFor>.Store(job);
                JobHandle jobHandle = new JobWithKey(key).Schedule(arrayLength, innerLoopBatchCount, dependsOn);
                return new JobFreeKey(key).Schedule(jobHandle);
            }
            else
            {
                GCHandle handle = GCHandle.Alloc(job, GCHandleType.Pinned);
                JobHandle jobHandle = new JobWithHandle(handle).Schedule(arrayLength, innerLoopBatchCount, dependsOn);
                return new JobFreeHandle(handle).Schedule(jobHandle);
            }
        }

        /// <summary>
        /// Schedules and automatically watches the completition of this job.<br/>
        /// Useful for fire and forget.
        /// </summary>
        /// <param name="job">Job to schedule and watch.</param>
        /// <param name="arrayLength">The number of iterations the for loop will execute.</param>
        /// <param name="innerLoopBatchCount">Granularity in which workstealing is performed. A value of 32, means the job queue will steal 32 iterations and then perform them in an efficient inner loop.</param>
        /// <param name="dependsOn">Another job that must be executed before executing <paramref name="job"/>.</param>
        /// <returns>Job handle of the scheduled task.</returns>
        public static JobHandle ScheduleAndWatch(this IManagedJobParallelFor job, int arrayLength, int innerLoopBatchCount, JobHandle dependsOn = default)
            => job.Schedule(arrayLength, innerLoopBatchCount, dependsOn).WatchCompletition();

        /// <inheritdoc cref="IJobParallelForExtensions.Schedule{T}(T, int, int, JobHandle)"/>
        public static JobHandle Schedule<T>(this T job, int arrayLength, int innerLoopBatchCount, JobHandle dependsOn = default)
            where T : struct, IManagedJobParallelFor
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                StrongBox<T> box = ConcurrentPool<StrongBox<T>>.Rent();
                box.Value = job;
                long key = GlobalDictionary<StrongBox<T>>.Store(box);
                JobHandle jobHandle = new JobWithKey<T>(key).Schedule(arrayLength, innerLoopBatchCount, dependsOn);
                return new JobFreeKey<T>(key).Schedule(jobHandle);
            }
            else
            {
                StrongBox<T> box = ConcurrentPool<StrongBox<T>>.Rent();
                box.Value = job;
                GCHandle handle = GCHandle.Alloc(box, GCHandleType.Pinned);
                JobHandle jobHandle = new JobWithHandle<T>(handle).Schedule(arrayLength, innerLoopBatchCount, dependsOn);
                return new JobFreeHandle<T>(handle).Schedule(jobHandle);
            }
        }

        /// <summary>
        /// Schedules and automatically watches the completition of this job.<br/>
        /// Useful for fire and forget.
        /// </summary>
        /// <typeparam name="T">Type of job.</typeparam>
        /// <param name="job">Job to schedule and watch.</param>
        /// <param name="arrayLength">The number of iterations the for loop will execute.</param>
        /// <param name="innerLoopBatchCount">Granularity in which workstealing is performed. A value of 32, means the job queue will steal 32 iterations and then perform them in an efficient inner loop.</param>
        /// <param name="dependsOn">Another job that must be executed before executing <paramref name="job"/>.</param>
        /// <returns>Job handle of the scheduled task.</returns>
        public static JobHandle ScheduleAndWatch<T>(this T job, int arrayLength, int innerLoopBatchCount, JobHandle dependsOn = default)
            where T : struct, IManagedJobParallelFor
            => job.Schedule(arrayLength, innerLoopBatchCount, dependsOn).WatchCompletition();

        /// <summary>
        /// Wrap a job into a managed job.
        /// </summary>
        /// <typeparam name="T">Type of job to wrap.</typeparam>
        /// <param name="job">Job to wrap.</param>
        /// <returns>Wrapped job.</returns>
        public static ManagedJobParallelFor<T> AsManaged<T>(this T job) where T : IJobParallelFor
            => new ManagedJobParallelFor<T>(job);

        /// <inheritdoc cref="IJobParallelForExtensions.Run{T}(T, int)"/>
        public static void Run(this IManagedJobParallelFor job, int arrayLength)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                long key = GlobalDictionary<IManagedJobParallelFor>.Store(job);
                new JobWithKey(key).Run(arrayLength);
                GlobalDictionary<IManagedJobParallelFor>.Drain(key);
            }
            else
            {
                GCHandle handle = GCHandle.Alloc(job, GCHandleType.Pinned);
                new JobWithHandle(handle).Run(arrayLength);
                handle.Free();
            }
        }

        /// <inheritdoc cref="IJobParallelForExtensions.Run{T}(T, int)"/>
        public static void Run<T>(this T job, int arrayLength)
            where T : struct, IManagedJobParallelFor
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                StrongBox<T> box = ConcurrentPool<StrongBox<T>>.Rent();
                box.Value = job;
                long key = GlobalDictionary<StrongBox<T>>.Store(box);
                new JobWithKey<T>(key).Run(arrayLength);
                GlobalDictionary<StrongBox<T>>.Remove(key);
                ConcurrentPool<StrongBox<T>>.Return(box);
            }
            else
            {
                StrongBox<T> box = ConcurrentPool<StrongBox<T>>.Rent();
                box.Value = job;
                GCHandle handle = GCHandle.Alloc(box, GCHandleType.Pinned);
                new JobWithHandle<T>(handle).Run(arrayLength);
                ConcurrentPool<StrongBox<T>>.Return(box);
            }
        }

        private readonly struct JobWithHandle : IJobParallelFor
        {
            private readonly GCHandle handle;

            public JobWithHandle(GCHandle handle) => this.handle = handle;

            public void Execute(int index)
            {
                IManagedJobParallelFor job = (IManagedJobParallelFor)handle.Target;
                job.Execute(index);
            }
        }

        private readonly struct JobFreeHandle : IJob
        {
            private readonly GCHandle handle;

            public JobFreeHandle(GCHandle handle) => this.handle = handle;

            public void Execute() => handle.Free();
        }

        private readonly struct JobWithHandle<T> : IJobParallelFor where T : IManagedJobParallelFor
        {
            private readonly GCHandle handle;

            public JobWithHandle(GCHandle handle) => this.handle = handle;

            public void Execute(int index)
            {
                StrongBox<T> box = (StrongBox<T>)handle.Target;
                T job = box.Value;
                job.Execute(index);
            }
        }

        private readonly struct JobFreeHandle<T> : IJob where T : IManagedJobParallelFor
        {
            private readonly GCHandle handle;

            public JobFreeHandle(GCHandle handle) => this.handle = handle;

            public void Execute()
            {
                StrongBox<T> box = (StrongBox<T>)handle.Target;
                ConcurrentPool<StrongBox<T>>.Return(box);
                handle.Free();
            }
        }

        private readonly struct JobWithKey : IJobParallelFor
        {
            private readonly long key;

            public JobWithKey(long key) => this.key = key;

            public void Execute(int index) => GlobalDictionary<IManagedJobParallelFor>.Peek(key).Execute(index);
        }

        private readonly struct JobFreeKey : IJob
        {
            private readonly long key;

            public JobFreeKey(long key) => this.key = key;

            public void Execute() => GlobalDictionary<IManagedJobParallelFor>.Remove(key);
        }

        private readonly struct JobWithKey<T> : IJobParallelFor where T : IManagedJobParallelFor
        {
            private readonly long key;

            public JobWithKey(long key) => this.key = key;

            public void Execute(int index)
            {
                StrongBox<T> box = GlobalDictionary<StrongBox<T>>.Peek(key);
                T job = box.Value;
                job.Execute(index);
            }
        }

        private readonly struct JobFreeKey<T> : IJob where T : IManagedJobParallelFor
        {
            private readonly long key;

            public JobFreeKey(long key) => this.key = key;

            public void Execute()
            {
                StrongBox<T> box = GlobalDictionary<StrongBox<T>>.Drain(key);
                ConcurrentPool<StrongBox<T>>.Return(box);
            }
        }
    }
}