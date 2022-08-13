using Enderlook.Pools;

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

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
                return new JobWithKeyValueType<T>(GlobalDictionary<StrongBox<T>>.Store(box)).Schedule(dependsOn);
#else
                box.Value = job;
                return new JobWithHandleValueType<T>(GCHandle.Alloc(box, GCHandleType.Pinned)).Schedule(dependsOn);
#endif
            }
            else
            {
#if UNITY_WEBGL
                return new JobWithKeyReferenceType
#if UNITY_EDITOR
                    <T>
#endif
                    (GlobalDictionary<IManagedJob>.Store(job)).Schedule(dependsOn);
#else
                return new JobWithHandleReferenceType
#if UNITY_EDITOR
                    <T>
#endif
                    (GCHandle.Alloc(job, GCHandleType.Pinned)).Schedule(dependsOn);
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
                new JobWithKeyValueType<T>(GlobalDictionary<StrongBox<T>>.Store(box)).Run();
#else
                StrongBox<T> box = ObjectPool<StrongBox<T>>.Shared.Rent();
                box.Value = job;
                new JobWithHandleValueType<T>(GCHandle.Alloc(box, GCHandleType.Pinned)).Run();
#endif
            }
            else
            {
#if UNITY_WEBGL
                new JobWithKeyReferenceType
#if UNITY_EDITOR
                    <T>
#endif
                    (GlobalDictionary<IManagedJob>.Store(job)).Run();
#else
                new JobWithHandleReferenceType
#if UNITY_EDITOR
                    <T>
#endif
                    (GCHandle.Alloc(job, GCHandleType.Pinned)).Run();
#endif
            }
        }

#if !UNITY_WEBGL
        private readonly struct JobWithHandleReferenceType
#if UNITY_EDITOR
            <T>
#endif
            : IJob
#if UNITY_EDITOR
             where T : IManagedJob
#endif
        {
            private readonly GCHandle handle;

#if UNITY_EDITOR
            private static int count;

            static JobWithHandleReferenceType() => ManagedJobEditorHelper.AddPoolContainer(GetTypeForContainer<T>().Name, () => count);
#endif

            public JobWithHandleReferenceType(GCHandle handle)
            {
                this.handle = handle;
#if UNITY_EDITOR
                Interlocked.Increment(ref count);
#endif
            }

            public void Execute()
            {
                object target = handle.Target;
                Debug.Assert(target is IManagedJob);
                IManagedJob job = Unsafe.As<IManagedJob>(target);
                handle.Free();
#if UNITY_EDITOR
                try
                {
#endif
                    job.Execute();
#if UNITY_EDITOR
                }
                finally
                {
                    Interlocked.Decrement(ref count);
                }
#endif
            }
        }

        private readonly struct JobWithHandleValueType<T> : IJob where T : IManagedJob
        {
            private readonly GCHandle handle;

#if UNITY_EDITOR
            private static int count;

            static JobWithHandleValueType() => ManagedJobEditorHelper.AddPoolContainer(GetTypeForContainer<T>().Name, () => count);
#endif

            public JobWithHandleValueType(GCHandle handle)
            {
                this.handle = handle;
#if UNITY_EDITOR
                Interlocked.Increment(ref count);
#endif
            }

            public void Execute()
            {
                object target = handle.Target;
                Debug.Assert(target is StrongBox<T>);
                StrongBox<T> box = Unsafe.As<StrongBox<T>>(target);
                handle.Free();
                T job = box.Value;
                ObjectPool<StrongBox<T>>.Shared.Return(box);
#if UNITY_EDITOR
                try
                {
#endif
                    job.Execute();
#if UNITY_EDITOR
                }
                finally
                {
                    Interlocked.Decrement(ref count);
                }
#endif
            }
        }
#else
        private readonly struct JobWithKeyReferenceType
#if UNITY_EDITOR
            <T>
#endif
            : IJob
#if UNITY_EDITOR
            where T : IManagedJob
