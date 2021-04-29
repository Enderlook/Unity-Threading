﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    /// <summary>
    /// Aditional methods for value coroutines.
    /// </summary>
    public static class CoroutineExtensions
    {
        /// <summary>
        /// Start a value coroutine.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="source"><see cref="MonoBehaviour"/> from which this coroutine is attached to.</param>
        /// <param name="routine">Coroutine to start.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StartValueCoroutine<T>(this MonoBehaviour source, T routine) where T : IEnumerator<ValueYieldInstruction>
            => CoroutineScheduler.Shared.Start(routine, new CancellableUnityObject(source));

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="source"><see cref="MonoBehaviour"/> from which this coroutine is attached to.</param>
        /// <param name="routine">Coroutine to start.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConcurrentStartValueCoroutine<T>(this MonoBehaviour source, T routine) where T : IEnumerator<ValueYieldInstruction>
            => CoroutineScheduler.Shared.ConcurrentStart(routine, new CancellableUnityObject(source));

        /// <summary>
        /// Start a value coroutine.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="source"><see cref="MonoBehaviour"/> from which this coroutine is attached to.</param>
        /// <param name="routine">Coroutine to start.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine StartValueCoroutineWithHandle<T>(this MonoBehaviour source, T routine) where T : IEnumerator<ValueYieldInstruction>
            => CoroutineScheduler.Shared.StartWithHandle(routine, new CancellableUnityObject(source));

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="source"><see cref="MonoBehaviour"/> from which this coroutine is attached to.</param>
        /// <param name="routine">Coroutine to start.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine ConcurrentStartValueCoroutineWithHandle<T>(this MonoBehaviour source, T routine) where T : IEnumerator<ValueYieldInstruction>
            => CoroutineScheduler.Shared.ConcurrentStartWithHandle(routine, new CancellableUnityObject(source));
    }
}