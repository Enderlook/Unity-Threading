using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    internal static class CoroutineSchedulerManagersExtensions
    {
        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method must be executed from the Unity thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Start<T>(this CoroutineScheduler.Managers managers, T routine)
            where T : IEnumerator<ValueYieldInstruction>
            => managers.Start(new ValueCoroutineEnumerator<T>(routine));

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConcurrentStart<T>(this CoroutineScheduler.Managers managers, T routine)
            where T : IEnumerator<ValueYieldInstruction>
            => managers.ConcurrentStart(new ValueCoroutineEnumerator<T>(routine));

        /// <summary>
        /// Start a value coroutine.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine StartWithHandle<T>(this CoroutineScheduler.Managers managers, T routine)
            where T : IEnumerator<ValueYieldInstruction>
            => managers.StartWithHandle(new ValueCoroutineEnumerator<T>(routine));

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine ConcurrentStartWithHandle<T>(this CoroutineScheduler.Managers managers, T routine)
            where T : IEnumerator<ValueYieldInstruction>
            => managers.ConcurrentStartWithHandle(new ValueCoroutineEnumerator<T>(routine));

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method must be executed from the Unity thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="token"><see cref="CancellationToken"/> token of the coroutine.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Start<T>(this CoroutineScheduler.Managers managers, T routine, CancellationToken token)
            where T : IEnumerator<ValueYieldInstruction>
            => managers.Start(new ValueCoroutineEnumeratorWithState<T, CancellationTokenFinalizer>(routine, new CancellationTokenFinalizer(token)));

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="token"><see cref="CancellationToken"/> token of the coroutine.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConcurrentStart<T>(this CoroutineScheduler.Managers managers, T routine, CancellationToken token)
            where T : IEnumerator<ValueYieldInstruction>
            => managers.ConcurrentStart(new ValueCoroutineEnumeratorWithState<T, CancellationTokenFinalizer>(routine, new CancellationTokenFinalizer(token)));

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method must be executed from the Unity thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="token"><see cref="CancellationToken"/> token of the coroutine.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine StartWithHandle<T>(this CoroutineScheduler.Managers managers, T routine, CancellationToken token)
            where T : IEnumerator<ValueYieldInstruction>
            => managers.StartWithHandle(new ValueCoroutineEnumeratorWithState<T, CancellationTokenFinalizer>(routine, new CancellationTokenFinalizer(token)));

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="token"><see cref="CancellationToken"/> token of the coroutine.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine ConcurrentStartWithHandle<T>(this CoroutineScheduler.Managers managers, T routine, CancellationToken token)
            where T : IEnumerator<ValueYieldInstruction>
            => managers.ConcurrentStartWithHandle(new ValueCoroutineEnumeratorWithState<T, CancellationTokenFinalizer>(routine, new CancellationTokenFinalizer(token)));

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method must be executed from the Unity thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="source"><see cref="Object"/> from which this coroutine is attached to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Start<T>(this CoroutineScheduler.Managers managers, T routine, Object source)
            where T : IEnumerator<ValueYieldInstruction>
            => managers.Start(new ValueCoroutineEnumeratorWithState<T, UnityObjectFinalizeWhenNull>(routine, new UnityObjectFinalizeWhenNull(source)));

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="source"><see cref="Object"/> from which this coroutine is attached to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConcurrentStart<T>(this CoroutineScheduler.Managers managers, T routine, Object source)
            where T : IEnumerator<ValueYieldInstruction>
            => managers.ConcurrentStart(new ValueCoroutineEnumeratorWithState<T, UnityObjectFinalizeWhenNull>(routine, new UnityObjectFinalizeWhenNull(source)));

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method must be executed from the Unity thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="source"><see cref="Object"/> from which this coroutine is attached to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine StartWithHandle<T>(this CoroutineScheduler.Managers managers, T routine, Object source)
            where T : IEnumerator<ValueYieldInstruction>
            => managers.StartWithHandle(new ValueCoroutineEnumeratorWithState<T, UnityObjectFinalizeWhenNull>(routine, new UnityObjectFinalizeWhenNull(source)));

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="source"><see cref="Object"/> from which this coroutine is attached to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine ConcurrentStartWithHandle<T>(this CoroutineScheduler.Managers managers, T routine, Object source)
            where T : IEnumerator<ValueYieldInstruction>
            => managers.ConcurrentStartWithHandle(new ValueCoroutineEnumeratorWithState<T, UnityObjectFinalizeWhenNull>(routine, new UnityObjectFinalizeWhenNull(source)));

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method must be executed from the Unity thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="source"><see cref="GameObject"/> from which this coroutine is attached to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Start<T>(this CoroutineScheduler.Managers managers, T routine, GameObject source)
            where T : IEnumerator<ValueYieldInstruction>
            => managers.Start(new ValueCoroutineEnumeratorWithState<T, GameObjectFinalizeWhenNullAndSuspendWhenIsNotActive>(routine, new GameObjectFinalizeWhenNullAndSuspendWhenIsNotActive(source)));

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="source"><see cref="GameObject"/> from which this coroutine is attached to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConcurrentStart<T>(this CoroutineScheduler.Managers managers, T routine, GameObject source)
            where T : IEnumerator<ValueYieldInstruction>
            => managers.ConcurrentStart(new ValueCoroutineEnumeratorWithState<T, GameObjectFinalizeWhenNullAndSuspendWhenIsNotActive>(routine, new GameObjectFinalizeWhenNullAndSuspendWhenIsNotActive(source)));

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method must be executed from the Unity thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="source"><see cref="Object"/> from which this coroutine is attached to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine StartWithHandle<T>(this CoroutineScheduler.Managers managers, T routine, GameObject source)
            where T : IEnumerator<ValueYieldInstruction>
            => managers.StartWithHandle(new ValueCoroutineEnumeratorWithState<T, GameObjectFinalizeWhenNullAndSuspendWhenIsNotActive>(routine, new GameObjectFinalizeWhenNullAndSuspendWhenIsNotActive(source)));

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="source"><see cref="Object"/> from which this coroutine is attached to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine ConcurrentStartWithHandle<T>(this CoroutineScheduler.Managers managers, T routine, GameObject source)
            where T : IEnumerator<ValueYieldInstruction>
            => managers.ConcurrentStartWithHandle(new ValueCoroutineEnumeratorWithState<T, GameObjectFinalizeWhenNullAndSuspendWhenIsNotActive>(routine, new GameObjectFinalizeWhenNullAndSuspendWhenIsNotActive(source)));

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method must be executed from the Unity thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="source"><see cref="MonoBehaviour"/> from which this coroutine is attached to.</param>
        /// <param name="suspendWhenSourceIsDisabled">If <see langword="true"/>, coroutine will be suspended while <paramref name="source"/> is disabled.<br/>
        /// By default this is false to simulate the behaviour of Unity Coroutines.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Start<T>(this CoroutineScheduler.Managers managers, T routine, MonoBehaviour source, bool suspendWhenSourceIsDisabled = false)
            where T : IEnumerator<ValueYieldInstruction>
        {
            if (suspendWhenSourceIsDisabled)
                managers.Start(new ValueCoroutineEnumeratorWithState<T, MonoBehaviourFinalizeWhenNullAndSuspendWhenIsNotActiveNorEnabled>(routine, new MonoBehaviourFinalizeWhenNullAndSuspendWhenIsNotActiveNorEnabled(source)));
            else
                managers.Start(new ValueCoroutineEnumeratorWithState<T, MonoBehaviourFinalizeWhenNullAndSuspendWhenGameObjectIsDisabled>(routine, new MonoBehaviourFinalizeWhenNullAndSuspendWhenGameObjectIsDisabled(source)));
        }

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="source"><see cref="MonoBehaviour"/> from which this coroutine is attached to.</param>
        /// <param name="suspendWhenSourceIsDisabled">If <see langword="true"/>, coroutine will be suspended while <paramref name="source"/> is disabled.<br/>
        /// By default this is false to simulate the behaviour of Unity Coroutines.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ConcurrentStart<T>(this CoroutineScheduler.Managers managers, T routine, MonoBehaviour source, bool suspendWhenSourceIsDisabled = false)
            where T : IEnumerator<ValueYieldInstruction>
        {
            if (suspendWhenSourceIsDisabled)
                managers.ConcurrentStart(new ValueCoroutineEnumeratorWithState<T, MonoBehaviourFinalizeWhenNullAndSuspendWhenIsNotActiveNorEnabled>(routine, new MonoBehaviourFinalizeWhenNullAndSuspendWhenIsNotActiveNorEnabled(source)));
            else
                managers.ConcurrentStart(new ValueCoroutineEnumeratorWithState<T, MonoBehaviourFinalizeWhenNullAndSuspendWhenGameObjectIsDisabled>(routine, new MonoBehaviourFinalizeWhenNullAndSuspendWhenGameObjectIsDisabled(source)));
        }

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method must be executed from the Unity thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="source"><see cref="Object"/> from which this coroutine is attached to.</param>
        /// <param name="suspendWhenSourceIsDisabled">If <see langword="true"/>, coroutine will be suspended while <paramref name="source"/> is disabled.<br/>
        /// By default this is false to simulate the behaviour of Unity Coroutines.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine StartWithHandle<T>(this CoroutineScheduler.Managers managers, T routine, MonoBehaviour source, bool suspendWhenSourceIsDisabled = false)
            where T : IEnumerator<ValueYieldInstruction>
        {
            if (suspendWhenSourceIsDisabled)
                return managers.StartWithHandle(new ValueCoroutineEnumeratorWithState<T, MonoBehaviourFinalizeWhenNullAndSuspendWhenIsNotActiveNorEnabled>(routine, new MonoBehaviourFinalizeWhenNullAndSuspendWhenIsNotActiveNorEnabled(source)));
            else
                return managers.StartWithHandle(new ValueCoroutineEnumeratorWithState<T, MonoBehaviourFinalizeWhenNullAndSuspendWhenGameObjectIsDisabled>(routine, new MonoBehaviourFinalizeWhenNullAndSuspendWhenGameObjectIsDisabled(source)));
        }

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="source"><see cref="Object"/> from which this coroutine is attached to.</param>
        /// <param name="suspendWhenSourceIsDisabled">If <see langword="true"/>, coroutine will be suspended while <paramref name="source"/> is disabled.<br/>
        /// By default this is false to simulate the behaviour of Unity Coroutines.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine ConcurrentStartWithHandle<T>(this CoroutineScheduler.Managers managers, T routine, MonoBehaviour source, bool suspendWhenSourceIsDisabled = false)
            where T : IEnumerator<ValueYieldInstruction>
        {
            if (suspendWhenSourceIsDisabled)
                return managers.ConcurrentStartWithHandle(new ValueCoroutineEnumeratorWithState<T, MonoBehaviourFinalizeWhenNullAndSuspendWhenIsNotActiveNorEnabled>(routine, new MonoBehaviourFinalizeWhenNullAndSuspendWhenIsNotActiveNorEnabled(source)));
            else
                return managers.ConcurrentStartWithHandle(new ValueCoroutineEnumeratorWithState<T, MonoBehaviourFinalizeWhenNullAndSuspendWhenGameObjectIsDisabled>(routine, new MonoBehaviourFinalizeWhenNullAndSuspendWhenGameObjectIsDisabled(source)));
        }
    }
}