#endif
        {
            private readonly long key;

#if UNITY_EDITOR
            private static int count;

            static JobWithKeyReferenceType() => ManagedJobEditorHelper.AddPoolContainer(GetTypeForContainer<T>().Name, () => count);
#endif

            public JobWithKeyReferenceType(long key)
            {
                this.key = key;
#if UNITY_EDITOR
                Interlocked.Increment(ref count);
#endif
            }

            public void Execute()
            {
                IManagedJob managedJob = GlobalDictionary<IManagedJob>.Drain(key);
#if UNITY_EDITOR
                try
                {
#endif
                    managedJob.Execute();
#if UNITY_EDITOR
                }
                finally
                {
                    Interlocked.Decrement(ref count);
                }
#endif
            }
        }

        private readonly struct JobWithKeyValueType<T> : IJob where T : IManagedJob
        {
            private readonly long key;

#if UNITY_EDITOR
            private static int count;

            static JobWithKeyValueType() => ManagedJobEditorHelper.AddPoolContainer(GetTypeForContainer<T>().Name, () => count);
#endif

            public JobWithKeyValueType(long key)
            {
                this.key = key;
#if UNITY_EDITOR
                Interlocked.Increment(ref count);
#endif
            }

            public void Execute()
            {
                StrongBox<T> box = GlobalDictionary<StrongBox<T>>.Drain(key);
                T job = box.Value;
                ObjectPool<StrongBox<T>>.Shared.Return(box);
#if UNITY_EDITOR
                try
                {
#endif
                    job.Execute();
#if UNITY_EDITOR
                }
                finally
                {
                    Interlocked.Decrement(ref count);
                }
#endif
            }
        }
#endif

#if UNITY_EDITOR
        private static Type GetTypeForContainer<T>()
        {
            Type type = typeof(T);
            if (type.Assembly == typeof(JobAction).Assembly)
            {
                if (type == typeof(JobAction))
                    type = typeof(Action);
                else if (type.IsGenericType)
                {
                    Type definition = type.GetGenericTypeDefinition();
                    if (definition == typeof(JobAction<>))
                        type = typeof(JobAction<>).MakeGenericType(type.GenericTypeArguments);
                    else if (definition == typeof(JobAction<,>))
                        type = typeof(Action<,>).MakeGenericType(type.GenericTypeArguments);
                    else if (definition == typeof(JobAction<,,>))
                        type = typeof(Action<,,>).MakeGenericType(type.GenericTypeArguments);
                    else if (definition == typeof(JobAction<,,,>))
                        type = typeof(Action<,,,>).MakeGenericType(type.GenericTypeArguments);
                    else if (definition == typeof(JobAction<,,,,>))
                        type = typeof(Action<,,,,>).MakeGenericType(type.GenericTypeArguments);
                    else if (definition == typeof(JobAction<,,,,,>))
                        type = typeof(Action<,,,,,>).MakeGenericType(type.GenericTypeArguments);
                    else if (definition == typeof(JobAction<,,,,,,>))
                        type = typeof(Action<,,,,,,>).MakeGenericType(type.GenericTypeArguments);
                    else if (definition == typeof(JobAction<,,,,,,,>))
                        type = typeof(Action<,,,,,,,>).MakeGenericType(type.GenericTypeArguments);
                    else if (definition == typeof(JobAction<,,,,,,,,>))
                        type = typeof(Action<,,,,,,,,>).MakeGenericType(type.GenericTypeArguments);
                    else if (definition == typeof(JobAction<,,,,,,,,,>))
                        type = typeof(Action<,,,,,,,,,>).MakeGenericType(type.GenericTypeArguments);
                    else if (definition == typeof(JobAction<,,,,,,,,,,>))
                        type = typeof(Action<,,,,,,,,,,>).MakeGenericType(type.GenericTypeArguments);
                    else if (definition == typeof(JobAction<,,,,,,,,,,,>))
                        type = typeof(Action<,,,,,,,,,,,>).MakeGenericType(type.GenericTypeArguments);
                    else if (definition == typeof(JobAction<,,,,,,,,,,,,>))
                        type = typeof(Action<,,,,,,,,,,,,>).MakeGenericType(type.GenericTypeArguments);
                    else if (definition == typeof(JobAction<,,,,,,,,,,,,,>))
                        type = typeof(Action<,,,,,,,,,,,,,>).MakeGenericType(type.GenericTypeArguments);
                    else if (definition == typeof(JobAction<,,,,,,,,,,,,,,>))
                        type = typeof(Action<,,,,,,,,,,,,,,>).MakeGenericType(type.GenericTypeArguments);
                    else if (definition == typeof(JobAction<,,,,,,,,,,,,,,,>))
                        type = typeof(Action<,,,,,,,,,,,,,,,>).MakeGenericType(type.GenericTypeArguments);
                }
            }

            return type;
        }
#endif
    }
}