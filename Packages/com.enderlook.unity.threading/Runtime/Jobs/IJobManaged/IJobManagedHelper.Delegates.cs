using System;

using Unity.Jobs;

namespace Enderlook.Unity.Threading.Jobs
{
    public static partial class IJobManagedHelper
    {
        /// <inheritdoc cref="IJobExtensions.Schedule{T}(T, JobHandle)"/>
        public static JobHandle Schedule(this Action job, JobHandle dependsOn = default)
            => new JobAction(job).Schedule(dependsOn);

        /// <inheritdoc cref="IJobExtensions.Schedule{T}(T, JobHandle)"/>
        public static JobHandle Schedule<T1>(this Action<T1> job, T1 p1, JobHandle dependsOn = default)
            => new JobAction<T1>(job, p1).Schedule(dependsOn);

        /// <inheritdoc cref="IJobExtensions.Schedule{T}(T, JobHandle)"/>
        public static JobHandle Schedule<T1, T2>(this Action<T1, T2> job, T1 p1, T2 p2, JobHandle dependsOn = default)
            => new JobAction<T1, T2>(job, p1, p2).Schedule(dependsOn);

        /// <inheritdoc cref="IJobExtensions.Schedule{T}(T, JobHandle)"/>
        public static JobHandle Schedule<T1, T2, T3>(this Action<T1, T2, T3> job, T1 p1, T2 p2, T3 p3, JobHandle dependsOn = default)
            => new JobAction<T1, T2, T3>(job, p1, p2, p3).Schedule(dependsOn);

        /// <inheritdoc cref="IJobExtensions.Schedule{T}(T, JobHandle)"/>
        public static JobHandle Schedule<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> job, T1 p1, T2 p2, T3 p3, T4 p4, JobHandle dependsOn = default)
            => new JobAction<T1, T2, T3, T4>(job, p1, p2, p3, p4).Schedule(dependsOn);

        /// <inheritdoc cref="IJobExtensions.Schedule{T}(T, JobHandle)"/>
        public static JobHandle Schedule<T1, T2, T3, T4, T5>(this Action<T1, T2, T3, T4, T5> job, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, JobHandle dependsOn = default)
            => new JobAction<T1, T2, T3, T4, T5>(job, p1, p2, p3, p4, p5).Schedule(dependsOn);

        /// <inheritdoc cref="IJobExtensions.Schedule{T}(T, JobHandle)"/>
        public static JobHandle Schedule<T1, T2, T3, T4, T5, T6>(this Action<T1, T2, T3, T4, T5, T6> job, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, JobHandle dependsOn = default)
            => new JobAction<T1, T2, T3, T4, T5, T6>(job, p1, p2, p3, p4, p5, p6).Schedule(dependsOn);

        /// <inheritdoc cref="IJobExtensions.Schedule{T}(T, JobHandle)"/>
        public static JobHandle Schedule<T1, T2, T3, T4, T5, T6, T7>(this Action<T1, T2, T3, T4, T5, T6, T7> job, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, JobHandle dependsOn = default)
            => new JobAction<T1, T2, T3, T4, T5, T6, T7>(job, p1, p2, p3, p4, p5, p6, p7).Schedule(dependsOn);

        /// <inheritdoc cref="IJobExtensions.Schedule{T}(T, JobHandle)"/>
        public static JobHandle Schedule<T1, T2, T3, T4, T5, T6, T7, T8>(this Action<T1, T2, T3, T4, T5, T6, T7, T8> job, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, JobHandle dependsOn = default)
            => new JobAction<T1, T2, T3, T4, T5, T6, T7, T8>(job, p1, p2, p3, p4, p5, p6, p7, p8).Schedule(dependsOn);

