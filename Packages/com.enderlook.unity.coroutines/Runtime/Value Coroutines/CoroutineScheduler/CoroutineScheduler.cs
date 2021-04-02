using Enderlook.Unity.Threading;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    /// <summary>
    /// Represent a manager of value coroutines.<br/>
    /// This object should not be copied nor stored in readonly fields, and should be passed by reference.
    /// </summary>
    [Serializable]
    public partial struct CoroutineScheduler : IDisposable
    {
        internal static readonly Managers Shared = new Managers();

        static CoroutineScheduler()
        {
            Manager.OnUpdate += Shared.OnUpdate;
            Manager.OnUpdate += Shared.OnPoll;
            Manager.OnFixedUpdate += Shared.OnFixedUpdate;
            Manager.OnEndOfFrame += Shared.OnEndOfFrame;
            Manager.OnLateUpdate += Shared.OnEndOfFrame;
        }

        [SerializeField, HideInInspector]
        private Managers core;

        /// <summary>
        /// Amount of miliseconds spent in executing poll coroutines per call to <see cref="OnPoll"/>.
        /// </summary>
        public int MilisecondsExecutedPerFrameOnPoll {
            get => core.MilisecondsExecutedPerFrameOnPoll;
            set => core.MilisecondsExecutedPerFrameOnPoll = value;
        }

        /// <summary>
        /// Percentage of total execution that must be executed on per call to <see cref="OnPoll"/> regardless of <see cref="MilisecondsExecutedPerFrameOnPoll"/>.
        /// </summary>
        public float MinimumPercentOfExecutionsPerFrameOnPoll {
            get => core.MinimumPercentOfExecutionsPerFrameOnPoll;
            set => core.MinimumPercentOfExecutionsPerFrameOnPoll = value;
        }

        /// <summary>
        /// Creates a manager whose events must be called manually.
        /// </summary>
        /// <param name="monoBehaviour"><see cref="MonoBehaviour"/> used to fallback Unity coroutines.</param>
        /// <returns>A new manager.</returns>
        public static CoroutineScheduler Create(MonoBehaviour monoBehaviour)
        {
            Managers core = ConcurrentPool.Rent<Managers>();
            core.SetMonoBehaviour(monoBehaviour);
            return new CoroutineScheduler
            {
                core = core,
                MilisecondsExecutedPerFrameOnPoll = Coroutine.MilisecondsExecutedPerFrameOnPoll,
                MinimumPercentOfExecutionsPerFrameOnPoll = Coroutine.MinimumPercentOfExecutionsPerFrameOnPoll,
            };
        }

        /// <summary>
        /// Creates a manager whose events must be called manually.
        /// </summary>
        /// <param name="monoBehaviour"><see cref="MonoBehaviour"/> used to fallback Unity coroutines.</param>
        /// <param name="milisecondsExecutedPerFrameOnPoll">Amount of miliseconds spent in executing global poll coroutines per frame.</param>
        /// <param name="minimumPercentOfExecutionsPerFrameOnPoll">Percentage of total execution that must be executed on per frame regardless of <paramref name="milisecondsExecutedPerFrameOnPoll"/> for global poll coroutines.</param>
        /// <returns>A new manager.</returns>
        public static CoroutineScheduler Create(MonoBehaviour monoBehaviour, int milisecondsExecutedPerFrameOnPoll, int minimumPercentOfExecutionsPerFrameOnPoll)
        {
            Managers core = ConcurrentPool.Rent<Managers>();
            core.SetMonoBehaviour(monoBehaviour);
            return new CoroutineScheduler
            {
                core = core,
                MilisecondsExecutedPerFrameOnPoll = milisecondsExecutedPerFrameOnPoll,
                MinimumPercentOfExecutionsPerFrameOnPoll = minimumPercentOfExecutionsPerFrameOnPoll,
            };
        }

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method must be executed from the Unity thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="cancellator">Object that determines when the coroutine should be cancelled.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start<T, U>(T routine, U cancellator)
            where T : IEnumerator<ValueYieldInstruction>
            where U : ICancellable
        {
            Managers manager = core;
            if (manager is null)
                ThrowDisposed();
            manager.Start(routine, cancellator);
        }

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method must be executed from the Unity thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="cancellator">Object that determines when the coroutine should be cancelled.</param>
        /// <returns>Handle of the coroutine.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueCoroutine StartWithHandle<T, U>(T routine, U cancellator)
            where T : IEnumerator<ValueYieldInstruction>
            where U : ICancellable
        {
            Managers manager = core;
            if (manager is null)
                ThrowDisposed();
            return manager.StartWithHandle(routine, cancellator);
        }

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="cancellator">Object that determines when the coroutine should be cancelled.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ConcurrentStart<T, U>(T routine, U cancellator)
            where T : IEnumerator<ValueYieldInstruction>
            where U : ICancellable
        {
            Managers manager = core;
            if (manager is null)
                ThrowDisposed();
            manager.ConcurrentStart(routine, cancellator);
        }

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="cancellator">Object that determines when the coroutine should be cancelled.</param>
        /// <returns>Handle of the coroutine.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueCoroutine ConcurrentStartWithHandle<T, U>(T routine, U cancellator)
            where T : IEnumerator<ValueYieldInstruction>
            where U : ICancellable
        {
            Managers manager = core;
            if (manager is null)
                ThrowDisposed();
            return manager.ConcurrentStartWithHandle(routine, cancellator);
        }

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method must be executed from the Unity thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start<T>(T routine) where T : IEnumerator<ValueYieldInstruction>
            => Start(routine, new Uncancellable());

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ConcurrentStart<T>(T routine) where T : IEnumerator<ValueYieldInstruction>
            => ConcurrentStart(routine, new Uncancellable());

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method must be executed from the Unity thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <returns>Handle of the coroutine.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueCoroutine StartWithHandle<T>(T routine) where T : IEnumerator<ValueYieldInstruction>
            => StartWithHandle(routine, new Uncancellable());

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <returns>Handle of the coroutine.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueCoroutine ConcurrentStartWithHandle<T>(T routine) where T : IEnumerator<ValueYieldInstruction>
            => ConcurrentStartWithHandle(routine, new Uncancellable());

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method must be executed from the Unity thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="cancellator">Object than when is destroyed ends the coroutine.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start<T>(T routine, UnityEngine.Object cancellator) where T : IEnumerator<ValueYieldInstruction>
            => Start(routine, new CancellableUnityObject(cancellator));

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="cancellator">Object than when is destroyed ends the coroutine.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ConcurrentStart<T>(T routine, UnityEngine.Object cancellator) where T : IEnumerator<ValueYieldInstruction>
            => ConcurrentStart(routine, new CancellableUnityObject(cancellator));

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method must be executed from the Unity thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="cancellator">Object than when is destroyed ends the coroutine.</param>
        /// <returns>Handle of the coroutine.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueCoroutine StartWithHandle<T>(T routine, UnityEngine.Object cancellator) where T : IEnumerator<ValueYieldInstruction>
            => StartWithHandle(routine, new CancellableUnityObject(cancellator));

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="cancellator">Object than when is destroyed ends the coroutine.</param>
        /// <returns>Handle of the coroutine.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueCoroutine ConcurrentStartWithHandle<T>(T routine, UnityEngine.Object cancellator) where T : IEnumerator<ValueYieldInstruction>
            => ConcurrentStartWithHandle(routine, new CancellableUnityObject(cancellator));

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method must be executed from the Unity thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="cancellator">Cancelation token of the coroutine.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start<T>(T routine, CancellationToken cancellator) where T : IEnumerator<ValueYieldInstruction>
            => Start(routine, new CancellableCancellationToken(cancellator));

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="cancellator">Cancelation token of the coroutine.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ConcurrentStart<T>(T routine, CancellationToken cancellator) where T : IEnumerator<ValueYieldInstruction>
            => ConcurrentStart(routine, new CancellableCancellationToken(cancellator));

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method must be executed from the Unity thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="cancellator">Cancelation token of the coroutine.</param>
        /// <returns>Handle of the coroutine.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueCoroutine StartWithHandle<T>(T routine, CancellationToken cancellator) where T : IEnumerator<ValueYieldInstruction>
            => StartWithHandle(routine, new CancellableCancellationToken(cancellator));

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="cancellator">Cancelation token of the coroutine.</param>
        /// <returns>Handle of the coroutine.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueCoroutine ConcurrentStartWithHandle<T>(T routine, CancellationToken cancellator) where T : IEnumerator<ValueYieldInstruction>
            => ConcurrentStartWithHandle(routine, new CancellableCancellationToken(cancellator));

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowDisposed()
            => throw new ObjectDisposedException(nameof(Managers), $"{nameof(CoroutineScheduler)} was disposed of is a defalt struct.");

        /// <summary>
        /// Executes the update event of all coroutines.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnUpdate()
        {
            CheckThread();
            Managers manager = core;
            if (manager is null)
                ThrowDisposed();
            manager.OnUpdate();
        }

        /// <summary>
        /// Executes the fixed update event of all coroutines.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnFixedUpdate()
        {
            CheckThread();
            Managers manager = core;
            if (manager is null)
                ThrowDisposed();
            manager.OnFixedUpdate();
        }

        /// <summary>
        /// Executes the late update event of all coroutines.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnLateUpdate()
        {
            CheckThread();
            Managers manager = core;
            if (manager is null)
                ThrowDisposed();
            manager.OnLateUpdate();
        }

        /// <summary>
        /// Executes the end of frame event of all coroutines.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnEndOfFrame()
        {
            CheckThread();
            Managers manager = core;
            if (manager is null)
                ThrowDisposed();
            manager.OnEndOfFrame();
        }

        /// <summary>
        /// Executes the poll event of all coroutines.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnPoll()
        {
            CheckThread();
            Managers manager = core;
            if (manager is null)
                ThrowDisposed();
            manager.OnPoll();
        }

        /// <inheritdoc cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            CheckThread();
            Managers manager = Interlocked.Exchange(ref core, null);
            if (!(manager is null))
            {
                manager.Free();
                ConcurrentPool.Return(manager);
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckThread()
        {
#if DEBUG
            if (!UnityThread.IsMainThread)
                Debug.LogError("This function can only be executed in the Unity thread. This has produced undefined behaviour. This error will not shown on release.");
#endif
        }
    }
}