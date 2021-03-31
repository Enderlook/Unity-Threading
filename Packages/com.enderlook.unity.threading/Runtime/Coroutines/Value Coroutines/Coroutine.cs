using Enderlook.Unity.Jobs;

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    /// <summary>
    /// Helper methods for coroutines.
    /// </summary>
    public static partial class Coroutine
    {
        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method must be executed from the Unity thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Start<T>(T routine) where T : IEnumerator<ValueYieldInstruction>
            => Manager.Shared.CoroutinesManager.Start(routine);

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StartThreadSafe<T>(T routine) where T : IEnumerator<ValueYieldInstruction>
            => Manager.Shared.CoroutinesManager.StartThreadSafe(routine);

        /// <summary>
        /// Start a value coroutine.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine StartWithHandle<T>(T routine) where T : IEnumerator<ValueYieldInstruction>
            => Manager.Shared.CoroutinesManager.StartWithHandle(routine);

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine StartWithHandleThreadSafe<T>(T routine) where T : IEnumerator<ValueYieldInstruction>
            => Manager.Shared.CoroutinesManager.StartWithHandleThreadSafe(routine);

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method must be executed from the Unity thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="cancellator">Cancellator of the coroutine.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Start<T, U>(T routine, U cancellator)
            where T : IEnumerator<ValueYieldInstruction>
            where U : ICancellable
            => Manager.Shared.CoroutinesManager.Start(routine, cancellator);

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="cancellator">Cancellator of the coroutine.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StartThreadSafe<T, U>(T routine, U cancellator)
            where T : IEnumerator<ValueYieldInstruction>
            where U : ICancellable
            => Manager.Shared.CoroutinesManager.StartThreadSafe(routine, cancellator);

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method must be executed from the Unity thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="source"><see cref="Object"/> from which this coroutine is attached to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Start<T>(T routine, Object source) where T : IEnumerator<ValueYieldInstruction>
            => Manager.Shared.CoroutinesManager.Start(routine, source);

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="source"><see cref="Object"/> from which this coroutine is attached to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StartThreadSafe<T>(T routine, Object source) where T : IEnumerator<ValueYieldInstruction>
            => Manager.Shared.CoroutinesManager.StartThreadSafe(routine, source);

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method must be executed from the Unity thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="source"><see cref="Object"/> from which this coroutine is attached to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine StartWithHandle<T>(T routine, Object source) where T : IEnumerator<ValueYieldInstruction>
            => Manager.Shared.CoroutinesManager.StartWithHandle(routine, source);

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="source"><see cref="Object"/> from which this coroutine is attached to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine StartWithHandleThreadSafe<T>(T routine, Object source) where T : IEnumerator<ValueYieldInstruction>
            => Manager.Shared.CoroutinesManager.StartWithHandleThreadSafe(routine, source);

        /// <summary>
        /// Start a value coroutine.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> of this coroutine.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Start<T>(T routine, CancellationToken cancellationToken) where T : IEnumerator<ValueYieldInstruction>
            => Manager.Shared.CoroutinesManager.Start(routine, cancellationToken);

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> of this coroutine.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StartThreadSafe<T>(T routine, CancellationToken cancellationToken) where T : IEnumerator<ValueYieldInstruction>
            => Manager.Shared.CoroutinesManager.StartThreadSafe(routine, cancellationToken);

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method must be executed from the Unity thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> of this coroutine.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine StartWithHandle<T>(T routine, CancellationToken cancellationToken) where T : IEnumerator<ValueYieldInstruction>
            => Manager.Shared.CoroutinesManager.StartWithHandle(routine, cancellationToken);

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> of this coroutine.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine StartWithHandleThreadSafe<T>(T routine, CancellationToken cancellationToken) where T : IEnumerator<ValueYieldInstruction>
            => Manager.Shared.CoroutinesManager.StartWithHandleThreadSafe(routine, cancellationToken);
    }
}