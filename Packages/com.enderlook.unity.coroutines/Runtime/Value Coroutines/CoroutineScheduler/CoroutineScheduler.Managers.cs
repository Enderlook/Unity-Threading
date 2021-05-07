using Enderlook.Collections.LowLevel;
using Enderlook.Threading;
using Enderlook.Unity.Threading;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    public partial struct CoroutineScheduler
    {
        [Serializable]
        internal partial class Managers
        {
            private ValueCoroutineStateBoxed state;
            private MonoBehaviour monoBehaviour;
            private Dictionary<Type, ManagerBase> managersDictionary = new Dictionary<Type, ManagerBase>();
            private RawList<ManagerBase> managersList = RawList<ManagerBase>.Create();
            private ReadWriterLock managerLock;

            private PollEnumerator pollEnumerator;
            private readonly Action<ValueCoroutineStateBoxed> backgroundAction;

            public int MilisecondsExecutedPerFrameOnPoll {
                get => milisecondsExecutedPerFrameOnPoll;
                set {
                    if (value < 0)
                        ThrowOutOfRangeMilisecondsExecuted();
                    milisecondsExecutedPerFrameOnPoll = value;
                }
            }
            [SerializeField, Min(0), Tooltip("Amount of miliseconds spent in executing poll coroutines")]
            private int milisecondsExecutedPerFrameOnPoll = 5;

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

            public Managers()
            {
                pollEnumerator = new PollEnumerator(this);
                if (Application.platform != RuntimePlatform.WebGLPlayer)
                {
                    backgroundAction = (ValueCoroutineStateBoxed s) =>
                    {
                        int index = 0;
                        while (s.State != ValueCoroutineState.Finalized)
                        {
                            managerLock.ReadBegin();
                            RawList<ManagerBase> managersList = this.managersList;
                            managerLock.ReadEnd();
                            if (index < managersList.Count)
                                managersList[index++].OnBackground();
                            else
                                index = 0;
                        }
                    };
                }
            }

            ~Managers() => Free();

            public void Initialize(MonoBehaviour monoBehaviour)
            {
                if (monoBehaviour is null)
                    throw new ArgumentNullException(nameof(monoBehaviour));

                if (this.monoBehaviour is null)
                    this.monoBehaviour = monoBehaviour;
                else
                    throw new InvalidOperationException($"Already has set a {nameof(MonoBehaviour)}.");

                state = new ValueCoroutineStateBoxed() { State = ValueCoroutineState.Continue };
                if (Application.platform == RuntimePlatform.WebGLPlayer)
                    Task.Factory.StartNew(backgroundAction, state, TaskCreationOptions.LongRunning);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Start<T>(T coroutine) where T : IValueCoroutineEnumerator
            {
                if (typeof(T).IsValueType)
                    StartInner(coroutine);
                else
                    StartInner((IValueCoroutineEnumerator)coroutine);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void StartInner<T>(T coroutine) where T : IValueCoroutineEnumerator
            {
                Type type = typeof(T);
                managerLock.ReadBegin();
                if (managersDictionary.TryGetValue(type, out ManagerBase manager))
                {
                    managerLock.ReadEnd();
                    ((TypedManager<T>)manager).Start(coroutine);
                }
                else
                {
                    managerLock.UpgradeFromReaderToWriter();
                    TypedManager<T> manager_ = new TypedManager<T>(this);
                    managersDictionary.Add(type, manager_);
                    managersList.Add(manager_);
                    managerLock.WriteEnd();
                    manager_.Start(coroutine);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ValueCoroutine StartWithHandle<T>(T routine) where T : IValueCoroutineEnumerator
                => ValueCoroutine.Start(this, routine);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ValueCoroutine ConcurrentStartWithHandle<T>(T routine) where T : IValueCoroutineEnumerator
                => ValueCoroutine.ConcurrentStart(this, routine);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void ConcurrentStart<T>(T routine) where T : IValueCoroutineEnumerator
            {
                if (typeof(T).IsValueType)
                    ConcurrentStartInner(routine, ThreadMode.Unknown);
                else
                    ConcurrentStartInner((IValueCoroutineEnumerator)routine, ThreadMode.Unknown);
            }

            private void ConcurrentStartInner<T>(T routine, ThreadMode mode)
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
                if (managersDictionary.TryGetValue(enumerator_, out ManagerBase manager))
                {
                    managerLock.ReadEnd();
                    ((TypedManager<T>)manager).ConcurrentStart(routine, mode);
                }
                else
                {
                    managerLock.UpgradeFromReaderToWriter();
                    TypedManager<T> manager_ = new TypedManager<T>(this);
                    managersDictionary.Add(enumerator_, manager_);
                    managersList.Add(manager_);
                    managerLock.WriteEnd();
                    manager_.ConcurrentStart(routine, mode);
                }
            }

            public void OnUpdate()
            {
                managerLock.ReadBegin();
                RawList<ManagerBase> managers = managersList;
                managerLock.ReadEnd();
                for (int i = 0; i < managers.Count; i++)
                    managers[i].OnUpdate();
            }

            public void OnLateUpdate()
            {
                managerLock.ReadBegin();
                RawList<ManagerBase> managers = managersList;
                managerLock.ReadEnd();
                for (int i = 0; i < managers.Count; i++)
                    managers[i].OnLateUpdate();
            }

            public void OnFixedUpdate()
            {
                managerLock.ReadBegin();
                RawList<ManagerBase> managers = managersList;
                managerLock.ReadEnd();
                for (int i = 0; i < managers.Count; i++)
                    managers[i].OnFixedUpdate();
            }

            public void OnEndOfFrame()
            {
                managerLock.ReadBegin();
                RawList<ManagerBase> managers = managersList;
                managerLock.ReadEnd();
                for (int i = 0; i < managers.Count; i++)
                    managers[i].OnEndOfFrame();
            }

            public void OnPoll()
            {
                int total = 0;
                {
                    managerLock.ReadBegin();
                    RawList<ManagerBase> managersList = this.managersList;
                    for (int i = 0; i < managersList.Count; i++)
                        total += managersList[i].PollCount();
                    managerLock.ReadEnd();
                }

                /* TODO: `total` can be lower than the real value (even it can be negative) 
                 * since users can queue to remove multiple times the same task (or tasks that doesn't exists)
                 * but currently `EventsQueue` is just substracting the removal queue from `total`. */

                int to = Mathf.CeilToInt(total * minimumPercentOfExecutionsPerFrameOnPoll);
                int index = 0;
                int until = DateTime.Now.Millisecond + milisecondsExecutedPerFrameOnPoll;
                while (pollEnumerator.MoveNext(until, ref index, to)) ;

                /* TODO: Since users can remove tasks from poll, `i` may actually never reach total
                 * That won't be a deadlock but it will burn a lot of CPU until the timeout is reached. */
            }

            public void Free()
            {
                managerLock.WriteBegin();
                state.State = ValueCoroutineState.Finalized;
                foreach (ManagerBase manager in managersDictionary.Values)
                    manager.Free();
                monoBehaviour = null;
                managerLock.WriteEnd();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void StopUnityCoroutine(Coroutine coroutine)
                => monoBehaviour.StopCoroutine(coroutine);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private Coroutine StartUnityCoroutine(IEnumerator coroutine)
                => monoBehaviour.StartCoroutine(coroutine);

            private struct PollEnumerator
            {
                private readonly Managers managers;
                private int index;

                public PollEnumerator(Managers managers)
                {
                    this.managers = managers;
                    index = 0;
                }

                public bool MoveNext(int until, ref int i, int to)
                {
                    managers.managerLock.ReadBegin();
                    RawList<ManagerBase> managersList = managers.managersList;
                    managers.managerLock.ReadEnd();
                    if (index < managersList.Count)
                    {
                        int old = index;
                        index++;
                        return managersList[old].OnPoll(until, ref i, to);
                    }
                    else
                        index = 0;
                    return false;
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static void ThrowOutOfRangePercentExecutions() => throw new ArgumentOutOfRangeException("percentOfExecutions", "Must be from 0 to 1.");

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static void ThrowOutOfRangeMilisecondsExecuted() => throw new ArgumentOutOfRangeException("milisecondsExecuted", "Can't be negative.");
        }
    }
}