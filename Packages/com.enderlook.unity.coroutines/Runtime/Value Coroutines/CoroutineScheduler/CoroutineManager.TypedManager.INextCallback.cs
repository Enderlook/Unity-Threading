using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Unity.Jobs;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    public sealed partial class CoroutineManager
    {
        private interface INextCallback<T> where T : IValueCoroutineEnumerator
        {
            void Suspend(TypedManager<T> manager, T routine);

            void ConcurrentSuspend(TypedManager<T> manager, T routine);
        }

        private partial class TypedManager<T> : ManagerBase
        {
            private readonly struct EntryNextCallback : INextCallback<T>
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Suspend(TypedManager<T> manager, T routine) => manager.suspendedEntry.Add(routine);

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void ConcurrentSuspend(TypedManager<T> manager, T routine) => manager.suspendedEntry.ConcurrentAdd(routine);
            }

            private readonly struct UpdateNextCallback : INextCallback<T>
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Suspend(TypedManager<T> manager, T routine) => manager.onUpdate.Add(routine);

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void ConcurrentSuspend(TypedManager<T> manager, T routine) => manager.onUpdate.ConcurrentAdd(routine);
            }

            private readonly struct LateUpdateNextCallback : INextCallback<T>
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Suspend(TypedManager<T> manager, T routine) => manager.onLateUpdate.Add(routine);

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void ConcurrentSuspend(TypedManager<T> manager, T routine) => manager.onLateUpdate.ConcurrentAdd(routine);
            }

            private readonly struct FixedUpdateNextCallback : INextCallback<T>
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Suspend(TypedManager<T> manager, T routine) => manager.onFixedUpdate.Add(routine);

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void ConcurrentSuspend(TypedManager<T> manager, T routine) => manager.onFixedUpdate.ConcurrentAdd(routine);
            }

            private readonly struct EndOfFrameNextCallback : INextCallback<T>
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Suspend(TypedManager<T> manager, T routine) => manager.onEndOfFrame.Add(routine);

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void ConcurrentSuspend(TypedManager<T> manager, T routine) => manager.onEndOfFrame.ConcurrentAdd(routine);
            }

            private readonly struct PollNextCallback : INextCallback<T>
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Suspend(TypedManager<T> manager, T routine) => manager.onUnityPoll.Enqueue(routine);

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void ConcurrentSuspend(TypedManager<T> manager, T routine) => manager.onUnityPoll.ConcurrentEnqueue(routine);
            }

            private readonly struct WaitForSecondsNextCallback : INextCallback<T>
            {
                private readonly float condition;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public WaitForSecondsNextCallback(float condition) => this.condition = condition;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Suspend(TypedManager<T> manager, T routine) => manager.onWaitSeconds.Add((condition, routine));

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void ConcurrentSuspend(TypedManager<T> manager, T routine) => manager.onWaitSeconds.ConcurrentAdd((condition, routine));
            }

            private readonly struct WaitForRealtimeSecondsNextCallback : INextCallback<T>
            {
                private readonly float condition;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public WaitForRealtimeSecondsNextCallback(float condition) => this.condition = condition;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Suspend(TypedManager<T> manager, T routine) => manager.onWaitRealtimeSeconds.Add((condition, routine));

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void ConcurrentSuspend(TypedManager<T> manager, T routine) => manager.onWaitRealtimeSeconds.ConcurrentAdd((condition, routine));
            }

            private readonly struct CustomNextCallback : INextCallback<T>
            {
                private readonly CustomYieldInstruction condition;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public CustomNextCallback(CustomYieldInstruction condition) => this.condition = condition;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Suspend(TypedManager<T> manager, T routine) => manager.onCustom.Add((condition, routine));

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void ConcurrentSuspend(TypedManager<T> manager, T routine) => manager.onCustom.ConcurrentAdd((condition, routine));
            }

            private readonly struct WhileNextCallback : INextCallback<T>
            {
                private readonly Func<bool> condition;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public WhileNextCallback(Func<bool> condition) => this.condition = condition;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Suspend(TypedManager<T> manager, T routine) => manager.onWhile.Add((condition, routine));

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void ConcurrentSuspend(TypedManager<T> manager, T routine) => manager.onWhile.ConcurrentAdd((condition, routine));
            }

            private readonly struct UntilNextCallback : INextCallback<T>
            {
                private readonly Func<bool> condition;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public UntilNextCallback(Func<bool> condition) => this.condition = condition;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Suspend(TypedManager<T> manager, T routine) => manager.onUntil.Add((condition, routine));

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void ConcurrentSuspend(TypedManager<T> manager, T routine) => manager.onUntil.ConcurrentAdd((condition, routine));
            }

            private readonly struct ValueTaskNextCallback : INextCallback<T>
            {
                private readonly ValueTask condition;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public ValueTaskNextCallback(ValueTask condition) => this.condition = condition;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Suspend(TypedManager<T> manager, T routine) => manager.onTask.Add((condition, routine));

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void ConcurrentSuspend(TypedManager<T> manager, T routine) => manager.onTask.ConcurrentAdd((condition, routine));
            }

            private readonly struct JobHandleNextCallback : INextCallback<T>
            {
                private readonly JobHandle condition;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public JobHandleNextCallback(JobHandle condition) => this.condition = condition;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Suspend(TypedManager<T> manager, T routine) => manager.onJobHandle.Add((condition, routine));

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void ConcurrentSuspend(TypedManager<T> manager, T routine) => manager.onJobHandle.ConcurrentAdd((condition, routine));
            }

            private readonly struct ValueCoroutineNextCallback : INextCallback<T>
            {
                private readonly ValueCoroutine condition;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public ValueCoroutineNextCallback(ValueCoroutine condition) => this.condition = condition;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Suspend(TypedManager<T> manager, T routine) => manager.onValueCoroutine.Add((condition, routine));

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void ConcurrentSuspend(TypedManager<T> manager, T routine) => manager.onValueCoroutine.ConcurrentAdd((condition, routine));
            }

            private readonly struct BackgroundShortNextCallback : INextCallback<T>
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Suspend(TypedManager<T> manager, T routine)
                {
#if DEBUG && UNITY_WEBGL
                    Debug.Assert(false);
#endif
                    manager.suspendedBackgroundShort.Enqueue(routine);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void ConcurrentSuspend(TypedManager<T> manager, T routine)
                {
#if DEBUG && UNITY_WEBGL
                    Debug.Assert(false);
#endif
                    manager.suspendedBackgroundShort.Enqueue(routine);
                }
            }

            private readonly struct BackgroundLongNextCallback : INextCallback<T>
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Suspend(TypedManager<T> manager, T routine)
                {
#if DEBUG && UNITY_WEBGL
                    Debug.Assert(false);
#endif
                    manager.suspendedBackgroundLong.Enqueue(routine);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void ConcurrentSuspend(TypedManager<T> manager, T routine)
                {
#if DEBUG && UNITY_WEBGL
                    Debug.Assert(false);
#endif
                    manager.suspendedBackgroundLong.Enqueue(routine);
                }
            }
        }
    }
}