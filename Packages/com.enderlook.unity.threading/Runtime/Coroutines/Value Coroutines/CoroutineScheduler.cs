using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    /// <summary>
    /// Represent a manager of value coroutines.<br/>
    /// This object should not be copied not stored in readonly fields.
    /// </summary>
    public partial struct CoroutineScheduler : IDisposable
    {
        private Managers core;

        /// <summary>
        /// Amount of miliseconds spent in executing poll coroutines per frame.
        /// </summary>
        public int MilisecondsExecutedPerFrameOnPoll { get; set; }

        /// <summary>
        /// Percentage of total execution that must be executed on per frame regardless of <see cref="MilisecondsExecutedPerFrameOnPoll"/>.
        /// </summary>
        public float MinimumPercentOfExecutionsPerFrameOnPoll { get; set; }

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
                MilisecondsExecutedPerFrameOnPoll = Yield.MilisecondsExecutedPerFrameOnPoll,
                MinimumPercentOfExecutionsPerFrameOnPoll = Yield.MinimumPercentOfExecutionsPerFrameOnPoll,
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
            if (typeof(T).IsValueType)
            {
                if (typeof(U).IsValueType)
                    manager.Start(cancellator, routine);
                else
                    manager.Start((ICancellable)cancellator, routine);
            }
            else
            {
                if (typeof(U).IsValueType)
                    manager.Start(new Uncancellable(), (IEnumerator<ValueYieldInstruction>)routine);
                else
                    manager.Start((ICancellable)cancellator, (IEnumerator<ValueYieldInstruction>)routine);
            }
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
            if (typeof(T).IsValueType)
            {
                if (typeof(U).IsValueType)
                    return manager.StartWithHandle(cancellator, routine);
                else
                    return manager.StartWithHandle((ICancellable)cancellator, routine);
            }
            else
            {
                if (typeof(U).IsValueType)
                    return manager.StartWithHandle(new Uncancellable(), (IEnumerator<ValueYieldInstruction>)routine);
                else
                    return manager.StartWithHandle((ICancellable)cancellator, (IEnumerator<ValueYieldInstruction>)routine);
            }
        }

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="cancellator">Object that determines when the coroutine should be cancelled.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StartThreadSafe<T, U>(T routine, U cancellator)
            where T : IEnumerator<ValueYieldInstruction>
            where U : ICancellable
        {
            Managers manager = core;
            if (manager is null)
                ThrowDisposed();
            if (typeof(T).IsValueType)
            {
                if (typeof(U).IsValueType)
                    manager.StartThreadSafe(cancellator, routine);
                else
                    manager.StartThreadSafe((ICancellable)cancellator, routine);
            }
            else
            {
                if (typeof(U).IsValueType)
                    manager.StartThreadSafe(new Uncancellable(), (IEnumerator<ValueYieldInstruction>)routine);
                else
                    manager.StartThreadSafe((ICancellable)cancellator, (IEnumerator<ValueYieldInstruction>)routine);
            }
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
        public ValueCoroutine StartWithHandleThreadSafe<T, U>(T routine, U cancellator)
            where T : IEnumerator<ValueYieldInstruction>
            where U : ICancellable
        {
            Managers manager = core;
            if (manager is null)
                ThrowDisposed();
            if (typeof(T).IsValueType)
            {
                if (typeof(U).IsValueType)
                    return manager.StartWithHandleThreadSafe(cancellator, routine);
                else
                    return manager.StartWithHandleThreadSafe((ICancellable)cancellator, routine);
            }
            else
            {
                if (typeof(U).IsValueType)
                    return manager.StartWithHandleThreadSafe(new Uncancellable(), (IEnumerator<ValueYieldInstruction>)routine);
                else
                    return manager.StartWithHandleThreadSafe((ICancellable)cancellator, (IEnumerator<ValueYieldInstruction>)routine);
            }
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
        public void StartThreadSafe<T>(T routine) where T : IEnumerator<ValueYieldInstruction>
            => StartThreadSafe(routine, new Uncancellable());

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
        public ValueCoroutine StartWithHandleThreadSafe<T>(T routine) where T : IEnumerator<ValueYieldInstruction>
            => StartWithHandleThreadSafe(routine, new Uncancellable());

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
        public void StartThreadSafe<T>(T routine, UnityEngine.Object cancellator) where T : IEnumerator<ValueYieldInstruction>
            => StartThreadSafe(routine, new CancellableUnityObject(cancellator));

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
        public ValueCoroutine StartWithHandleThreadSafe<T>(T routine, UnityEngine.Object cancellator) where T : IEnumerator<ValueYieldInstruction>
            => StartWithHandleThreadSafe(routine, new CancellableUnityObject(cancellator));

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
        public void StartThreadSafe<T>(T routine, CancellationToken cancellator) where T : IEnumerator<ValueYieldInstruction>
            => StartThreadSafe(routine, new CancellableCancellationToken(cancellator));

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
        public ValueCoroutine StartWithHandleThreadSafe<T>(T routine, CancellationToken cancellator) where T : IEnumerator<ValueYieldInstruction>
            => StartWithHandleThreadSafe(routine, new CancellableCancellationToken(cancellator));

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowDisposed()
            => throw new ObjectDisposedException(nameof(Managers), $"{nameof(CoroutineScheduler)} was disposed of is a defalt struct.");

        /// <summary>
        /// Executes the update event of all coroutines.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnUpdate()
        {
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
            Managers manager = core;
            if (manager is null)
                ThrowDisposed();
            manager.OnPoll(MilisecondsExecutedPerFrameOnPoll, MinimumPercentOfExecutionsPerFrameOnPoll);
        }

        /// <inheritdoc cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            Managers manager = Interlocked.Exchange(ref core, null);
            if (!(manager is null))
            {
                manager.Free();
                ConcurrentPool.Return(manager);
            }
        }
    }
}