using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using UnityEngine;

using Object = UnityEngine.Object;

namespace Enderlook.Unity.Coroutines
{
    public partial struct CoroutineScheduler
    {
        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.Start{T}(Cores, T)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start<T>(T routine)
            where T : IEnumerator<ValueYieldInstruction>
            => Core.Start(routine);

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.ConcurrentStart{T}(Cores, T)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ConcurrentStart<T>(T routine)
            where T : IEnumerator<ValueYieldInstruction>
            => Core.ConcurrentStart(routine);

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.StartWithHandle{T}(Cores, T)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueCoroutine StartWithHandle<T>(T routine)
            where T : IEnumerator<ValueYieldInstruction>
            => Core.StartWithHandle(routine);

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.ConcurrentStartWithHandle{T}(Cores, T)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueCoroutine ConcurrentStartWithHandle<T>(T routine)
            where T : IEnumerator<ValueYieldInstruction>
            => Core.ConcurrentStartWithHandle(routine);

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.Start{T}(Cores, T, CancellationToken)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start<T>(T routine, CancellationToken token)
            where T : IEnumerator<ValueYieldInstruction>
            => Core.Start(routine, token);

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.ConcurrentStart{T}(Cores, T, CancellationToken)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ConcurrentStart<T>(T routine, CancellationToken token)
            where T : IEnumerator<ValueYieldInstruction>
            => Core.ConcurrentStart(routine, token);

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.StartWithHandle{T}(Cores, T, CancellationToken)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueCoroutine StartWithHandle<T>(T routine, CancellationToken token)
            where T : IEnumerator<ValueYieldInstruction>
            => Core.StartWithHandle(routine, token);

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.ConcurrentStartWithHandle{T}(Cores, T, CancellationToken)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueCoroutine ConcurrentStartWithHandle<T>(T routine, CancellationToken token)
            where T : IEnumerator<ValueYieldInstruction>
            => Core.ConcurrentStartWithHandle(routine, token);

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.Start{T}(Cores, T, Object)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start<T>(T routine, Object source)
            where T : IEnumerator<ValueYieldInstruction>
            => Core.Start(routine, source);

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.ConcurrentStart{T}(Cores, T, Object)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ConcurrentStart<T>(T routine, Object source)
            where T : IEnumerator<ValueYieldInstruction>
            => Core.ConcurrentStart(routine, source);

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.StartWithHandle{T}(Cores, T, Object)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueCoroutine StartWithHandle<T>(T routine, Object source)
            where T : IEnumerator<ValueYieldInstruction>
            => Core.StartWithHandle(routine, source);

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.ConcurrentStartWithHandle{T}(Cores, T, Object)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueCoroutine ConcurrentStartWithHandle<T>(T routine, Object source)
            where T : IEnumerator<ValueYieldInstruction>
            => Core.ConcurrentStartWithHandle(routine, source);

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.Start{T}(Cores, T, GameObject)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start<T>(T routine, GameObject source)
            where T : IEnumerator<ValueYieldInstruction>
            => Core.Start(routine, source);

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.ConcurrentStart{T}(Cores, T, GameObject)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ConcurrentStart<T>(T routine, GameObject source)
            where T : IEnumerator<ValueYieldInstruction>
            => Core.ConcurrentStart(routine, source);

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.StartWithHandle{T}(Cores, T, GameObject)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueCoroutine StartWithHandle<T>(T routine, GameObject source)
            where T : IEnumerator<ValueYieldInstruction>
            => Core.StartWithHandle(routine, source);

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.ConcurrentStartWithHandle{T}(Cores, T, GameObject)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueCoroutine ConcurrentStartWithHandle<T>(T routine, GameObject source)
            where T : IEnumerator<ValueYieldInstruction>
            => Core.ConcurrentStartWithHandle(routine, source);

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.Start{T}(Cores, T, MonoBehaviour, bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start<T>(T routine, MonoBehaviour source, bool suspendWhenSourceIsDisabled = false)
            where T : IEnumerator<ValueYieldInstruction>
            => Core.Start(routine, source, suspendWhenSourceIsDisabled);

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.ConcurrentStart{T}(Cores, T, MonoBehaviour, bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ConcurrentStart<T>(T routine, MonoBehaviour source, bool suspendWhenSourceIsDisabled = false)
            where T : IEnumerator<ValueYieldInstruction>
            => Core.ConcurrentStart(routine, source, suspendWhenSourceIsDisabled);

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.StartWithHandle{T}(Cores, T, MonoBehaviour, bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueCoroutine StartWithHandle<T>(T routine, MonoBehaviour source, bool suspendWhenSourceIsDisabled = false)
            where T : IEnumerator<ValueYieldInstruction>
            => Core.StartWithHandle(routine, source, suspendWhenSourceIsDisabled);

        /// <inheritdoc cref="CoroutineSchedulerCoresExtensions.ConcurrentStartWithHandle{T}(Cores, T, MonoBehaviour, bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueCoroutine ConcurrentStartWithHandle<T>(T routine, MonoBehaviour source, bool suspendWhenSourceIsDisabled = false)
            where T : IEnumerator<ValueYieldInstruction>
            => Core.ConcurrentStartWithHandle(routine, source, suspendWhenSourceIsDisabled);
    }
}