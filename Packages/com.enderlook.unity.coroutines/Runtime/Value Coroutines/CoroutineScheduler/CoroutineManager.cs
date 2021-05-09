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
        internal static readonly CoroutineManager Shared;

        private ValueCoroutineState state;
        private MonoBehaviour monoBehaviour;
        private Dictionary<Type, ManagerBase> managersDictionary = new Dictionary<Type, ManagerBase>();
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

        static CoroutineManager()
        {
            Shared = new CoroutineManager(Manager.Shared);
            Manager.OnUpdate += Shared.OnUpdate;
            Manager.OnUpdate += Shared.OnPoll;
            Manager.OnFixedUpdate += Shared.OnFixedUpdate;
            Manager.OnEndOfFrame += Shared.OnEndOfFrame;
            Manager.OnLateUpdate += Shared.OnEndOfFrame;

            if (Application.platform != RuntimePlatform.WebGLPlayer)
            {
                new Thread(() => {
                    while (true)
                        Shared.OnBackground();
                }).Start();
            }
        }

        ~CoroutineManager() => Dispose();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ValueCoroutine StartEnumeratorWithHandle<T>(T routine) where T : IValueCoroutineEnumerator
            => ValueCoroutine.StartEnumerator(this, routine);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ValueCoroutine ConcurrentStartEnumeratorWithHandle<T>(T routine) where T : IValueCoroutineEnumerator
            => ValueCoroutine.ConcurrentStartEnumerator(this, routine);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void StartEnumerator<T>(T coroutine) where T : IValueCoroutineEnumerator
        {
            CheckThread();
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
            ((TypedManager<T>)manager).Start(coroutine);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ConcurrentStartEnumerator<T>(T coroutine) where T : IValueCoroutineEnumerator
        {
            if (typeof(T).IsValueType)
                ConcurrentStartEnumeratorInner(coroutine, ThreadMode.Unknown);
            else
                ConcurrentStartEnumeratorInner((IValueCoroutineEnumerator)coroutine, ThreadMode.Unknown);
        }

        private void ConcurrentStartEnumeratorInner<T>(T coroutine, ThreadMode mode)
            where T : IValueCoroutineEnumerator
        {
#if DEBUG
            if (Application.platform == RuntimePlatform.WebGLPlayer)
                Debug.LogWarning("This platform doesn't support multithreading. This doesn't mean that the function will fail (it works), but it would be more perfomant to call the non-concurrent version instead.");
            else if (UnityThread.IsMainThread)
                Debug.LogWarning("This function was executed in the main thread. This is not an error, thought it's more perfomant to call the non-concurrent version instead.");
#endif

            Type enumerator_ = typeof(T);
            managerLock.ReadBegin();
            bool found = managersDictionary.TryGetValue(enumerator_, out ManagerBase manager);
            managerLock.ReadEnd();
            if (!found)
                manager = CreateManager<T>();
            ((TypedManager<T>)manager).ConcurrentStart(coroutine, mode);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private TypedManager<T> CreateManager<T>() where T : IValueCoroutineEnumerator
        {
            managerLock.WriteBegin();
            ManagerBase manager = new TypedManager<T>(this);
            managersDictionary.Add(typeof(T), manager);
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
            foreach (ManagerBase manager in managersDictionary.Values)
                manager.Dispose(ref tasks);
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

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowOutOfRangePercentExecutions() => throw new ArgumentOutOfRangeException("percentOfExecutions", "Must be from 0 to 1.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowOutOfRangeMilisecondsExecuted() => throw new ArgumentOutOfRangeException("milisecondsExecuted", "Can't be negative.");

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowMonoBehaviourNullException() => throw new ArgumentNullException("monoBehaviour");

        [System.Diagnostics.Conditional("DEBUG")]
        private void CheckThread()
        {
#if DEBUG
            if (!UnityThread.IsMainThread)
                Debug.LogError("This function can only be executed in the Unity thread. This has produced undefined behaviour.");
#endif
        }
    }
}