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
            => CoroutineScheduler.Shared.Start(routine, new Uncancellable());

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConcurrentStart<T>(T routine) where T : IEnumerator<ValueYieldInstruction>
            => CoroutineScheduler.Shared.ConcurrentStart(routine, new Uncancellable());

        /// <summary>
        /// Start a value coroutine.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine StartWithHandle<T>(T routine) where T : IEnumerator<ValueYieldInstruction>
            => CoroutineScheduler.Shared.StartWithHandle(routine, new Uncancellable());

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine ConcurrentStartWithHandle<T>(T routine) where T : IEnumerator<ValueYieldInstruction>
            => CoroutineScheduler.Shared.ConcurrentStartWithHandle(routine, new Uncancellable());

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
            => CoroutineScheduler.Shared.Start(routine, cancellator);

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="cancellator">Cancellator of the coroutine.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConcurrentStart<T, U>(T routine, U cancellator)
            where T : IEnumerator<ValueYieldInstruction>
            where U : ICancellable
            => CoroutineScheduler.Shared.ConcurrentStart(routine, cancellator);

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method must be executed from the Unity thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="source"><see cref="Object"/> from which this coroutine is attached to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Start<T>(T routine, Object source) where T : IEnumerator<ValueYieldInstruction>
            => CoroutineScheduler.Shared.Start(routine, new CancellableUnityObject(source));

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="source"><see cref="Object"/> from which this coroutine is attached to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConcurrentStart<T>(T routine, Object source) where T : IEnumerator<ValueYieldInstruction>
            => CoroutineScheduler.Shared.ConcurrentStart(routine, new CancellableUnityObject(source));

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method must be executed from the Unity thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="source"><see cref="Object"/> from which this coroutine is attached to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine StartWithHandle<T>(T routine, Object source) where T : IEnumerator<ValueYieldInstruction>
            => CoroutineScheduler.Shared.StartWithHandle(routine, new CancellableUnityObject(source));

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="source"><see cref="Object"/> from which this coroutine is attached to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine ConcurrentStartWithHandle<T>(T routine, Object source) where T : IEnumerator<ValueYieldInstruction>
            => CoroutineScheduler.Shared.ConcurrentStartWithHandle(routine, new CancellableUnityObject(source));

        /// <summary>
        /// Start a value coroutine.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> of this coroutine.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Start<T>(T routine, CancellationToken cancellationToken) where T : IEnumerator<ValueYieldInstruction>
            => CoroutineScheduler.Shared.Start(routine, new CancellableCancellationToken(cancellationToken));

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> of this coroutine.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConcurrentStart<T>(T routine, CancellationToken cancellationToken) where T : IEnumerator<ValueYieldInstruction>
            => CoroutineScheduler.Shared.ConcurrentStart(routine, new CancellableCancellationToken(cancellationToken));

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method must be executed from the Unity thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> of this coroutine.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine StartWithHandle<T>(T routine, CancellationToken cancellationToken) where T : IEnumerator<ValueYieldInstruction>
            => CoroutineScheduler.Shared.StartWithHandle(routine, new CancellableCancellationToken(cancellationToken));

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> of this coroutine.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine ConcurrentStartWithHandle<T>(T routine, CancellationToken cancellationToken) where T : IEnumerator<ValueYieldInstruction>
            => CoroutineScheduler.Shared.ConcurrentStartWithHandle(routine, new CancellableCancellationToken(cancellationToken));
    }
}