        /// <inheritdoc cref="IJobExtensions.Schedule{T}(T, JobHandle)"/>
        public static JobHandle Schedule<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> job, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, JobHandle dependsOn = default)
            => new JobAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>(job, p1, p2, p3, p4, p5, p6, p7, p8, p9).Schedule(dependsOn);

        /// <inheritdoc cref="IJobExtensions.Schedule{T}(T, JobHandle)"/>
        public static JobHandle Schedule<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> job, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, JobHandle dependsOn = default)
            => new JobAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(job, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10).Schedule(dependsOn);

        /// <inheritdoc cref="IJobExtensions.Schedule{T}(T, JobHandle)"/>
        public static JobHandle Schedule<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> job, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, JobHandle dependsOn = default)
            => new JobAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(job, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11).Schedule(dependsOn);

        /// <inheritdoc cref="IJobExtensions.Schedule{T}(T, JobHandle)"/>
        public static JobHandle Schedule<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> job, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, JobHandle dependsOn = default)
            => new JobAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(job, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12).Schedule(dependsOn);

        /// <inheritdoc cref="IJobExtensions.Schedule{T}(T, JobHandle)"/>
        public static JobHandle Schedule<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> job, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, JobHandle dependsOn = default)
            => new JobAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(job, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13).Schedule(dependsOn);

        /// <inheritdoc cref="IJobExtensions.Schedule{T}(T, JobHandle)"/>
        public static JobHandle Schedule<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> job, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, JobHandle dependsOn = default)
            => new JobAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(job, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14).Schedule(dependsOn);

        /// <inheritdoc cref="IJobExtensions.Schedule{T}(T, JobHandle)"/>
        public static JobHandle Schedule<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> job, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15, JobHandle dependsOn = default)
            => new JobAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(job, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15).Schedule(dependsOn);

        /// <inheritdoc cref="IJobExtensions.Schedule{T}(T, JobHandle)"/>
        public static JobHandle Schedule<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(this Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> job, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15, T16 p16, JobHandle dependsOn = default)
            => new JobAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(job, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16).Schedule(dependsOn);

        private readonly struct JobAction : IJobManaged
        {
            private readonly Action action;

            public JobAction(Action action) => this.action = action;

            public void Execute() => action();
        }

        private readonly struct JobAction<T1> : IJobManaged
        {
            private readonly Action<T1> action;
            private readonly T1 p1;

            public JobAction(Action<T1> action, T1 p1)
            {
                this.action = action;
                this.p1 = p1;
            }

            public void Execute() => action(p1);
        }

        private readonly struct JobAction<T1, T2> : IJobManaged
        {
            private readonly Action<T1, T2> action;
            private readonly T1 p1;
            private readonly T2 p2;

            public JobAction(Action<T1, T2> action, T1 p1, T2 p2)
            {
                this.action = action;
                this.p1 = p1;
                this.p2 = p2;
            }

            public void Execute() => action(p1, p2);
        }

        private readonly struct JobAction<T1, T2, T3> : IJobManaged
        {
            private readonly Action<T1, T2, T3> action;
            private readonly T1 p1;
            private readonly T2 p2;
            private readonly T3 p3;

            public JobAction(Action<T1, T2, T3> action, T1 p1, T2 p2, T3 p3)
            {
                this.action = action;
                this.p1 = p1;
                this.p2 = p2;
                this.p3 = p3;
            }

            public void Execute() => action(p1, p2, p3);
        }

        private readonly struct JobAction<T1, T2, T3, T4> : IJobManaged
        {
            private readonly Action<T1, T2, T3, T4> action;
            private readonly T1 p1;
            private readonly T2 p2;
            private readonly T3 p3;
            private readonly T4 p4;

            public JobAction(Action<T1, T2, T3, T4> action, T1 p1, T2 p2, T3 p3, T4 p4)
            {
                this.action = action;
                this.p1 = p1;
                this.p2 = p2;
                this.p3 = p3;
                this.p4 = p4;
            }

            public void Execute() => action(p1, p2, p3, p4);
        }

        private readonly struct JobAction<T1, T2, T3, T4, T5> : IJobManaged
        {
            private readonly Action<T1, T2, T3, T4, T5> action;
            private readonly T1 p1;
            private readonly T2 p2;
            private readonly T3 p3;
            private readonly T4 p4;
            private readonly T5 p5;

            public JobAction(Action<T1, T2, T3, T4, T5> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5)
            {
                this.action = action;
                this.p1 = p1;
                this.p2 = p2;
                this.p3 = p3;
                this.p4 = p4;
                this.p5 = p5;
            }

            public void Execute() => action(p1, p2, p3, p4, p5);
        }

        private readonly struct JobAction<T1, T2, T3, T4, T5, T6> : IJobManaged
        {
            private readonly Action<T1, T2, T3, T4, T5, T6> action;
            private readonly T1 p1;
            private readonly T2 p2;
            private readonly T3 p3;
            private readonly T4 p4;
            private readonly T5 p5;
            private readonly T6 p6;

            public JobAction(Action<T1, T2, T3, T4, T5, T6> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6)
            {
                this.action = action;
                this.p1 = p1;
                this.p2 = p2;
                this.p3 = p3;
                this.p4 = p4;
                this.p5 = p5;
                this.p6 = p6;
            }

            public void Execute() => action(p1, p2, p3, p4, p5, p6);
        }

        private readonly struct JobAction<T1, T2, T3, T4, T5, T6, T7> : IJobManaged
        {
            private readonly Action<T1, T2, T3, T4, T5, T6, T7> action;
            private readonly T1 p1;
            private readonly T2 p2;
            private readonly T3 p3;
            private readonly T4 p4;
            private readonly T5 p5;
            private readonly T6 p6;
            private readonly T7 p7;

            public JobAction(Action<T1, T2, T3, T4, T5, T6, T7> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7)
            {
                this.action = action;
                this.p1 = p1;
                this.p2 = p2;
                this.p3 = p3;
                this.p4 = p4;
                this.p5 = p5;
                this.p6 = p6;
                this.p7 = p7;
            }

            public void Execute() => action(p1, p2, p3, p4, p5, p6, p7);
        }

        private readonly struct JobAction<T1, T2, T3, T4, T5, T6, T7, T8> : IJobManaged
        {
            private readonly Action<T1, T2, T3, T4, T5, T6, T7, T8> action;
            private readonly T1 p1;
            private readonly T2 p2;
            private readonly T3 p3;
            private readonly T4 p4;
            private readonly T5 p5;
            private readonly T6 p6;
            private readonly T7 p7;
            private readonly T8 p8;

            public JobAction(Action<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8)
            {
                this.action = action;
                this.p1 = p1;
                this.p2 = p2;
                this.p3 = p3;
                this.p4 = p4;
                this.p5 = p5;
                this.p6 = p6;
                this.p7 = p7;
                this.p8 = p8;
            }

            public void Execute() => action(p1, p2, p3, p4, p5, p6, p7, p8);
        }

        private readonly struct JobAction<T1, T2, T3, T4, T5, T6, T7, T8, T9> : IJobManaged
        {
            private readonly Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action;
            private readonly T1 p1;
            private readonly T2 p2;
            private readonly T3 p3;
            private readonly T4 p4;
            private readonly T5 p5;
            private readonly T6 p6;
            private readonly T7 p7;
            private readonly T8 p8;
            private readonly T9 p9;

            public JobAction(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9)
            {
                this.action = action;
                this.p1 = p1;
                this.p2 = p2;
                this.p3 = p3;
                this.p4 = p4;
                this.p5 = p5;
                this.p6 = p6;
                this.p7 = p7;
                this.p8 = p8;
                this.p9 = p9;
            }

            public void Execute() => action(p1, p2, p3, p4, p5, p6, p7, p8, p9);
        }

        private readonly struct JobAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> : IJobManaged
        {
            private readonly Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action;
            private readonly T1 p1;
            private readonly T2 p2;
            private readonly T3 p3;
            private readonly T4 p4;
            private readonly T5 p5;
            private readonly T6 p6;
            private readonly T7 p7;
            private readonly T8 p8;
            private readonly T9 p9;
            private readonly T10 p10;

            public JobAction(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10)
            {
                this.action = action;
                this.p1 = p1;
                this.p2 = p2;
                this.p3 = p3;
                this.p4 = p4;
                this.p5 = p5;
                this.p6 = p6;
                this.p7 = p7;
                this.p8 = p8;
                this.p9 = p9;
                this.p10 = p10;
            }

            public void Execute() => action(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10);
        }

        private readonly struct JobAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> : IJobManaged
        {
            private readonly Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action;
            private readonly T1 p1;
            private readonly T2 p2;
            private readonly T3 p3;
            private readonly T4 p4;
            private readonly T5 p5;
            private readonly T6 p6;
            private readonly T7 p7;
            private readonly T8 p8;
            private readonly T9 p9;
            private readonly T10 p10;
            private readonly T11 p11;

            public JobAction(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11)
            {
                this.action = action;
                this.p1 = p1;
                this.p2 = p2;
                this.p3 = p3;
                this.p4 = p4;
                this.p5 = p5;
                this.p6 = p6;
                this.p7 = p7;
                this.p8 = p8;
                this.p9 = p9;
                this.p10 = p10;
                this.p11 = p11;
            }

            public void Execute() => action(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11);
        }

        private readonly struct JobAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> : IJobManaged
        {
            private readonly Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action;
            private readonly T1 p1;
            private readonly T2 p2;
            private readonly T3 p3;
            private readonly T4 p4;
            private readonly T5 p5;
            private readonly T6 p6;
            private readonly T7 p7;
            private readonly T8 p8;
            private readonly T9 p9;
            private readonly T10 p10;
            private readonly T11 p11;
            private readonly T12 p12;

            public JobAction(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12)
            {
                this.action = action;
                this.p1 = p1;
                this.p2 = p2;
                this.p3 = p3;
                this.p4 = p4;
                this.p5 = p5;
                this.p6 = p6;
                this.p7 = p7;
                this.p8 = p8;
                this.p9 = p9;
                this.p10 = p10;
                this.p11 = p11;
                this.p12 = p12;
            }

            public void Execute() => action(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12);
        }

        private readonly struct JobAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> : IJobManaged
        {
            private readonly Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action;
            private readonly T1 p1;
            private readonly T2 p2;
            private readonly T3 p3;
            private readonly T4 p4;
            private readonly T5 p5;
            private readonly T6 p6;
            private readonly T7 p7;
            private readonly T8 p8;
            private readonly T9 p9;
            private readonly T10 p10;
            private readonly T11 p11;
            private readonly T12 p12;
            private readonly T13 p13;

            public JobAction(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13)
            {
                this.action = action;
                this.p1 = p1;
                this.p2 = p2;
                this.p3 = p3;
                this.p4 = p4;
                this.p5 = p5;
                this.p6 = p6;
                this.p7 = p7;
                this.p8 = p8;
                this.p9 = p9;
                this.p10 = p10;
                this.p11 = p11;
                this.p12 = p12;
                this.p13 = p13;
            }

            public void Execute() => action(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13);
        }

        private readonly struct JobAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> : IJobManaged
        {
            private readonly Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action;
            private readonly T1 p1;
            private readonly T2 p2;
            private readonly T3 p3;
            private readonly T4 p4;
            private readonly T5 p5;
            private readonly T6 p6;
            private readonly T7 p7;
            private readonly T8 p8;
            private readonly T9 p9;
            private readonly T10 p10;
            private readonly T11 p11;
            private readonly T12 p12;
            private readonly T13 p13;
            private readonly T14 p14;

            public JobAction(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14)
            {
                this.action = action;
                this.p1 = p1;
                this.p2 = p2;
                this.p3 = p3;
                this.p4 = p4;
                this.p5 = p5;
                this.p6 = p6;
                this.p7 = p7;
                this.p8 = p8;
                this.p9 = p9;
                this.p10 = p10;
                this.p11 = p11;
                this.p12 = p12;
                this.p13 = p13;
                this.p14 = p14;
            }

            public void Execute() => action(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14);
        }

        private readonly struct JobAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> : IJobManaged
        {
            private readonly Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> action;
            private readonly T1 p1;
            private readonly T2 p2;
            private readonly T3 p3;
            private readonly T4 p4;
            private readonly T5 p5;
            private readonly T6 p6;
            private readonly T7 p7;
            private readonly T8 p8;
            private readonly T9 p9;
            private readonly T10 p10;
            private readonly T11 p11;
            private readonly T12 p12;
            private readonly T13 p13;
            private readonly T14 p14;
            private readonly T15 p15;

            public JobAction(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15)
            {
                this.action = action;
                this.p1 = p1;
                this.p2 = p2;
                this.p3 = p3;
                this.p4 = p4;
                this.p5 = p5;
                this.p6 = p6;
                this.p7 = p7;
                this.p8 = p8;
                this.p9 = p9;
                this.p10 = p10;
                this.p11 = p11;
                this.p12 = p12;
                this.p13 = p13;
                this.p14 = p14;
                this.p15 = p15;
            }

            public void Execute() => action(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15);
        }

        private readonly struct JobAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> : IJobManaged
        {
            private readonly Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> action;
            private readonly T1 p1;
            private readonly T2 p2;
            private readonly T3 p3;
            private readonly T4 p4;
            private readonly T5 p5;
            private readonly T6 p6;
            private readonly T7 p7;
            private readonly T8 p8;
            private readonly T9 p9;
            private readonly T10 p10;
            private readonly T11 p11;
            private readonly T12 p12;
            private readonly T13 p13;
            private readonly T14 p14;
            private readonly T15 p15;
            private readonly T16 p16;

            public JobAction(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> action, T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15, T16 p16)
            {
                this.action = action;
                this.p1 = p1;
                this.p2 = p2;
                this.p3 = p3;
                this.p4 = p4;
                this.p5 = p5;
                this.p6 = p6;
                this.p7 = p7;
                this.p8 = p8;
                this.p9 = p9;
                this.p10 = p10;
                this.p11 = p11;
                this.p12 = p12;
                this.p13 = p13;
                this.p14 = p14;
                this.p15 = p15;
                this.p16 = p16;
            }

            public void Execute() => action(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16);
        }
    }
}