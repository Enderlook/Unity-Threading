using Enderlook.Collections.LowLevel;
using Enderlook.Unity.Threading;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    [Serializable]
    public sealed partial class CoroutineManager
    {
        internal const int MILISECONDS_EXECUTED_PER_FRAME_ON_POLL_DEFAULT_VALUE = 5;
        internal const float MINIMUM_PERCENT_OF_EXECUTIONS_PER_FRAME_ON_POLL_DEFAULT_VALUE = .025f;

        internal static CoroutineManager Shared;

        private static readonly Action<Task> logExceptionIfFaulted = e =>
        {
            if (e.IsFaulted)
                Debug.LogException(e.Exception);
        };

        private ValueCoroutineState state;
        private MonoBehaviour monoBehaviour;
        private readonly Dictionary<Type, ManagerBase> managersDictionary = new Dictionary<Type, ManagerBase>();

        // Note: Don't use a RawPooledList here because resizing would clear the previous array,
        // which may be still being used by methods that cached it.
        private RawList<ManagerBase> managersList = RawList<ManagerBase>.Create();
        private ReadWriterLock managerLock;

        /// <summary>
        /// Amount of miliseconds spent in executing poll coroutines per call to <see cref="OnPoll"/>.
        /// </summary>
        public int MilisecondsExecutedPerFrameOnPoll {
            get => milisecondsExecutedPerFrameOnPoll;
            set {
                if (value < 0)
                    ThrowOutOfRangeMilisecondsExecuted();
                milisecondsExecutedPerFrameOnPoll = value;
            }
        }
        [SerializeField, Min(0), Tooltip("Amount of miliseconds spent in executing poll coroutines.")]
        private int milisecondsExecutedPerFrameOnPoll = MILISECONDS_EXECUTED_PER_FRAME_ON_POLL_DEFAULT_VALUE;

        /// <summary>
        /// Percentage of total execution that must be executed on per call to <see cref="OnPoll"/> regardless of <see cref="MilisecondsExecutedPerFrameOnPoll"/>.
        /// </summary>
        public float MinimumPercentOfExecutionsPerFrameOnPoll {
            get => minimumPercentOfExecutionsPerFrameOnPoll;
            set {
                if (value < 0)
                    ThrowOutOfRangePercentExecutions();
                minimumPercentOfExecutionsPerFrameOnPoll = value;
            }
        }
        [SerializeField, Range(0, 1), Tooltip("Percentage of total executions that must be executed on poll coroutines regardless of timeout.")]
        private float minimumPercentOfExecutionsPerFrameOnPoll = MINIMUM_PERCENT_OF_EXECUTIONS_PER_FRAME_ON_POLL_DEFAULT_VALUE;

        /// <summary>
        /// Determines if this manager is suspended.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown when manager is disposed.</exception>
        public bool IsSuspended
        {
            get
            {
                ValueCoroutineState state = this.state;
                if (state == ValueCoroutineState.Finalized)
                    ThrowObjectDisposedException();
                return state == ValueCoroutineState.Suspended;
            }
        }

        /// <summary>
        /// Creates a manager whose events must be called manually.
        /// </summary>
        /// <param name="monoBehaviour"><see cref="MonoBehaviour"/> used to fallback Unity coroutines.</param>
        public CoroutineManager(MonoBehaviour monoBehaviour)
        {
            if (monoBehaviour == null)
                ThrowMonoBehaviourNullException();

            state = ValueCoroutineState.Continue;
            this.monoBehaviour = monoBehaviour;
        }

        internal void SetMonoBehaviour(MonoBehaviour monoBehaviour)
        {
            Debug.Assert(monoBehaviour != null && this.monoBehaviour is null);
            this.monoBehaviour = monoBehaviour;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (!(Shared is null))
                return;

            Manager.OnInitialized(() =>
            {
                CoroutineManager shared = Shared = new CoroutineManager(Manager.Shared);
                Manager.OnUpdate += shared.OnUpdate;
                Manager.OnUpdate += shared.OnPoll;
                Manager.OnFixedUpdate += shared.OnFixedUpdate;
                Manager.OnEndOfFrame += shared.OnEndOfFrame;
                Manager.OnLateUpdate += shared.OnEndOfFrame;
                GlobalCoroutinesManagerUnit.Load();
            });
        }

        ~CoroutineManager() => Dispose();

        /// <summary>
        /// Suspend the execution of the coroutines of this manager.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown when manager is disposed.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="IsSuspended"/> is <see langword="true"/> or is run outside the Unity thread.</exception>
        public void Suspend()
        {
            if (state == ValueCoroutineState.Finalized)
                ThrowObjectDisposedException();
            if (state == ValueCoroutineState.Suspended)
                ThrowIsAlreadySuspended();
            if (!UnityThread.IsMainThread)
                ThrowNonUnityThread();
            state = ValueCoroutineState.Suspended;
        }

        /// <summary>
        /// Suspend the execution of the coroutines of this manager.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown when manager is disposed.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="IsSuspended"/> is <see langword="false"/> or is run outside the Unity thread.</exception>
        public void Reanude()
        {
            if (state == ValueCoroutineState.Finalized)
                ThrowObjectDisposedException();
            if (state == ValueCoroutineState.Continue)
                ThrowIsNotSuspended();
            if (!UnityThread.IsMainThread)
                ThrowNonUnityThread();
            state = ValueCoroutineState.Continue;

            RawList<ManagerBase> managers = GetManagersList();
            for (int i = 0; i < managers.Count; i++)
                managers[i].BackgroundResume();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ValueCoroutine StartEnumeratorWithHandle<T>(T routine) where T : IValueCoroutineEnumerator
            => ValueCoroutine.StartEnumerator(this, routine);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void StartEnumerator<T>(T coroutine) where T : IValueCoroutineEnumerator
        {
            if (typeof(T).IsValueType)
                StartEnumeratorInner(coroutine);
            else
                StartEnumeratorInner((IValueCoroutineEnumerator)coroutine);
        }

        private void StartEnumeratorInner<T>(T coroutine) where T : IValueCoroutineEnumerator
        {
            Type enumerator_ = typeof(T);
            managerLock.ReadBegin();
            bool found = managersDictionary.TryGetValue(enumerator_, out ManagerBase manager);
            managerLock.ReadEnd();
            if (!found)
                manager = CreateManager<T>();
            TypedManager<T> manager_ = (TypedManager<T>)manager;
#if UNITY_WEBGL
            Debug.Assert(UnityThread.IsMainThread, "WebGL doesn't support multithreading. If it does, remove this optimization then.");
            manager_.Start(coroutine);
#else
            if (UnityThread.IsMainThread)
                manager_.Start(coroutine);
            else
                manager_.ConcurrentStart(coroutine, ThreadMode.Unknown);
#endif
        }

#if !UNITY_WEBGL
        private void StartEnumeratorInner<T>(T coroutine, ThreadMode mode) where T : IValueCoroutineEnumerator
        {
            Type enumerator_ = typeof(T);
            managerLock.ReadBegin();
            bool found = managersDictionary.TryGetValue(enumerator_, out ManagerBase manager);
            managerLock.ReadEnd();
            if (!found)
                manager = CreateManager<T>();
            ((TypedManager<T>)manager).ConcurrentStart(coroutine, mode);
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void StartNestedEnumerator<T>(T coroutine) where T : IValueCoroutineEnumerator
        {
            if (state == ValueCoroutineState.Finalized)
                return;

            if (typeof(T).IsValueType)
                StartEnumeratorInner(coroutine);
            else
                StartEnumeratorInner((IValueCoroutineEnumerator)coroutine);
        }

#if !UNITY_WEBGL
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ConcurrentStartNestedEnumerator<T>(T coroutine, ThreadMode mode) where T : IValueCoroutineEnumerator
        {
            if (state == ValueCoroutineState.Finalized)
                return;

            if (typeof(T).IsValueType)
                StartEnumeratorInner(coroutine, mode);
            else
                StartEnumeratorInner((IValueCoroutineEnumerator)coroutine, mode);
        }
#endif

        [MethodImpl(MethodImplOptions.NoInlining)]
        private TypedManager<T> CreateManager<T>() where T : IValueCoroutineEnumerator
        {
            managerLock.WriteBegin();
            if (!managersDictionary.TryGetValue(typeof(T), out ManagerBase manager))
                managersDictionary.Add(typeof(T), manager = new TypedManager<T>(this));
            managersList.Add(manager);
            managerLock.WriteEnd();
            return (TypedManager<T>)manager;
        }

        /// <inheritdoc cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            if (state == ValueCoroutineState.Finalized)
                return;
            state = ValueCoroutineState.Finalized;

            GC.SuppressFinalize(this);

            RawQueue<ValueTask> tasks = RawQueue<ValueTask>.Create();
            managerLock.WriteBegin();
            RawList<ManagerBase> managers = managersList;
            for (int i = 0; i < managers.Count; i++)
                managers[i].Dispose(ref tasks);
            monoBehaviour = null;
            managerLock.WriteEnd();

            while (tasks.TryDequeue(out ValueTask task))
            {
                if (task.IsCompleted)
                {
                    if (task.IsFaulted)
                        Debug.LogException(task.AsTask().Exception);
                }
                else
                    task.AsTask().ContinueWith(logExceptionIfFaulted);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void StopUnityCoroutine(Coroutine coroutine)
            => monoBehaviour.StopCoroutine(coroutine);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Coroutine StartUnityCoroutine(IEnumerator coroutine)
            => monoBehaviour.StartCoroutine(coroutine);

        private static void ThrowOutOfRangePercentExecutions() => throw new ArgumentOutOfRangeException("percentOfExecutions", "Must be from 0 to 1.");

        private static void ThrowOutOfRangeMilisecondsExecuted() => throw new ArgumentOutOfRangeException("milisecondsExecuted", "Can't be negative.");

        private static void ThrowMonoBehaviourNullException() => throw new ArgumentNullException("monoBehaviour");

        private static void ThrowObjectDisposedException() => throw new ObjectDisposedException("Manager");

        private static void ThrowNonUnityThread() => throw new InvalidOperationException("This function can only be executed in the Unity thread.");

        private static void ThrowIsAlreadySuspended() => throw new InvalidOperationException("The manager is already suspended.");

        private static void ThrowIsNotSuspended() => throw new InvalidOperationException("The manager is not suspended");
    }
}