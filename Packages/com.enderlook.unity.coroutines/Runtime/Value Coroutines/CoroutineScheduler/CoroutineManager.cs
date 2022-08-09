using Enderlook.Collections.LowLevel;
using Enderlook.Unity.Threading;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    [Serializable]
    public sealed partial class CoroutineManager
    {
        internal static CoroutineManager Shared;

        private ValueCoroutineState state;
        private MonoBehaviour monoBehaviour;
        private readonly Dictionary<Type, ManagerBase> managersDictionary = new Dictionary<Type, ManagerBase>();
        private RawList<ManagerBase> managersList = RawList<ManagerBase>.Create();
        private ReadWriterLock managerLock;
        private PollEnumerator pollEnumerator;

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
        private int milisecondsExecutedPerFrameOnPoll = 5;

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
        [SerializeField, Range(0, 1), Tooltip("Percentage of total execution that must be executed on poll coroutines regardless of timeout.")]
        private float minimumPercentOfExecutionsPerFrameOnPoll = .025f;

        /// <summary>
        /// Creates a manager whose events must be called manually.
        /// </summary>
        /// <param name="monoBehaviour"><see cref="MonoBehaviour"/> used to fallback Unity coroutines.</param>
        public CoroutineManager(MonoBehaviour monoBehaviour)
        {
            if (monoBehaviour == null)
                ThrowMonoBehaviourNullException();

            pollEnumerator = new PollEnumerator(this);
            state = ValueCoroutineState.Continue;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (!(Shared is null))
                return;

            Manager.OnInitialized(() =>
            {
                Shared = new CoroutineManager(Manager.Shared);
                Manager.OnUpdate += Shared.OnUpdate;
                Manager.OnUpdate += Shared.OnPoll;
                Manager.OnFixedUpdate += Shared.OnFixedUpdate;
                Manager.OnEndOfFrame += Shared.OnEndOfFrame;
                Manager.OnLateUpdate += Shared.OnEndOfFrame;

#if !UNITY_WEBGL
                new Thread(() => {
                    while (true)
                        Shared.OnBackground();
                }).Start();
#endif
            });
        }

        ~CoroutineManager() => Dispose();

        /// <summary>
        /// Suspend the execution of the coroutines of this manager.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown when manager is disposed.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="IsSuspended"/> is <see langword="true"/>.</exception>
        public void Suspend()
        {
            if (state == ValueCoroutineState.Finalized)
                ThrowObjectDisposedException();
            if (state == ValueCoroutineState.Suspended)
                ThrowIsAlreadySuspended();
            state = ValueCoroutineState.Suspended;
        }

        /// <summary>
        /// Suspend the execution of the coroutines of this manager.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown when manager is disposed.</exception>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="IsSuspended"/> is <see langword="false"/>.</exception>
        public void Reanude()
        {
            if (state == ValueCoroutineState.Finalized)
                ThrowObjectDisposedException();
            if (state == ValueCoroutineState.Continue)
                ThrowIsNotSuspended();
            state = ValueCoroutineState.Suspended;
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

            RawQueue<ValueTask> tasks = RawQueue<ValueTask>.Create();
            managerLock.WriteBegin();
            state = ValueCoroutineState.Finalized;
            foreach (KeyValuePair<Type, ManagerBase> kvp in managersDictionary) // Don't use .Values to prevent an allocation.
                kvp.Value.Dispose(ref tasks);
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
                    tasks.Enqueue(task);
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