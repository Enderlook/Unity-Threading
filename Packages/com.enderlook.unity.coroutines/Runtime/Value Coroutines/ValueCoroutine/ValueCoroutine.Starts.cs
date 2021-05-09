using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    public partial struct ValueCoroutine
    {
        /// <summary>
        /// Amount of miliseconds spent in executing global poll coroutines per frame.
        /// </summary>
        public static int MilisecondsExecutedPerFrameOnPoll {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => CoroutineManager.Shared.MilisecondsExecutedPerFrameOnPoll;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => CoroutineManager.Shared.MilisecondsExecutedPerFrameOnPoll = value;
        }

        /// <summary>
        /// Percentage of total execution that must be executed on per frame regardless of <see cref="MilisecondsExecutedPerFrameOnPoll"/> for global poll coroutines.
        /// </summary>
        public static float MinimumPercentOfExecutionsPerFrameOnPoll {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => CoroutineManager.Shared.MinimumPercentOfExecutionsPerFrameOnPoll;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => CoroutineManager.Shared.MinimumPercentOfExecutionsPerFrameOnPoll = value;
        }

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.Start{T}(Cores, T)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Start<T>(T routine)
            where T : IEnumerator<ValueYieldInstruction>
            => CoroutineManager.Shared.Start(routine);

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.ConcurrentStart{T}(Cores, T)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConcurrentStart<T>(T routine)
            where T : IEnumerator<ValueYieldInstruction>
            => CoroutineManager.Shared.ConcurrentStart(routine);

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.StartWithHandle{T}(Cores, T)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine StartWithHandle<T>(T routine)
            where T : IEnumerator<ValueYieldInstruction>
            => CoroutineManager.Shared.StartWithHandle(routine);

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.ConcurrentStartWithHandle{T}(Cores, T)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine ConcurrentStartWithHandle<T>(T routine)
            where T : IEnumerator<ValueYieldInstruction>
            => CoroutineManager.Shared.ConcurrentStartWithHandle(routine);

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.Start{T}(Cores, T, CancellationToken)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Start<T>(T routine, CancellationToken token)
            where T : IEnumerator<ValueYieldInstruction>
            => CoroutineManager.Shared.Start(routine, token);

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.ConcurrentStart{T}(Cores, T, CancellationToken)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConcurrentStart<T>(T routine, CancellationToken token)
            where T : IEnumerator<ValueYieldInstruction>
            => CoroutineManager.Shared.ConcurrentStart(routine, token);

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.StartWithHandle{T}(Cores, T, CancellationToken)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine StartWithHandle<T>(T routine, CancellationToken token)
            where T : IEnumerator<ValueYieldInstruction>
            => CoroutineManager.Shared.StartWithHandle(routine, token);

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.ConcurrentStartWithHandle{T}(Cores, T, CancellationToken)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine ConcurrentStartWithHandle<T>(T routine, CancellationToken token)
            where T : IEnumerator<ValueYieldInstruction>
            => CoroutineManager.Shared.ConcurrentStartWithHandle(routine, token);

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.Start{T}(Cores, T, Object)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Start<T>(T routine, Object source)
            where T : IEnumerator<ValueYieldInstruction>
            => CoroutineManager.Shared.Start(routine, source);

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.ConcurrentStart{T}(Cores, T, Object)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConcurrentStart<T>(T routine, Object source)
            where T : IEnumerator<ValueYieldInstruction>
            => CoroutineManager.Shared.ConcurrentStart(routine, source);

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.StartWithHandle{T}(Cores, T, Object)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine StartWithHandle<T>(T routine, Object source)
            where T : IEnumerator<ValueYieldInstruction>
            => CoroutineManager.Shared.StartWithHandle(routine, source);

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.ConcurrentStartWithHandle{T}(Cores, T, Object)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine ConcurrentStartWithHandle<T>(T routine, Object source)
            where T : IEnumerator<ValueYieldInstruction>
            => CoroutineManager.Shared.ConcurrentStartWithHandle(routine, source);

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.Start{T}(Cores, T, GameObject)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Start<T>(T routine, GameObject source)
            where T : IEnumerator<ValueYieldInstruction>
            => CoroutineManager.Shared.Start(routine, source);

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.ConcurrentStart{T}(Cores, T, GameObject)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConcurrentStart<T>(T routine, GameObject source)
            where T : IEnumerator<ValueYieldInstruction>
            => CoroutineManager.Shared.ConcurrentStart(routine, source);

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.StartWithHandle{T}(Cores, T, GameObject)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine StartWithHandle<T>(T routine, GameObject source)
            where T : IEnumerator<ValueYieldInstruction>
            => CoroutineManager.Shared.StartWithHandle(routine, source);

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.ConcurrentStartWithHandle{T}(Cores, T, GameObject)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine ConcurrentStartWithHandle<T>(T routine, GameObject source)
            where T : IEnumerator<ValueYieldInstruction>
            => CoroutineManager.Shared.ConcurrentStartWithHandle(routine, source);

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.Start{T}(Cores, T, MonoBehaviour, bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Start<T>(T routine, MonoBehaviour source, bool suspendWhenSourceIsDisabled = false)
            where T : IEnumerator<ValueYieldInstruction>
            => CoroutineManager.Shared.Start(routine, source, suspendWhenSourceIsDisabled);

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.ConcurrentStart{T}(Cores, T, MonoBehaviour, bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConcurrentStart<T>(T routine, MonoBehaviour source, bool suspendWhenSourceIsDisabled = false)
            where T : IEnumerator<ValueYieldInstruction>
            => CoroutineManager.Shared.ConcurrentStart(routine, source, suspendWhenSourceIsDisabled);

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.StartWithHandle{T}(Cores, T, MonoBehaviour, bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine StartWithHandle<T>(T routine, MonoBehaviour source, bool suspendWhenSourceIsDisabled = false)
            where T : IEnumerator<ValueYieldInstruction>
            => CoroutineManager.Shared.StartWithHandle(routine, source, suspendWhenSourceIsDisabled);

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.ConcurrentStartWithHandle{T}(Cores, T, MonoBehaviour, bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine ConcurrentStartWithHandle<T>(T routine, MonoBehaviour source, bool suspendWhenSourceIsDisabled = false)
            where T : IEnumerator<ValueYieldInstruction>
            => CoroutineManager.Shared.ConcurrentStartWithHandle(routine, source, suspendWhenSourceIsDisabled);
    }
}