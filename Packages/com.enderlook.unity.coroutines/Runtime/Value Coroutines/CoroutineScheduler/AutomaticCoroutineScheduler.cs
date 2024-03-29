﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    /// <summary>
    /// A wrapper arround <see cref="CoroutineManager"/> to automatically execute its events.
    /// </summary>
    [AddComponentMenu("Enderlook/Automatic Coroutines Manager")]
    [DefaultExecutionOrder(int.MaxValue)]
    public sealed class AutomaticCoroutineScheduler : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        private CoroutineManager manager = new CoroutineManager();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            manager.SetMonoBehaviour(this);
            StartCoroutine(Work());
            IEnumerator Work()
            {
                while (true)
                {
                    yield return Wait.ForEndOfFrame;
                    manager.OnEndOfFrame();
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Update() => manager.OnUpdate();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void LateUpdate()
        {
            manager.OnLateUpdate();
            manager.OnPoll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void FixedUpdate() => manager.OnFixedUpdate();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void OnDestroy()
        {
            CoroutineManager m = manager;
            manager = null;
            m.Dispose();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void OnEnable()
        {
            if (manager?.IsSuspended ?? false)
                manager?.Reanude();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void OnDisable() => manager?.Suspend();

        /// <inheritdoc cref="CoroutineManager.Start{T}(T)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StartValueCoroutine<T>(T coroutine)
            where T : IEnumerator<ValueYieldInstruction>
            => manager.Start(coroutine);

        /// <inheritdoc cref="CoroutineManager.Start{T}(T)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueCoroutine StartValueCoroutineWithHandle<T>(T coroutine)
            where T : IEnumerator<ValueYieldInstruction>
            => manager.StartWithHandle(coroutine);

        /// <inheritdoc cref="CoroutineManager.Start{T}(T, CancellationToken)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StartValueCoroutine<T>(T coroutine, CancellationToken token)
            where T : IEnumerator<ValueYieldInstruction>
            => manager.Start(coroutine, token);

        /// <inheritdoc cref="CoroutineManager.Start{T}(T, CancellationToken)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueCoroutine StartValueCoroutineWithHandle<T>(T coroutine, CancellationToken token)
            where T : IEnumerator<ValueYieldInstruction>
            => manager.StartWithHandle(coroutine, token);

        /// <inheritdoc cref="CoroutineManager.Start{T}(T, Object)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StartValueCoroutine<T>(T coroutine, Object source)
            where T : IEnumerator<ValueYieldInstruction>
            => manager.Start(coroutine, source);

        /// <inheritdoc cref="CoroutineManager.Start{T}(T, Object)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueCoroutine StartValueCoroutineWithHandle<T>(T coroutine, Object source)
            where T : IEnumerator<ValueYieldInstruction>
            => manager.StartWithHandle(coroutine, source);

        /// <inheritdoc cref="CoroutineManager.Start{T}(T, GameObject)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StartValueCoroutine<T>(T coroutine, GameObject source)
            where T : IEnumerator<ValueYieldInstruction>
            => manager.Start(coroutine, source);

        /// <inheritdoc cref="CoroutineManager.Start{T}(T, GameObject)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueCoroutine StartValueCoroutineWithHandle<T>(T coroutine, GameObject source)
            where T : IEnumerator<ValueYieldInstruction>
            => manager.StartWithHandle(coroutine, source);

        /// <inheritdoc cref="CoroutineManager.Start{T}(T, MonoBehaviour, bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StartValueCoroutine<T>(T coroutine, MonoBehaviour source, bool suspendWhenSourceIsDisabled = false)
            where T : IEnumerator<ValueYieldInstruction>
            => manager.Start(coroutine, source, suspendWhenSourceIsDisabled);

        /// <inheritdoc cref="CoroutineManager.Start{T}(T, MonoBehaviour, bool)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueCoroutine StartValueCoroutineWithHandle<T>(T coroutine, MonoBehaviour source, bool suspendWhenSourceIsDisabled = false)
            where T : IEnumerator<ValueYieldInstruction>
            => manager.StartWithHandle(coroutine, source, suspendWhenSourceIsDisabled);
    }
}