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
        /// Amount of miliseconds spent in executing global poll coroutines per frame.
        /// </summary>
        public static int MilisecondsExecutedPerFrameOnPoll {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => CoroutineScheduler.Shared.MilisecondsExecutedPerFrameOnPoll;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => CoroutineScheduler.Shared.MilisecondsExecutedPerFrameOnPoll = value;
        }

        /// <summary>
        /// Percentage of total execution that must be executed on per frame regardless of <see cref="MilisecondsExecutedPerFrameOnPoll"/> for global poll coroutines.
        /// </summary>
        public static float MinimumPercentOfExecutionsPerFrameOnPoll {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => CoroutineScheduler.Shared.MinimumPercentOfExecutionsPerFrameOnPoll;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => CoroutineScheduler.Shared.MinimumPercentOfExecutionsPerFrameOnPoll = value;
        }

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method must be executed from the Unity thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Start<T>(T routine) where T : IEnumerator<ValueYieldInstruction>
            => CoroutineScheduler.Shared.Start(new Uncancellable(), routine);

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StartThreadSafe<T>(T routine) where T : IEnumerator<ValueYieldInstruction>
            => CoroutineScheduler.Shared.StartThreadSafe(new Uncancellable(), routine);

        /// <summary>
        /// Start a value coroutine.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine StartWithHandle<T>(T routine) where T : IEnumerator<ValueYieldInstruction>
            => CoroutineScheduler.Shared.StartWithHandle(new Uncancellable(), routine);

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine StartWithHandleThreadSafe<T>(T routine) where T : IEnumerator<ValueYieldInstruction>
            => CoroutineScheduler.Shared.StartWithHandleThreadSafe(new Uncancellable(), routine);

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
            => CoroutineScheduler.Shared.Start(cancellator, routine);

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
            => CoroutineScheduler.Shared.StartThreadSafe(cancellator, routine);

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method must be executed from the Unity thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="source"><see cref="Object"/> from which this coroutine is attached to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Start<T>(T routine, Object source) where T : IEnumerator<ValueYieldInstruction>
            => CoroutineScheduler.Shared.Start(new CancellableUnityObject(source), routine);

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="source"><see cref="Object"/> from which this coroutine is attached to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StartThreadSafe<T>(T routine, Object source) where T : IEnumerator<ValueYieldInstruction>
            => CoroutineScheduler.Shared.StartThreadSafe(new CancellableUnityObject(source), routine);

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method must be executed from the Unity thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="source"><see cref="Object"/> from which this coroutine is attached to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine StartWithHandle<T>(T routine, Object source) where T : IEnumerator<ValueYieldInstruction>
            => CoroutineScheduler.Shared.StartWithHandle(new CancellableUnityObject(source), routine);

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="source"><see cref="Object"/> from which this coroutine is attached to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine StartWithHandleThreadSafe<T>(T routine, Object source) where T : IEnumerator<ValueYieldInstruction>
            => CoroutineScheduler.Shared.StartWithHandleThreadSafe(new CancellableUnityObject(source), routine);

        /// <summary>
        /// Start a value coroutine.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> of this coroutine.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Start<T>(T routine, CancellationToken cancellationToken) where T : IEnumerator<ValueYieldInstruction>
            => CoroutineScheduler.Shared.Start(new CancellableCancellationToken(cancellationToken), routine);

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> of this coroutine.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StartThreadSafe<T>(T routine, CancellationToken cancellationToken) where T : IEnumerator<ValueYieldInstruction>
            => CoroutineScheduler.Shared.StartThreadSafe(new CancellableCancellationToken(cancellationToken), routine);

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method must be executed from the Unity thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> of this coroutine.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine StartWithHandle<T>(T routine, CancellationToken cancellationToken) where T : IEnumerator<ValueYieldInstruction>
            => CoroutineScheduler.Shared.StartWithHandle(new CancellableCancellationToken(cancellationToken), routine);

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> of this coroutine.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine StartWithHandleThreadSafe<T>(T routine, CancellationToken cancellationToken) where T : IEnumerator<ValueYieldInstruction>
            => CoroutineScheduler.Shared.StartWithHandleThreadSafe(new CancellableCancellationToken(cancellationToken), routine);
    }
}