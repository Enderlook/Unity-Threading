using Enderlook.Collections.LowLevel;
using Enderlook.Threading;
using Enderlook.Unity.Threading;

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Unity.Jobs;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    public partial struct CoroutineScheduler
    {
        internal sealed partial class Managers
        {
            private partial class TypedManager<T, U> : ManagerBase
                where T : IEnumerator<ValueYieldInstruction>
                where U : ICancellable
            {
                private readonly Action<Routine> nextShortBackgroundAction;
                private readonly Action<Routine> nextLongBackgroundAction;
                private readonly Managers manager;

                // TODO: Cache locality may be improved by using a circular buffer instead of two lists when iterating each one. Futher reasearch is required.

                private RawList<Routine> onUpdate = RawList<Routine>.Create();
                private RawList<Routine> onLateUpdate = RawList<Routine>.Create();
                private RawList<Routine> onFixedUpdate = RawList<Routine>.Create();
                private RawList<Routine> onEndOfFrame = RawList<Routine>.Create();
                private RawQueue<Routine> onPoll = RawQueue<Routine>.Create();
                private RawList<(CustomYieldInstruction, Routine)> onCustom = RawList<(CustomYieldInstruction, Routine)>.Create();
                private RawList<(Func<bool>, Routine)> onWhile = RawList<(Func<bool>, Routine)>.Create();
                private RawList<(Func<bool>, Routine)> onUntil = RawList<(Func<bool>, Routine)>.Create();
                private RawList<(ValueTask, Routine)> onTask = RawList<(ValueTask, Routine)>.Create();
                private RawList<(JobHandle, Routine)> onJobHandle = RawList<(JobHandle, Routine)>.Create();
                // Waiter with timer can be reduced in time complexity by using priority queues.
                private RawList<(float, Routine)> onTime = RawList<(float, Routine)>.Create();
                private RawList<(float, Routine)> onRealtime = RawList<(float, Routine)>.Create();
                private RawList<(ValueCoroutine, Routine)> onValueCoroutine = RawList<(ValueCoroutine, Routine)>.Create();

                private RawList<(U, UnityEngine.Coroutine)> onUnityCoroutine = RawList<(U, UnityEngine.Coroutine)>.Create();

                private RawList<Routine> tmpT = RawList<Routine>.Create();
                private RawQueue<Routine> tmpTQueue = RawQueue<Routine>.Create();
                private RawList<(CustomYieldInstruction, Routine)> tmpCustom = RawList<(CustomYieldInstruction, Routine)>.Create();
                private RawList<(float, Routine)> tmpFloat = RawList<(float, Routine)>.Create();
                private RawList<(Func<bool>, Routine)> tmpFuncBool = RawList<(Func<bool>, Routine)>.Create();
                private RawList<(ValueTask, Routine)> tmpTask = RawList<(ValueTask, Routine)>.Create();
                private RawList<(JobHandle, Routine)> tmpJobHandle = RawList<(JobHandle, Routine)>.Create();
                private RawList<(ValueCoroutine, Routine)> tmpValueCoroutine = RawList<(ValueCoroutine, Routine)>.Create();

                private readonly ConcurrentBag<Routine> onUpdateBag;
                private readonly ConcurrentBag<Routine> onLateUpdateBag;
                private readonly ConcurrentBag<Routine> onFixedUpdateBag;
                private readonly ConcurrentBag<Routine> onEndOfFrameBag;
                private readonly ConcurrentQueue<Routine> onPollQueue;
                private readonly ConcurrentBag<(CustomYieldInstruction, Routine)> onCustomBag;
                private readonly ConcurrentBag<(Func<bool>, Routine)> onWhileBag;
                private readonly ConcurrentBag<(Func<bool>, Routine)> onUntilBag;
                private readonly ConcurrentBag<(ValueTask, Routine)> onTaskBag;
                private readonly ConcurrentBag<(JobHandle, Routine)> onJobHandleBag;
                private readonly ConcurrentBag<(float, Routine)> onTimeBag;
                private readonly ConcurrentBag<(float, Routine)> onRealtimeBag;
                private readonly ConcurrentBag<(U, IEnumerator)> onUnityCoroutineBag;
                private readonly ConcurrentBag<(ValueCoroutine, Routine)> onValueCoroutineBag;

                public TypedManager(Managers manager)
                {
                    this.manager = manager;

                    nextShortBackgroundAction = (e) =>
                    {
                        if (!e.IsCancelationRequested)
                            NextBackground(e, ShortThread);
                    };
                    nextLongBackgroundAction = (e) =>
                    {
                        if (!e.IsCancelationRequested)
                            NextBackground(e, LongThread);
                    };

                    if (Application.platform != RuntimePlatform.WebGLPlayer)
                    {
                        onUpdateBag = new ConcurrentBag<Routine>();
                        onLateUpdateBag = new ConcurrentBag<Routine>();
                        onFixedUpdateBag = new ConcurrentBag<Routine>();
                        onEndOfFrameBag = new ConcurrentBag<Routine>();
                        onPollQueue = new ConcurrentQueue<Routine>();
                        onCustomBag = new ConcurrentBag<(CustomYieldInstruction, Routine)>();
                        onWhileBag = new ConcurrentBag<(Func<bool>, Routine)>();
                        onUntilBag = new ConcurrentBag<(Func<bool>, Routine)>();
                        onTaskBag = new ConcurrentBag<(ValueTask, Routine)>();
                        onJobHandleBag = new ConcurrentBag<(JobHandle, Routine)>();
                        onTimeBag = new ConcurrentBag<(float, Routine)>();
                        onRealtimeBag = new ConcurrentBag<(float, Routine)>();
                        onUnityCoroutineBag = new ConcurrentBag<(U, IEnumerator)>();
                        onValueCoroutineBag = new ConcurrentBag<(ValueCoroutine, Routine)>();
                    }
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Start(U cancellator, T coroutine)
                    => Next(new Routine(cancellator, coroutine));

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void StartThreadSafe(U cancellator, T coroutine, int mode)
                    => NextBackground(new Routine(cancellator, coroutine), mode);

                public override void OnUpdate()
                {
                    RawList<Routine> local = onUpdate;
                    onUpdate = tmpT;

                    for (int i = 0; i < local.Count; i++)
                    {
                        Routine routine = local[i];
                        if (!routine.IsCancelationRequested)
                            Next(local[i]);
                    }

                    ConcurrentBag<Routine> bag = onUpdateBag;
                    if (!(bag is null))
                    {
                        while (bag.TryTake(out Routine routine))
                            if (!routine.IsCancelationRequested)
                                Next(routine);
                    }

                    local.Clear();
                    tmpT = local;

                    OnOthers();
                }

                public override void OnLateUpdate()
                {
                    RawList<Routine> local = onLateUpdate;
                    onLateUpdate = tmpT;

                    for (int i = 0; i < local.Count; i++)
                    {
                        Routine routine = local[i];
                        if (!routine.IsCancelationRequested)
                            Next(local[i]);
                    }

                    ConcurrentBag<Routine> bag = onLateUpdateBag;
                    if (!(bag is null))
                    {
                        while (bag.TryTake(out Routine routine))
                            if (!routine.IsCancelationRequested)
                                Next(routine);
                    }

                    local.Clear();
                    tmpT = local;
                }

                public override void OnFixedUpdate()
                {
                    RawList<Routine> local = onFixedUpdate;
                    onFixedUpdate = tmpT;

                    for (int i = 0; i < local.Count; i++)
                    {
                        Routine routine = local[i];
                        if (!routine.IsCancelationRequested)
                            Next(local[i]);
                    }

                    ConcurrentBag<Routine> bag = onFixedUpdateBag;
                    if (!(bag is null))
                    {
                        while (bag.TryTake(out Routine routine))
                            if (!routine.IsCancelationRequested)
                                Next(routine);
                    }

                    local.Clear();
                    tmpT = local;
                }

                public override void OnEndOfFrame()
                {
                    RawList<Routine> local = onEndOfFrame;
                    onEndOfFrame = tmpT;

                    for (int i = 0; i < local.Count; i++)
                    {
                        Routine routine = local[i];
                        if (!routine.IsCancelationRequested)
                            Next(local[i]);
                    }

                    ConcurrentBag<Routine> bag = onEndOfFrameBag;
                    if (!(bag is null))
                    {
                        while (bag.TryTake(out Routine routine))
                            if (!routine.IsCancelationRequested)
                                Next(routine);
                    }

                    local.Clear();
                    tmpT = local;
                }

                public override bool OnPoll(int until, float percentOfExecutions)
                {
                    RawQueue<Routine> local = onPoll;
                    onPoll = tmpTQueue;

                    ConcurrentQueue<Routine> queue = onPollQueue;
                    if (!(queue is null))
                    {
                        while (queue.TryDequeue(out Routine routine))
                            local.Enqueue(routine);
                    }

                    float i = 0;
                    int total = local.Count;
                    while (local.TryDequeue(out Routine routine))
                    {
                        i++;
                        if (!routine.IsCancelationRequested)
                            Next(routine);
                        // At least percent of all stored tasks must be executed.
                        if (DateTime.Now.Millisecond >= until && i / total >= percentOfExecutions)
                            break;
                    }

                    tmpTQueue = onPoll;
                    onPoll = local;

                    // TODO: This may be improved with Array.Copy or similar.
                    while (tmpTQueue.TryDequeue(out Routine routine))
                        local.Enqueue(routine);

                    tmpTQueue.Clear();

                    return total > 0;
                }

                private void OnWaitForSeconds()
                {
                    RawList<(float, Routine)> local = onTime;
                    onTime = tmpFloat;

                    for (int i = 0; i < local.Count; i++)
                    {
                        (float condition, Routine routine) tmp = local[i];
                        if (tmp.routine.IsCancelationRequested)
                            continue;
                        if (tmp.condition <= Time.time)
                            Next(tmp.routine);
                        else
                            onTime.Add(tmp);
                    }

                    ConcurrentBag<(float condition, Routine routine)> bag = onTimeBag;
                    if (!(bag is null))
                    {
                        while (bag.TryTake(out (float condition, Routine routine) tmp))
                        {
                            if (tmp.routine.IsCancelationRequested)
                                continue;
                            if (tmp.condition <= Time.time)
                                Next(tmp.routine);
                            else
                                onTime.Add(tmp);
                        }
                    }

                    local.Clear();
                    tmpFloat = local;
                }

                private void OnWaitForRealtimeSeconds()
                {
                    RawList<(float, Routine)> local = onRealtime;
                    onRealtime = tmpFloat;

                    for (int i = 0; i < local.Count; i++)
                    {
                        (float condition, Routine routine) tmp = local[i];
                        if (tmp.routine.IsCancelationRequested)
                            continue;
                        if (tmp.condition <= Time.realtimeSinceStartup)
                            Next(tmp.routine);
                        else
                            onRealtime.Add(tmp);
                    }

                    ConcurrentBag<(float condition, Routine routine)> bag = onRealtimeBag;
                    if (!(bag is null))
                    {
                        while (bag.TryTake(out (float condition, Routine routine) tmp))
                        {
                            if (tmp.routine.IsCancelationRequested)
                                continue;
                            if (tmp.condition <= Time.realtimeSinceStartup)
                                Next(tmp.routine);
                            else
                                onTime.Add(tmp);
                        }
                    }

                    local.Clear();
                    tmpFloat = local;
                }

                private void OnCustom()
                {
                    RawList<(CustomYieldInstruction, Routine)> local = onCustom;
                    onCustom = tmpCustom;

                    for (int i = 0; i < local.Count; i++)
                    {
                        (CustomYieldInstruction condition, Routine routine) tmp = local[i];
                        if (tmp.routine.IsCancelationRequested)
                            continue;
                        if (tmp.condition.keepWaiting)
                            onCustom.Add(tmp);
                        else
                            Next(tmp.routine);
                    }

                    ConcurrentBag<(CustomYieldInstruction condition, Routine routine)> bag = onCustomBag;
                    if (!(bag is null))
                    {
                        while (bag.TryTake(out (CustomYieldInstruction condition, Routine routine) tmp))
                        {
                            if (tmp.routine.IsCancelationRequested)
                                continue;
                            if (tmp.condition.keepWaiting)
                                onCustom.Add(tmp);
                            else
                                Next(tmp.routine);
                        }
                    }

                    local.Clear();
                    tmpCustom = local;
                }

                private void OnWhile()
                {
                    RawList<(Func<bool>, Routine)> local = onWhile;
                    onWhile = tmpFuncBool;

                    for (int i = 0; i < local.Count; i++)
                    {
                        (Func<bool> condition, Routine routine) tmp = local[i];
                        if (tmp.routine.IsCancelationRequested)
                            continue;
                        if (tmp.condition())
                            onWhile.Add(tmp);
                        else
                            Next(tmp.routine);
                    }

                    ConcurrentBag<(Func<bool> condition, Routine routine)> bag = onWhileBag;
                    if (!(bag is null))
                    {
                        while (bag.TryTake(out (Func<bool> condition, Routine routine) tmp))
                        {
                            if (tmp.routine.IsCancelationRequested)
                                continue;
                            if (tmp.condition())
                                onWhile.Add(tmp);
                            else
                                Next(tmp.routine);
                        }
                    }

                    local.Clear();
                    tmpFuncBool = local;
                }

                private void OnUntil()
                {
                    RawList<(Func<bool>, Routine)> local = onUntil;
                    onUntil = tmpFuncBool;

                    for (int i = 0; i < local.Count; i++)
                    {
                        (Func<bool> condition, Routine routine) tmp = local[i];
                        if (tmp.routine.IsCancelationRequested)
                            continue;
                        if (tmp.condition())
                            Next(tmp.routine);
                        else
                            onUntil.Add(tmp);
                    }

                    ConcurrentBag<(Func<bool> condition, Routine routine)> bag = onUntilBag;
                    if (!(bag is null))
                    {
                        while (bag.TryTake(out (Func<bool> condition, Routine routine) tmp))
                        {
                            if (tmp.routine.IsCancelationRequested)
                                continue;
                            if (tmp.condition())
                                Next(tmp.routine);
                            else
                                onWhile.Add(tmp);
                        }
                    }

                    local.Clear();
                    tmpFuncBool = local;
                }

                private void OnTask()
                {
                    RawList<(ValueTask, Routine)> local = onTask;
                    onTask = tmpTask;

                    for (int i = 0; i < local.Count; i++)
                    {
                        (ValueTask condition, Routine routine) tmp = local[i];
                        if (tmp.routine.IsCancelationRequested)
                            continue;
                        if (tmp.condition.IsCompleted)
                            Next(tmp.routine);
                        else
                            onTask.Add(tmp);
                    }

                    ConcurrentBag<(ValueTask condition, Routine routine)> bag = onTaskBag;
                    if (!(bag is null))
                    {
                        while (bag.TryTake(out (ValueTask condition, Routine routine) tmp))
                        {
                            if (tmp.routine.IsCancelationRequested)
                                continue;
                            if (tmp.condition.IsCompleted)
                                Next(tmp.routine);
                            else
                                onTask.Add(tmp);
                        }
                    }

                    local.Clear();
                    tmpTask = local;
                }

                private void OnJobHandle()
                {
                    RawList<(JobHandle, Routine)> local = onJobHandle;
                    onJobHandle = tmpJobHandle;

                    for (int i = 0; i < local.Count; i++)
                    {
                        (JobHandle condition, Routine routine) tmp = local[i];
                        if (tmp.routine.IsCancelationRequested)
                            continue;
                        if (tmp.condition.IsCompleted)
                        {
                            tmp.condition.Complete();
                            Next(tmp.routine);
                        }
                        else
                            onJobHandle.Add(tmp);
                    }

                    ConcurrentBag<(JobHandle condition, Routine routine)> bag = onJobHandleBag;
                    if (!(bag is null))
                    {
                        while (bag.TryTake(out (JobHandle condition, Routine routine) tmp))
                        {
                            if (tmp.routine.IsCancelationRequested)
                                continue;
                            if (tmp.condition.IsCompleted)
                            {
                                tmp.condition.Complete();
                                Next(tmp.routine);
                            }
                            else
                                onJobHandle.Add(tmp);
                        }
                    }

                    local.Clear();
                    tmpJobHandle = local;
                }

                private void OnValueCoroutine()
                {
                    RawList<(ValueCoroutine, Routine)> local = onValueCoroutine;
                    onValueCoroutine = tmpValueCoroutine;

                    for (int i = 0; i < local.Count; i++)
                    {
                        (ValueCoroutine condition, Routine routine) tmp = local[i];
                        if (tmp.routine.IsCancelationRequested)
                            continue;
                        if (tmp.condition.IsCompleted)
                            Next(tmp.routine);
                        else
                            onValueCoroutine.Add(tmp);
                    }

                    ConcurrentBag<(ValueCoroutine condition, Routine routine)> bag = onValueCoroutineBag;
                    if (!(bag is null))
                    {
                        while (bag.TryTake(out (ValueCoroutine condition, Routine routine) tmp))
                        {
                            if (tmp.routine.IsCancelationRequested)
                                continue;
                            if (tmp.condition.IsCompleted)
                                Next(tmp.routine);
                            else
                                onValueCoroutine.Add(tmp);
                        }
                    }

                    local.Clear();
                    tmpValueCoroutine = local;
                }

                private void OnOthers()
                {
                    OnWaitForSeconds();
                    OnWaitForRealtimeSeconds();
                    OnWhile();
                    OnUntil();
                    OnCustom();
                    OnTask();
                    OnJobHandle();
                    OnValueCoroutine();
                    CheckUnityCoroutines();
                }

                private void CheckUnityCoroutines()
                {
                    RawList<(U, UnityEngine.Coroutine)> onUnityCoroutine = this.onUnityCoroutine;
                    for (int i = onUnityCoroutine.Count - 1; i >= 0; i--)
                    {
                        (U condition, UnityEngine.Coroutine coroutine) tmp = this.onUnityCoroutine[i];
                        if (tmp.condition.IsCancelationRequested)
                        {
                            manager.StopUnityCoroutine(tmp.coroutine);
                            this.onUnityCoroutine.RemoveAt(i);
                        }
                    }

                    ConcurrentBag<(U condition, IEnumerator coroutine)> bag = onUnityCoroutineBag;
                    if (!(bag is null))
                    {
                        while (bag.TryTake(out (U condition, IEnumerator coroutine) tmp))
                        {
                            if (tmp.condition.IsCancelationRequested)
                                continue;
                            UnityEngine.Coroutine coroutine = manager.StartUnityCoroutine(tmp.coroutine);
                            this.onUnityCoroutine.Add((tmp.condition, coroutine));
                        }
                    }
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Next(Routine routine)
                {
                    if (routine.MoveNext())
                    {
                        ValueYieldInstruction instruction = routine.Current;
                        switch (instruction.Mode)
                        {
                            case ValueYieldInstruction.Type.ForUpdate:
                                onUpdate.Add(routine);
                                break;
                            case ValueYieldInstruction.Type.ForLateUpdate:
                                onLateUpdate.Add(routine);
                                break;
                            case ValueYieldInstruction.Type.ForFixedUpdate:
                                onFixedUpdate.Add(routine);
                                break;
                            case ValueYieldInstruction.Type.ForEndOfFrame:
                                onEndOfFrame.Add(routine);
                                break;
                            case ValueYieldInstruction.Type.ForSeconds:
                                onTime.Add((instruction.Float + Time.time, routine));
                                break;
                            case ValueYieldInstruction.Type.ForRealtimeSeconds:
                                onRealtime.Add((instruction.Float + Time.realtimeSinceStartup, routine));
                                break;
                            case ValueYieldInstruction.Type.Until:
                                onUntil.Add((instruction.FuncBool, routine));
                                break;
                            case ValueYieldInstruction.Type.While:
                                onWhile.Add((instruction.FuncBool, routine));
                                break;
                            case ValueYieldInstruction.Type.CustomYieldInstruction:
                                onCustom.Add((instruction.CustomYieldInstruction, routine));
                                break;
                            case ValueYieldInstruction.Type.ValueTask:
                                onTask.Add((instruction.ValueTask, routine));
                                break;
                            case ValueYieldInstruction.Type.JobHandle:
                                onJobHandle.Add((instruction.JobHandle, routine));
                                break;
                            case ValueYieldInstruction.Type.ValueEnumerator:
                                manager.Start(routine.cancellator, new NestedEnumerator<T, U>(this, routine, instruction.ValueEnumerator));
                                break;
                            case ValueYieldInstruction.Type.BoxedEnumerator:
                            {
                                onUnityCoroutine.Add((routine.cancellator, manager.StartUnityCoroutine(Work(instruction.BoxedEnumerator))));
                                break;
                                IEnumerator Work(IEnumerator enumerator)
                                {
                                    yield return enumerator;
                                    Next(routine);
                                }
                            }
                            case ValueYieldInstruction.Type.ValueCoroutine:
                                onValueCoroutine.Add((instruction.ValueCoroutine, routine));
                                break;
                            case ValueYieldInstruction.Type.ToUnity:
#if UNITY_EDITOR
                                Debug.LogWarning($"{nameof(Yield)}.{nameof(Yield.ToUnity)} was yielded from main thread. This will be ignored.");
#endif
                                Next(routine);
                                break;
                            case ValueYieldInstruction.Type.ToBackground:
                                if (Application.platform == RuntimePlatform.WebGLPlayer)
                                {
#if UNITY_EDITOR
                                    Debug.LogWarning($"{nameof(Yield)}.{nameof(Yield.ToBackground)} was yielded but this platform doesn't support multithreading. A fallback to {nameof(Yield)}.{nameof(Yield.Poll)} was used. Be warned that this may produce deadlocks very easily.");
#endif
                                    onPoll.Enqueue(routine);
                                }
                                else
                                    Task.Factory.StartNew(nextShortBackgroundAction, routine);
                                break;
                            case ValueYieldInstruction.Type.ToLongBackground:
                                if (Application.platform == RuntimePlatform.WebGLPlayer)
                                {
#if UNITY_EDITOR
                                    Debug.LogWarning($"{nameof(Yield)}.{nameof(Yield.ToBackground)} was yielded but this platform doesn't support multithreading. A fallback to {nameof(Yield)}.{nameof(Yield.Poll)} was used. Be warned that this may produce deadlocks very easily.");
#endif
                                    onPoll.Enqueue(routine);
                                }
                                else
                                    Task.Factory.StartNew(nextShortBackgroundAction, routine, TaskCreationOptions.LongRunning);
                                break;
                            case ValueYieldInstruction.Type.YieldInstruction:
                            {
                                onUnityCoroutine.Add((routine.cancellator, manager.StartUnityCoroutine(Work(instruction.YieldInstruction))));
                                break;
                                IEnumerator Work(YieldInstruction yieldInstruction)
                                {
                                    yield return yieldInstruction;
                                    Next(routine);
                                }
                            }
                            case ValueYieldInstruction.Type.Poll:
                                onPoll.Enqueue(routine);
                                break;
                        }
                    }
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void NextBackground(Routine routine, int mode)
                {
                    if (routine.MoveNext())
                    {
                        ValueYieldInstruction instruction = routine.Current;
                        switch (instruction.Mode)
                        {
                            case ValueYieldInstruction.Type.ForUpdate:
                                onUpdateBag.Add(routine);
                                break;
                            case ValueYieldInstruction.Type.ForLateUpdate:
                                onLateUpdateBag.Add(routine);
                                break;
                            case ValueYieldInstruction.Type.ForFixedUpdate:
                                onFixedUpdateBag.Add(routine);
                                break;
                            case ValueYieldInstruction.Type.ForEndOfFrame:
                                onEndOfFrameBag.Add(routine);
                                break;
                            case ValueYieldInstruction.Type.ForSeconds:
                                onTimeBag.Add((instruction.Float + Time.time, routine));
                                break;
                            case ValueYieldInstruction.Type.ForRealtimeSeconds:
                                onRealtimeBag.Add((instruction.Float + Time.realtimeSinceStartup, routine));
                                break;
                            case ValueYieldInstruction.Type.Until:
                                onUntilBag.Add((instruction.FuncBool, routine));
                                break;
                            case ValueYieldInstruction.Type.While:
                                onWhileBag.Add((instruction.FuncBool, routine));
                                break;
                            case ValueYieldInstruction.Type.CustomYieldInstruction:
                                onCustomBag.Add((instruction.CustomYieldInstruction, routine));
                                break;
                            case ValueYieldInstruction.Type.ValueTask:
                                onTaskBag.Add((instruction.ValueTask, routine));
                                break;
                            case ValueYieldInstruction.Type.JobHandle:
                                onJobHandleBag.Add((instruction.JobHandle, routine));
                                break;
                            case ValueYieldInstruction.Type.ValueEnumerator:
                                manager.StartThreadSafe(routine.cancellator, new NestedEnumeratorBackground<T, U>(this, routine, instruction.ValueEnumerator, mode), mode);
                                break;
                            case ValueYieldInstruction.Type.BoxedEnumerator:
                            {
                                onUnityCoroutineBag.Add((routine.cancellator, Work(instruction.BoxedEnumerator)));
                                break;
                                IEnumerator Work(IEnumerator enumerator)
                                {
                                    yield return enumerator;
                                    Next(routine);
                                }
                            }
                            case ValueYieldInstruction.Type.ValueCoroutine:
                                onValueCoroutineBag.Add((instruction.ValueCoroutine, routine));
                                break;
                            case ValueYieldInstruction.Type.ToUnity:
#if UNITY_EDITOR
                                Debug.LogWarning($"{nameof(Yield)}.{nameof(Yield.ToUnity)} was yielded and it allocates memory. Alternatively you could use other methods such as {nameof(Yield)}.{nameof(Yield.ToUpdate)} which doesn't allocate and has a similar effect.");
#endif
                                // TODO: Allocations can be reduced.
                                Switch.OnUnityLater(() => Next(routine));
                                break;
                            case ValueYieldInstruction.Type.ToBackground:
#if UNITY_EDITOR
                                Debug.Assert(Application.platform != RuntimePlatform.WebGLPlayer);
#endif
                                if (mode == ShortThread)
                                {
#if UNITY_EDITOR
                                    Debug.LogWarning($"{nameof(Yield)}.{nameof(Yield.ToBackground)} was yielded from a background thread. This will be ignored.");
#endif
                                    NextBackground(routine, mode);
                                }
                                else
                                    Task.Factory.StartNew(nextLongBackgroundAction, routine);
                                break;
                            case ValueYieldInstruction.Type.ToLongBackground:
#if UNITY_EDITOR
                                Debug.Assert(Application.platform != RuntimePlatform.WebGLPlayer);
#endif
                                if (mode == LongThread)
                                {
#if UNITY_EDITOR
                                    Debug.LogWarning($"{nameof(Yield)}.{nameof(Yield.ToBackground)} was yielded from a long background thread. This will be ignored.");
#endif
                                    NextBackground(routine, mode);
                                }
                                else
                                    Task.Factory.StartNew(nextLongBackgroundAction, routine, TaskCreationOptions.LongRunning);
                                break;
                            case ValueYieldInstruction.Type.YieldInstruction:
                            {
                                onUnityCoroutineBag.Add((routine.cancellator, Work(instruction.YieldInstruction)));
                                break;
                                IEnumerator Work(YieldInstruction yieldInstruction)
                                {
                                    yield return yieldInstruction;
                                    Next(routine);
                                }
                            }
                        }
                    }
                }

                public override void Free()
                {
                    RawList<Routine> onUpdate = this.onUpdate;
                    for (int i = 0; i < onUpdate.Count; i++)
                        onUpdate[i].Dispose();
                    this.onUpdate.Clear();

                    RawList<Routine> onFixedUpdate = this.onFixedUpdate;
                    for (int i = 0; i < onFixedUpdate.Count; i++)
                        onFixedUpdate[i].Dispose();
                    this.onFixedUpdate.Clear();

                    RawList<Routine> onLateUpdate = this.onLateUpdate;
                    for (int i = 0; i < onLateUpdate.Count; i++)
                        onLateUpdate[i].Dispose();
                    this.onLateUpdate.Clear();

                    RawList<Routine> onEndOfFrame = this.onEndOfFrame;
                    for (int i = 0; i < onEndOfFrame.Count; i++)
                        onEndOfFrame[i].Dispose();
                    this.onEndOfFrame.Clear();

                    while (onPoll.TryDequeue(out Routine routine))
                        routine.Dispose();

                    RawList<(CustomYieldInstruction, Routine)> onCustom = this.onCustom;
                    for (int i = 0; i < onCustom.Count; i++)
                        onCustom[i].Item2.Dispose();
                    this.onCustom.Clear();

                    RawList<(Func<bool>, Routine)> onWhile = this.onWhile;
                    for (int i = 0; i < onWhile.Count; i++)
                        onWhile[i].Item2.Dispose();
                    this.onWhile.Clear();

                    RawList<(Func<bool>, Routine)> onUntil = this.onUntil;
                    for (int i = 0; i < onUntil.Count; i++)
                        onUntil[i].Item2.Dispose();
                    this.onUntil.Clear();

                    RawList<(ValueTask, Routine)> onTask = this.onTask;
                    for (int i = 0; i < onTask.Count; i++)
                        onTask[i].Item2.Dispose();
                    this.onTask.Clear();

                    RawList<(JobHandle, Routine)> onJobHandle = this.onJobHandle;
                    for (int i = 0; i < onJobHandle.Count; i++)
                        onJobHandle[i].Item2.Dispose();
                    this.onJobHandle.Clear();
                    
                    RawList<(float, Routine)> onTime = this.onTime;
                    for (int i = 0; i < onTime.Count; i++)
                        onTime[i].Item2.Dispose();
                    this.onTime.Clear();
                    
                    RawList<(float, Routine)> onRealtime = this.onRealtime;
                    for (int i = 0; i < onRealtime.Count; i++)
                        onRealtime[i].Item2.Dispose();
                    this.onRealtime.Clear();
                    
                    RawList<(ValueCoroutine, Routine)> onValueCoroutine = this.onValueCoroutine;
                    for (int i = 0; i < onValueCoroutine.Count; i++)
                        onValueCoroutine[i].Item2.Dispose();
                    this.onValueCoroutine.Clear();
                    
                    RawList<(U, UnityEngine.Coroutine)> onUnityCoroutine = this.onUnityCoroutine;
                    for (int i = 0; i < onUnityCoroutine.Count; i++)
                        manager.StopUnityCoroutine(onUnityCoroutine[i].Item2);
                    this.onUnityCoroutine.Clear();

                    RawList<Routine> tmpT = this.tmpT;
                    for (int i = 0; i < tmpT.Count; i++)
                        tmpT[i].Dispose();
                    this.tmpT.Clear();

                    while (tmpTQueue.TryDequeue(out Routine routine))
                        routine.Dispose();

                    RawList<(CustomYieldInstruction, Routine)> tmpCustom = this.tmpCustom;
                    for (int i = 0; i < tmpCustom.Count; i++)
                        tmpCustom[i].Item2.Dispose();
                    this.tmpCustom.Clear();

                    RawList<(float, Routine)> tmpFloat = this.tmpFloat;
                    for (int i = 0; i < tmpFloat.Count; i++)
                        tmpFloat[i].Item2.Dispose();
                    this.tmpFloat.Clear();

                    RawList<(Func<bool>, Routine)> tmpFuncBool = this.tmpFuncBool;
                    for (int i = 0; i < tmpFuncBool.Count; i++)
                        tmpFuncBool[i].Item2.Dispose();
                    this.tmpFuncBool.Clear();

                    RawList<(ValueTask, Routine)> tmpTask = this.tmpTask;
                    for (int i = 0; i < tmpTask.Count; i++)
                        tmpTask[i].Item2.Dispose();
                    this.tmpTask.Clear();

                    RawList<(JobHandle, Routine)> tmpJobHandle = this.tmpJobHandle;
                    for (int i = 0; i < tmpJobHandle.Count; i++)
                        tmpJobHandle[i].Item2.Dispose();
                    this.tmpJobHandle.Clear();
                    
                    RawList<(ValueCoroutine, Routine)> tmpValueCoroutine = this.tmpValueCoroutine;
                    for (int i = 0; i < tmpValueCoroutine.Count; i++)
                        tmpValueCoroutine[i].Item2.Dispose();
                    this.tmpValueCoroutine.Clear();

                    if (Application.platform != RuntimePlatform.WebGLPlayer)
                    {
                        ConcurrentBag<Routine> onUpdateBag = this.onUpdateBag;
                        while (onUpdateBag.TryTake(out Routine result))
                            result.Dispose();

                        ConcurrentBag<Routine> onFixedUpdateBag = this.onFixedUpdateBag;
                        while (onFixedUpdateBag.TryTake(out Routine result))
                            result.Dispose();

                        ConcurrentBag<Routine> onLateUpdateBag = this.onLateUpdateBag;
                        while (onLateUpdateBag.TryTake(out Routine result))
                            result.Dispose();

                        ConcurrentBag<Routine> onEndOfFrameBag = this.onEndOfFrameBag;
                        while (onEndOfFrameBag.TryTake(out Routine result))
                            result.Dispose();

                        ConcurrentQueue<Routine> onPollQueue = this.onPollQueue;
                        while (onPollQueue.TryDequeue(out Routine result))
                            result.Dispose();

                        ConcurrentBag<(CustomYieldInstruction, Routine)> onCustomBag = this.onCustomBag;
                        while (onCustomBag.TryTake(out (CustomYieldInstruction, Routine) result))
                            result.Item2.Dispose();

                        ConcurrentBag<(Func<bool>, Routine)> onWhileBag = this.onWhileBag;
                        while (onWhileBag.TryTake(out (Func<bool>, Routine) result))
                            result.Item2.Dispose();

                        ConcurrentBag<(Func<bool>, Routine)> onUntilBag = this.onUntilBag;
                        while (onUntilBag.TryTake(out (Func<bool>, Routine) result))
                            result.Item2.Dispose();

                        ConcurrentBag<(ValueTask, Routine)> onTaskBag = this.onTaskBag;
                        while (onTaskBag.TryTake(out (ValueTask, Routine) result))
                            result.Item2.Dispose();

                        ConcurrentBag<(JobHandle, Routine)> onJobHandleBag = this.onJobHandleBag;
                        while (onJobHandleBag.TryTake(out (JobHandle, Routine) result))
                            result.Item2.Dispose();

                        ConcurrentBag<(float, Routine)> onTimeBag = this.onTimeBag;
                        while (onTimeBag.TryTake(out (float, Routine) result))
                            result.Item2.Dispose();

                        ConcurrentBag<(float, Routine)> onRealtimeBag = this.onRealtimeBag;
                        while (onRealtimeBag.TryTake(out (float, Routine) result))
                            result.Item2.Dispose();

                        ConcurrentBag<(U, IEnumerator)> onUnityCoroutineBag = this.onUnityCoroutineBag;
                        // TODO: In .Net Standard 2.1, ConcurrentBag<T>.Clear() method exists.
                        while (onUnityCoroutineBag.TryTake(out _)) ;

                        ConcurrentBag<(ValueCoroutine, Routine)> onValueCoroutineBag = this.onValueCoroutineBag;
                        while (onValueCoroutineBag.TryTake(out (ValueCoroutine, Routine) result))
                            result.Item2.Dispose();
                    }
                }
            }
        }
    }
}