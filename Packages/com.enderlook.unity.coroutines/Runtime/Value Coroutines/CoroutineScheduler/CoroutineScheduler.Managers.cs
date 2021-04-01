using Enderlook.Collections.LowLevel;
using Enderlook.Unity.Threading;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    public partial struct CoroutineScheduler
    {
        internal partial class Managers
        {
            private const byte UnknownThread = 0;
            private const byte ShortThread = 1;
            private const byte LongThread = 2;

            private MonoBehaviour monoBehaviour;
            private Dictionary<(Type enumerator, Type cancellator), ManagerBase> managersDictionary = new Dictionary<(Type enumerator, Type cancellator), ManagerBase>();
            private RawList<ManagerBase> managersList = RawList<ManagerBase>.Create();
            private ReadWriterLock managerLock;

            private PollEnumerator pollEnumerator;

            public int MilisecondsExecutedPerFrameOnPoll {
                get => milisecondsExecutedPerFrameOnPoll;
                set {
                    if (value < 0)
                        ThrowOutOfRangeMilisecondsExecuted();
                    milisecondsExecutedPerFrameOnPoll = value;
                }
            }
            private int milisecondsExecutedPerFrameOnPoll = 5;

            public float MinimumPercentOfExecutionsPerFrameOnPoll {
                get => minimumPercentOfExecutionsPerFrameOnPoll;
                set {
                    if (value < 0)
                        ThrowOutOfRangePercentExecutions();
                    minimumPercentOfExecutionsPerFrameOnPoll = value;
                }
            }
            private float minimumPercentOfExecutionsPerFrameOnPoll = .025f;

            public Managers() => pollEnumerator = new PollEnumerator(this);

            public void SetMonoBehaviour(MonoBehaviour monoBehaviour)
            {
                if (monoBehaviour is null)
                    throw new ArgumentNullException(nameof(monoBehaviour));

                if (this.monoBehaviour is null)
                    this.monoBehaviour = monoBehaviour;
                else
                    throw new InvalidOperationException($"Already has set a {nameof(MonoBehaviour)}.");
            }

            public void Start<T, U>(U cancellator, T routine)
                where T : IEnumerator<ValueYieldInstruction>
                where U : ICancellable
            {
                if (typeof(T).IsValueType)
                {
                    if (typeof(U).IsValueType)
                        StartInner(cancellator, routine);
                    else
                        StartInner((ICancellable)cancellator, routine);
                }
                else
                {
                    if (typeof(U).IsValueType)
                        StartInner(new Uncancellable(), (IEnumerator<ValueYieldInstruction>)routine);
                    else
                        StartInner((ICancellable)cancellator, (IEnumerator<ValueYieldInstruction>)routine);
                }
            }

            private void StartInner<T, U>(U cancellator, T routine)
                where T : IEnumerator<ValueYieldInstruction>
                where U : ICancellable
            {
                Type enumerator_ = typeof(T);
                Type cancellator_ = typeof(U);

                managerLock.ReadBegin();
                if (managersDictionary.TryGetValue((enumerator_, cancellator_), out ManagerBase manager))
                {
                    managerLock.ReadEnd();
                    ((TypedManager<T, U>)manager).Start(cancellator, routine);
                }
                else
                {
                    managerLock.UpgradeFromReaderToWriter();
                    if (managersDictionary.TryGetValue((enumerator_, cancellator_), out manager))
                    {
                        managerLock.WriteEnd();
                        ((TypedManager<T, U>)manager).Start(cancellator, routine);
                    }
                    else
                    {
                        TypedManager<T, U> manager_ = new TypedManager<T, U>(this);
                        managersDictionary.Add((enumerator_, cancellator_), manager_);
                        managersList.Add(manager_);
                        managerLock.WriteEnd();
                        manager_.Start(cancellator, routine);
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ValueCoroutine StartWithHandle<T, U>(U cancellator, T routine)
               where T : IEnumerator<ValueYieldInstruction>
               where U : ICancellable
                => ValueCoroutine.Start(this, cancellator, routine);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ValueCoroutine StartWithHandleThreadSafe<T, U>(U cancellator, T routine)
               where T : IEnumerator<ValueYieldInstruction>
               where U : ICancellable
                => ValueCoroutine.StartThreadSafe(this, cancellator, routine);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void StartThreadSafe<T, U>(U cancellator, T routine)
               where T : IEnumerator<ValueYieldInstruction>
               where U : ICancellable
                => StartThreadSafe(cancellator, routine, UnknownThread);

            private void StartThreadSafe<T, U>(U cancellator, T routine, int mode)
                where T : IEnumerator<ValueYieldInstruction>
                where U : ICancellable
            {
                if (typeof(T).IsValueType)
                {
                    if (typeof(U).IsValueType)
                        StartThreadSafeInner(cancellator, routine, mode);
                    else
                        StartThreadSafeInner((ICancellable)cancellator, routine, mode);
                }
                else
                {
                    if (typeof(U).IsValueType)
                        StartThreadSafeInner(new Uncancellable(), (IEnumerator<ValueYieldInstruction>)routine, mode);
                    else
                        StartThreadSafeInner((ICancellable)cancellator, (IEnumerator<ValueYieldInstruction>)routine, mode);
                }
            }

            private void StartThreadSafeInner<T, U>(U cancellator, T routine, int mode)
                where T : IEnumerator<ValueYieldInstruction>
                where U : ICancellable
            {
#if DEBUG
                if (Application.platform == RuntimePlatform.WebGLPlayer)
                    Debug.LogWarning("This platform doesn't support multithreading. This doesn't mean that the function will fail (it works), but it would be more perfomant to call the non-concurrent version instead.");
                else if (UnityThread.IsMainThread)
                    Debug.LogWarning("This function was executed in the main thread. This is not an error, thought it's more perfomant to call the non-concurrent version instead.");
#endif

                Type enumerator_ = typeof(T);
                Type cancellator_ = typeof(U);

                managerLock.ReadBegin();
                if (managersDictionary.TryGetValue((enumerator_, cancellator_), out ManagerBase manager))
                {
                    managerLock.ReadEnd();
                    ((TypedManager<T, U>)manager).StartThreadSafe(cancellator, routine, UnknownThread);
                }
                else
                {
                    managerLock.UpgradeFromReaderToWriter();
                    if (managersDictionary.TryGetValue((enumerator_, cancellator_), out manager))
                    {
                        managerLock.WriteEnd();
                        ((TypedManager<T, U>)manager).StartThreadSafe(cancellator, routine, mode);
                    }
                    else
                    {
                        TypedManager<T, U> manager_ = new TypedManager<T, U>(this);
                        managersDictionary.Add((enumerator_, cancellator_), manager_);
                        managersList.Add(manager_);
                        managerLock.WriteEnd();
                        manager_.StartThreadSafe(cancellator, routine, mode);
                    }
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
                foreach (ManagerBase manager in managersDictionary.Values)
                    manager.Free();
                monoBehaviour = null;
                managerLock.WriteEnd();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void StopUnityCoroutine(UnityEngine.Coroutine coroutine)
                => monoBehaviour.StopCoroutine(coroutine);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private UnityEngine.Coroutine StartUnityCoroutine(IEnumerator coroutine)
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