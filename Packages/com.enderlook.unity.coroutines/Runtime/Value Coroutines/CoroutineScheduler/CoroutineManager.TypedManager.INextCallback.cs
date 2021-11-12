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

#if !UNITY_WEBGL
            void ConcurrentSuspend(TypedManager<T> manager, T routine);
#endif
        }

        private partial class TypedManager<T> : ManagerBase
        {
            private readonly struct EntryNextCallback : INextCallback<T>
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Suspend(TypedManager<T> manager, T routine) => manager.suspendedEntry.Add(routine);

#if !UNITY_WEBGL
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void ConcurrentSuspend(TypedManager<T> manager, T routine) => manager.suspendedEntry.ConcurrentAdd(routine);
#endif
            }

            private readonly struct UpdateNextCallback : INextCallback<T>
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Suspend(TypedManager<T> manager, T routine) => manager.onUpdate.Add(routine);

#if !UNITY_WEBGL
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void ConcurrentSuspend(TypedManager<T> manager, T routine) => manager.onUpdate.ConcurrentAdd(routine);
#endif
            }

            private readonly struct LateUpdateNextCallback : INextCallback<T>
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Suspend(TypedManager<T> manager, T routine) => manager.onLateUpdate.Add(routine);

#if !UNITY_WEBGL
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void ConcurrentSuspend(TypedManager<T> manager, T routine) => manager.onLateUpdate.ConcurrentAdd(routine);
#endif
            }

            private readonly struct FixedUpdateNextCallback : INextCallback<T>
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Suspend(TypedManager<T> manager, T routine) => manager.onFixedUpdate.Add(routine);

#if !UNITY_WEBGL
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void ConcurrentSuspend(TypedManager<T> manager, T routine) => manager.onFixedUpdate.ConcurrentAdd(routine);
#endif
            }

            private readonly struct EndOfFrameNextCallback : INextCallback<T>
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Suspend(TypedManager<T> manager, T routine) => manager.onEndOfFrame.Add(routine);

#if !UNITY_WEBGL
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void ConcurrentSuspend(TypedManager<T> manager, T routine) => manager.onEndOfFrame.ConcurrentAdd(routine);
#endif
            }

            private readonly struct PollNextCallback : INextCallback<T>
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Suspend(TypedManager<T> manager, T routine) => manager.onUnityPoll.Enqueue(routine);

#if !UNITY_WEBGL
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void ConcurrentSuspend(TypedManager<T> manager, T routine) => manager.onUnityPoll.ConcurrentEnqueue(routine);
#endif
            }

            private readonly struct WaitForSecondsNextCallback : INextCallback<T>
            {
                private readonly float condition;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public WaitForSecondsNextCallback(float condition) => this.condition = condition;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Suspend(TypedManager<T> manager, T routine) => manager.onWaitSeconds.Add((condition, routine));

#if !UNITY_WEBGL
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void ConcurrentSuspend(TypedManager<T> manager, T routine) => manager.onWaitSeconds.ConcurrentAdd((condition, routine));
#endif
            }

            private readonly struct WaitForRealtimeSecondsNextCallback : INextCallback<T>
            {
                private readonly float condition;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public WaitForRealtimeSecondsNextCallback(float condition) => this.condition = condition;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Suspend(TypedManager<T> manager, T routine) => manager.onWaitRealtimeSeconds.Add((condition, routine));

#if !UNITY_WEBGL
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void ConcurrentSuspend(TypedManager<T> manager, T routine) => manager.onWaitRealtimeSeconds.ConcurrentAdd((condition, routine));
#endif
            }

            private readonly struct CustomNextCallback : INextCallback<T>
            {
                private readonly CustomYieldInstruction condition;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public CustomNextCallback(CustomYieldInstruction condition) => this.condition = condition;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Suspend(TypedManager<T> manager, T routine) => manager.onCustom.Add((condition, routine));

#if !UNITY_WEBGL
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void ConcurrentSuspend(TypedManager<T> manager, T routine) => manager.onCustom.ConcurrentAdd((condition, routine));
#endif
            }

            private readonly struct WhileNextCallback : INextCallback<T>
            {
                private readonly Func<bool> condition;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public WhileNextCallback(Func<bool> condition) => this.condition = condition;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Suspend(TypedManager<T> manager, T routine) => manager.onWhile.Add((condition, routine));

#if !UNITY_WEBGL
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void ConcurrentSuspend(TypedManager<T> manager, T routine) => manager.onWhile.ConcurrentAdd((condition, routine));
#endif
            }

            private readonly struct UntilNextCallback : INextCallback<T>
            {
                private readonly Func<bool> condition;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public UntilNextCallback(Func<bool> condition) => this.condition = condition;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Suspend(TypedManager<T> manager, T routine) => manager.onUntil.Add((condition, routine));

#if !UNITY_WEBGL
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void ConcurrentSuspend(TypedManager<T> manager, T routine) => manager.onUntil.ConcurrentAdd((condition, routine));
#endif
            }

            private readonly struct ValueTaskNextCallback : INextCallback<T>
            {
                private readonly ValueTask condition;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public ValueTaskNextCallback(ValueTask condition) => this.condition = condition;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Suspend(TypedManager<T> manager, T routine) => manager.onTask.Add((condition, routine));

#if !UNITY_WEBGL
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void ConcurrentSuspend(TypedManager<T> manager, T routine) => manager.onTask.ConcurrentAdd((condition, routine));
#endif
            }

            private readonly struct JobHandleNextCallback : INextCallback<T>
            {
                private readonly JobHandle condition;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public JobHandleNextCallback(JobHandle condition) => this.condition = condition;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Suspend(TypedManager<T> manager, T routine) => manager.onJobHandle.Add((condition, routine));

#if !UNITY_WEBGL
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void ConcurrentSuspend(TypedManager<T> manager, T routine) => manager.onJobHandle.ConcurrentAdd((condition, routine));
#endif
            }

            private readonly struct ValueCoroutineNextCallback : INextCallback<T>
            {
                private readonly ValueCoroutine condition;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public ValueCoroutineNextCallback(ValueCoroutine condition) => this.condition = condition;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Suspend(TypedManager<T> manager, T routine) => manager.onValueCoroutine.Add((condition, routine));

#if !UNITY_WEBGL
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void ConcurrentSuspend(TypedManager<T> manager, T routine) => manager.onValueCoroutine.ConcurrentAdd((condition, routine));
#endif
            }

#if !UNITY_WEBGL
            private readonly struct BackgroundShortNextCallback : INextCallback<T>
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Suspend(TypedManager<T> manager, T routine) => manager.suspendedBackgroundShort.Enqueue(routine);

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void ConcurrentSuspend(TypedManager<T> manager, T routine) => manager.suspendedBackgroundShort.Enqueue(routine);
            }

            private readonly struct BackgroundLongNextCallback : INextCallback<T>
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Suspend(TypedManager<T> manager, T routine) => manager.suspendedBackgroundLong.Enqueue(routine);

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void ConcurrentSuspend(TypedManager<T> manager, T routine) => manager.suspendedBackgroundLong.Enqueue(routine);
            }
#endif
        }
    }
}