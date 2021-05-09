using Enderlook.Unity.Threading;

using System;
using System.Runtime.CompilerServices;
using System.Threading;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    /// <summary>
    /// Represent a manager of value coroutines.
    /// </summary>
    /// <remarks>This object should not be copied nor stored in readonly fields, and should be passed by reference.</remarks>
    [Serializable]
    public partial struct CoroutineScheduler : IDisposable
    {
        private static readonly Managers shared = new Managers();
        internal static CoroutineScheduler Shared => new CoroutineScheduler(shared);

        static CoroutineScheduler()
        {
            Manager.OnUpdate += Shared.OnUpdate;
            Manager.OnUpdate += Shared.OnPoll;
            Manager.OnFixedUpdate += Shared.OnFixedUpdate;
            Manager.OnEndOfFrame += Shared.OnEndOfFrame;
            Manager.OnLateUpdate += Shared.OnEndOfFrame;
            shared.Initialize(Manager.Shared);
        }

        [SerializeField, HideInInspector]
        private Managers core;

        private Managers Core {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                Managers manager = core;
                if (manager is null)
                    ThrowDisposed();
                return manager;
            }
        }

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private CoroutineScheduler(Managers core) => this.core = core;

        /// <summary>
        /// Creates a manager whose events must be called manually.
        /// </summary>
        /// <param name="monoBehaviour"><see cref="MonoBehaviour"/> used to fallback Unity coroutines.</param>
        /// <returns>A new manager.</returns>
        public static CoroutineScheduler Create(MonoBehaviour monoBehaviour)
        {
            Managers core = new Managers();
            core.Initialize(monoBehaviour);
            return new CoroutineScheduler(core)
            {
                MilisecondsExecutedPerFrameOnPoll = ValueCoroutine.MilisecondsExecutedPerFrameOnPoll,
                MinimumPercentOfExecutionsPerFrameOnPoll = ValueCoroutine.MinimumPercentOfExecutionsPerFrameOnPoll,
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
            Managers core = new Managers();
            core.Initialize(monoBehaviour);
            return new CoroutineScheduler(core)
            {
                MilisecondsExecutedPerFrameOnPoll = milisecondsExecutedPerFrameOnPoll,
                MinimumPercentOfExecutionsPerFrameOnPoll = minimumPercentOfExecutionsPerFrameOnPoll,
            };
        }

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