using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    public sealed partial class CoroutineManager
    {
        /// <summary>
        /// Start a value coroutine.
        /// </summary>
        /// <typeparam name="T">Type of coroutine to start.</typeparam>
        /// <param name="coroutine">Coroutine to start.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start<T>(T coroutine)
            where T : IEnumerator<ValueYieldInstruction>
            => StartEnumerator(new ValueCoroutineEnumerator<T>(coroutine));

        /// <summary>
        /// Start a value coroutine.
        /// </summary>
        /// <typeparam name="T">Type of coroutine to start.</typeparam>
        /// <param name="coroutine">Coroutine to start.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueCoroutine StartWithHandle<T>(T coroutine)
            where T : IEnumerator<ValueYieldInstruction>
            => StartEnumeratorWithHandle(new ValueCoroutineEnumerator<T>(coroutine));
        
        /// <summary>
        /// Start a value coroutine.
        /// </summary>
        /// <typeparam name="T">Type of coroutine to start.</typeparam>
        /// <param name="coroutine">Coroutine to start.</param>
        /// <param name="token"><see cref="CancellationToken"/> token of the coroutine.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start<T>(T coroutine, CancellationToken token)
            where T : IEnumerator<ValueYieldInstruction>
            => StartEnumerator(new ValueCoroutineEnumeratorWithState<T, CancellationTokenFinalizer>(coroutine, new CancellationTokenFinalizer(token)));

        /// <summary>
        /// Start a value coroutine.
        /// </summary>
        /// <typeparam name="T">Type of coroutine to start.</typeparam>
        /// <param name="coroutine">Coroutine to start.</param>
        /// <param name="token"><see cref="CancellationToken"/> token of the coroutine.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueCoroutine StartWithHandle<T>(T coroutine, CancellationToken token)
            where T : IEnumerator<ValueYieldInstruction>
            => StartEnumeratorWithHandle(new ValueCoroutineEnumeratorWithState<T, CancellationTokenFinalizer>(coroutine, new CancellationTokenFinalizer(token)));

        /// <summary>
        /// Start a value coroutine.
        /// </summary>
        /// <typeparam name="T">Type of coroutine to start.</typeparam>
        /// <param name="coroutine">Coroutine to start.</param>
        /// <param name="source"><see cref="Object"/> from which this coroutine is attached to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start<T>(T coroutine, Object source)
            where T : IEnumerator<ValueYieldInstruction>
            => StartEnumerator(new ValueCoroutineEnumeratorWithState<T, UnityObjectFinalizeWhenNull>(coroutine, new UnityObjectFinalizeWhenNull(source)));

        /// <summary>
        /// Start a value coroutine.
        /// </summary>
        /// <typeparam name="T">Type of coroutine to start.</typeparam>
        /// <param name="coroutine">Coroutine to start.</param>
        /// <param name="source"><see cref="Object"/> from which this coroutine is attached to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueCoroutine StartWithHandle<T>(T coroutine, Object source)
            where T : IEnumerator<ValueYieldInstruction>
            => StartEnumeratorWithHandle(new ValueCoroutineEnumeratorWithState<T, UnityObjectFinalizeWhenNull>(coroutine, new UnityObjectFinalizeWhenNull(source)));

        /// <summary>
        /// Start a value coroutine.
        /// </summary>
        /// <typeparam name="T">Type of coroutine to start.</typeparam>
        /// <param name="coroutine">Coroutine to start.</param>
        /// <param name="source"><see cref="GameObject"/> from which this coroutine is attached to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start<T>(T coroutine, GameObject source)
            where T : IEnumerator<ValueYieldInstruction>
            => StartEnumerator(new ValueCoroutineEnumeratorWithState<T, GameObjectFinalizeWhenNullAndSuspendWhenIsNotActive>(coroutine, new GameObjectFinalizeWhenNullAndSuspendWhenIsNotActive(source)));

        /// <summary>
        /// Start a value coroutine.
        /// </summary>
        /// <typeparam name="T">Type of coroutine to start.</typeparam>
        /// <param name="coroutine">Coroutine to start.</param>
        /// <param name="source"><see cref="Object"/> from which this coroutine is attached to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueCoroutine StartWithHandle<T>(T coroutine, GameObject source)
            where T : IEnumerator<ValueYieldInstruction>
            => StartEnumeratorWithHandle(new ValueCoroutineEnumeratorWithState<T, GameObjectFinalizeWhenNullAndSuspendWhenIsNotActive>(coroutine, new GameObjectFinalizeWhenNullAndSuspendWhenIsNotActive(source)));

        /// <summary>
        /// Start a value coroutine.
        /// </summary>
        /// <typeparam name="T">Type of coroutine to start.</typeparam>
        /// <param name="coroutine">Coroutine to start.</param>
        /// <param name="source"><see cref="MonoBehaviour"/> from which this coroutine is attached to.</param>
        /// <param name="suspendWhenSourceIsDisabled">If <see langword="true"/>, coroutine will be suspended while <paramref name="source"/> is disabled.<br/>
        /// By default this is false to simulate the behaviour of Unity Coroutines.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start<T>(T coroutine, MonoBehaviour source, bool suspendWhenSourceIsDisabled = false)
            where T : IEnumerator<ValueYieldInstruction>
        {
            if (suspendWhenSourceIsDisabled)
                StartEnumerator(new ValueCoroutineEnumeratorWithState<T, MonoBehaviourFinalizeWhenNullAndSuspendWhenIsNotActiveNorEnabled>(coroutine, new MonoBehaviourFinalizeWhenNullAndSuspendWhenIsNotActiveNorEnabled(source)));
            else
                StartEnumerator(new ValueCoroutineEnumeratorWithState<T, MonoBehaviourFinalizeWhenNullAndSuspendWhenGameObjectIsDisabled>(coroutine, new MonoBehaviourFinalizeWhenNullAndSuspendWhenGameObjectIsDisabled(source)));
        }

        /// <summary>
        /// Start a value coroutine.
        /// </summary>
        /// <typeparam name="T">Type of coroutine to start.</typeparam>
        /// <param name="coroutine">Coroutine to start.</param>
        /// <param name="source"><see cref="Object"/> from which this coroutine is attached to.</param>
        /// <param name="suspendWhenSourceIsDisabled">If <see langword="true"/>, coroutine will be suspended while <paramref name="source"/> is disabled.<br/>
        /// By default this is false to simulate the behaviour of Unity Coroutines.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueCoroutine StartWithHandle<T>(T coroutine, MonoBehaviour source, bool suspendWhenSourceIsDisabled = false)
            where T : IEnumerator<ValueYieldInstruction>
        {
            if (suspendWhenSourceIsDisabled)
                return StartEnumeratorWithHandle(new ValueCoroutineEnumeratorWithState<T, MonoBehaviourFinalizeWhenNullAndSuspendWhenIsNotActiveNorEnabled>(coroutine, new MonoBehaviourFinalizeWhenNullAndSuspendWhenIsNotActiveNorEnabled(source)));
            else
                return StartEnumeratorWithHandle(new ValueCoroutineEnumeratorWithState<T, MonoBehaviourFinalizeWhenNullAndSuspendWhenGameObjectIsDisabled>(coroutine, new MonoBehaviourFinalizeWhenNullAndSuspendWhenGameObjectIsDisabled(source)));
        }
    }
}