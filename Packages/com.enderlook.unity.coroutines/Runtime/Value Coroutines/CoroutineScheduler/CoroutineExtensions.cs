using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    /// <summary>
    /// Aditional methods for value coroutines.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class CoroutineExtensions
    {
        /// <inheritdoc cref="CoroutineManager.Start{T}(T, GameObject)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StartValueCoroutine<T>(this GameObject source, T routine)
            where T : IEnumerator<ValueYieldInstruction>
           => CoroutineManager.Shared.Start(routine, source);

        /// <inheritdoc cref="CoroutineManager.StartWithValueHandle{T}(T, GameObject)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine StartValueCoroutineWithValueHandle<T>(this GameObject source, T routine)
            where T : IEnumerator<ValueYieldInstruction>
           => CoroutineManager.Shared.StartWithHandle(routine, source);

        /// <inheritdoc cref="CoroutineManager.Start{T}(T, MonoBehaviour, bool)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StartValueCoroutine<T>(this MonoBehaviour source, T routine, bool suspendWhenSourceIsDisabled = false)
            where T : IEnumerator<ValueYieldInstruction>
           => CoroutineManager.Shared.Start(routine, source, suspendWhenSourceIsDisabled);

        /// <inheritdoc cref="CoroutineManager.Start{T}(T, MonoBehaviour, bool)"/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine StartValueCoroutineWithValueHandle<T>(this MonoBehaviour source, T routine, bool suspendWhenSourceIsDisabled = false)
            where T : IEnumerator<ValueYieldInstruction>
           => CoroutineManager.Shared.StartWithHandle(routine, source, suspendWhenSourceIsDisabled);
    }
}