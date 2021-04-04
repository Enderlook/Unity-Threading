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

                private PackList<Routine> onUpdate = PackList<Routine>.Create();
                private PackList<Routine> onFixedUpdate = PackList<Routine>.Create();
                private PackList<Routine> onLateUpdate = PackList<Routine>.Create();
                private PackList<Routine> onEndOfFrame = PackList<Routine>.Create();
                private PackQueue<Routine> onPoll = PackQueue<Routine>.Create();
                private PackList<(CustomYieldInstruction, Routine)> onCustom = PackList<(CustomYieldInstruction, Routine)>.Create();
                private PackList<(Func<bool>, Routine)> onWhile = PackList<(Func<bool>, Routine)>.Create();
                private PackList<(Func<bool>, Routine)> onUntil = PackList<(Func<bool>, Routine)>.Create();
                private PackList<(ValueTask, Routine)> onTask = PackList<(ValueTask, Routine)>.Create();
                private PackList<(JobHandle, Routine)> onJobHandle = PackList<(JobHandle, Routine)>.Create();
                // Waiter with timer can be reduced in time complexity by using priority queues.
                private PackList<(float, Routine)> onTime = PackList<(float, Routine)>.Create();
                private PackList<(float, Routine)> onRealtime = PackList<(float, Routine)>.Create();
                private PackList<(ValueCoroutine, Routine)> onValueCoroutine = PackList<(ValueCoroutine, Routine)>.Create();

                private RawList<(U, UnityEngine.Coroutine)> onUnityCoroutine = RawList<(U, UnityEngine.Coroutine)>.Create();
                private readonly ConcurrentBag<(U, IEnumerator)> onUnityCoroutineBag;

                private RawList<Routine> tmpT = RawList<Routine>.Create();
                private RawQueue<Routine> tmpTQueue = RawQueue<Routine>.Create();
                private RawList<(CustomYieldInstruction, Routine)> tmpCustom = RawList<(CustomYieldInstruction, Routine)>.Create();
                private RawList<(float, Routine)> tmpFloat = RawList<(float, Routine)>.Create();
                private RawList<(Func<bool>, Routine)> tmpFuncBool = RawList<(Func<bool>, Routine)>.Create();
                private RawList<(ValueTask, Routine)> tmpTask = RawList<(ValueTask, Routine)>.Create();
                private RawList<(JobHandle, Routine)> tmpJobHandle = RawList<(JobHandle, Routine)>.Create();
                private RawList<(ValueCoroutine, Routine)> tmpValueCoroutine = RawList<(ValueCoroutine, Routine)>.Create();

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
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Start(U cancellator, T coroutine)
                    => Next(new Routine(cancellator, coroutine));

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void ConcurrentStart(U cancellator, T coroutine, int mode)
                    => NextBackground(new Routine(cancellator, coroutine), mode);

                public override void OnUpdate()
                {
                    RawList<Routine> local = onUpdate.Swap(tmpT);

                    for (int i = 0; i < local.Count; i++)
                    {
                        Routine routine = local[i];
                        if (!routine.IsCancelationRequested)
                            Next(local[i]);
                    }

                    ConcurrentBag<Routine> bag = onUpdate.Concurrent;
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
                    RawList<Routine> local = onLateUpdate.Swap(tmpT);

                    for (int i = 0; i < local.Count; i++)
                    {
                        Routine routine = local[i];
                        if (!routine.IsCancelationRequested)
                            Next(local[i]);
                    }

                    ConcurrentBag<Routine> bag = onLateUpdate.Concurrent;
                    while (bag.TryTake(out Routine routine))
                        if (!routine.IsCancelationRequested)
                            Next(routine);

                    local.Clear();
                    tmpT = local;
                }

                public override void OnFixedUpdate()
                {
                    RawList<Routine> local = onFixedUpdate.Swap(tmpT);

                    for (int i = 0; i < local.Count; i++)
                    {
                        Routine routine = local[i];
                        if (!routine.IsCancelationRequested)
                            Next(local[i]);
                    }

                    ConcurrentBag<Routine> bag = onFixedUpdate.Concurrent;
                    while (bag.TryTake(out Routine routine))
                        if (!routine.IsCancelationRequested)
                            Next(routine);

                    local.Clear();
                    tmpT = local;
                }

                public override void OnEndOfFrame()
                {
                    RawList<Routine> local = onEndOfFrame.Swap(tmpT);

                    for (int i = 0; i < local.Count; i++)
                    {
                        Routine routine = local[i];
                        if (!routine.IsCancelationRequested)
                            Next(local[i]);
                    }

                    ConcurrentBag<Routine> bag = onEndOfFrame.Concurrent;
                    while (bag.TryTake(out Routine routine))
                        if (!routine.IsCancelationRequested)
                            Next(routine);

                    local.Clear();
                    tmpT = local;
                }

                public override int PollCount() => onPoll.Count;

                public override bool OnPoll(int until, ref int i, int to)
                {
                    onPoll.DrainConcurrent();
                    RawQueue<Routine> local = onPoll.Swap(tmpTQueue);

                    bool completed = true;
                    while (local.TryDequeue(out Routine routine))
                    {
                        i++;
                        if (!routine.IsCancelationRequested)
                            Next(routine);
                        if (DateTime.Now.Millisecond >= until && i < to)
                        {
                            completed = false;
                            break;
                        }
                    }

                    RawQueue<Routine> tmp = onPoll.Queue;

                    // TODO: This may be improved with Array.Copy or similar.
                    while (tmp.TryDequeue(out Routine routine))
                        local.Enqueue(routine);

                    onPoll.Queue = local;
                    tmpTQueue = tmp;

                    return completed;
                }

                private void OnWaitForSeconds()
                {
                    RawList<(float, Routine)> local = onTime.Swap(tmpFloat);

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

                    ConcurrentBag<(float condition, Routine routine)> bag = onTime.Concurrent;
                    while (bag.TryTake(out (float condition, Routine routine) tmp))
                    {
                        if (tmp.routine.IsCancelationRequested)
                            continue;
                        if (tmp.condition <= Time.time)
                            Next(tmp.routine);
                        else
                            onTime.Add(tmp);
                    }

                    local.Clear();
                    tmpFloat = local;
                }

                private void OnWaitForRealtimeSeconds()
                {
                    RawList<(float, Routine)> local = onRealtime.Swap(tmpFloat);

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

                    ConcurrentBag<(float condition, Routine routine)> bag = onRealtime.Concurrent;
                    while (bag.TryTake(out (float condition, Routine routine) tmp))
                    {
                        if (tmp.routine.IsCancelationRequested)
                            continue;
                        if (tmp.condition <= Time.realtimeSinceStartup)
                            Next(tmp.routine);
                        else
                            onTime.Add(tmp);
                    }

                    local.Clear();
                    tmpFloat = local;
                }

                private void OnCustom()
                {
                    RawList<(CustomYieldInstruction, Routine)> local = onCustom.Swap(tmpCustom);

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

                    ConcurrentBag<(CustomYieldInstruction condition, Routine routine)> bag = onCustom.Concurrent;
                    while (bag.TryTake(out (CustomYieldInstruction condition, Routine routine) tmp))
                    {
                        if (tmp.routine.IsCancelationRequested)
                            continue;
                        if (tmp.condition.keepWaiting)
                            onCustom.Add(tmp);
                        else
                            Next(tmp.routine);
                    }

                    local.Clear();
                    tmpCustom = local;
                }

                private void OnWhile()
                {
                    RawList<(Func<bool>, Routine)> local = onWhile.Swap(tmpFuncBool);

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

                    ConcurrentBag<(Func<bool> condition, Routine routine)> bag = onWhile.Concurrent;
                    while (bag.TryTake(out (Func<bool> condition, Routine routine) tmp))
                    {
                        if (tmp.routine.IsCancelationRequested)
                            continue;
                        if (tmp.condition())
                            onWhile.Add(tmp);
                        else
                            Next(tmp.routine);
                    }

                    local.Clear();
                    tmpFuncBool = local;
                }

                private void OnUntil()
                {
                    RawList<(Func<bool>, Routine)> local = onUntil.Swap(tmpFuncBool);

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

                    ConcurrentBag<(Func<bool> condition, Routine routine)> bag = onUntil.Concurrent;
                    while (bag.TryTake(out (Func<bool> condition, Routine routine) tmp))
                    {
                        if (tmp.routine.IsCancelationRequested)
                            continue;
                        if (tmp.condition())
                            Next(tmp.routine);
                        else
                            onWhile.Add(tmp);
                    }

                    local.Clear();
                    tmpFuncBool = local;
                }

                private void OnTask()
                {
                    RawList<(ValueTask, Routine)> local = onTask.Swap(tmpTask);

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

                    ConcurrentBag<(ValueTask condition, Routine routine)> bag = onTask.Concurrent;
                    while (bag.TryTake(out (ValueTask condition, Routine routine) tmp))
                    {
                        if (tmp.routine.IsCancelationRequested)
                            continue;
                        if (tmp.condition.IsCompleted)
                            Next(tmp.routine);
                        else
                            onTask.Add(tmp);
                    }

                    local.Clear();
                    tmpTask = local;
                }

                private void OnJobHandle()
                {
                    RawList<(JobHandle, Routine)> local = onJobHandle.Swap(tmpJobHandle);

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

                    ConcurrentBag<(JobHandle condition, Routine routine)> bag = onJobHandle.Concurrent;
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

                    local.Clear();
                    tmpJobHandle = local;
                }

                private void OnValueCoroutine()
                {
                    RawList<(ValueCoroutine, Routine)> local = onValueCoroutine.Swap(tmpValueCoroutine);

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

                    ConcurrentBag<(ValueCoroutine condition, Routine routine)> bag = onValueCoroutine.Concurrent;
                    while (bag.TryTake(out (ValueCoroutine condition, Routine routine) tmp))
                    {
                        if (tmp.routine.IsCancelationRequested)
                            continue;
                        if (tmp.condition.IsCompleted)
                            Next(tmp.routine);
                        else
                            onValueCoroutine.Add(tmp);
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
                    while (bag.TryTake(out (U condition, IEnumerator coroutine) tmp))
                    {
                        if (tmp.condition.IsCancelationRequested)
                            continue;
                        UnityEngine.Coroutine coroutine = manager.StartUnityCoroutine(tmp.coroutine);
                        this.onUnityCoroutine.Add((tmp.condition, coroutine));
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
                            case ValueYieldInstruction.Type.ToUpdate:
                                onUpdate.Add(routine);
                                break;
                            case ValueYieldInstruction.Type.ToLateUpdate:
                                onLateUpdate.Add(routine);
                                break;
                            case ValueYieldInstruction.Type.ToFixedUpdate:
                                onFixedUpdate.Add(routine);
                                break;
                            case ValueYieldInstruction.Type.ToEndOfFrame:
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
                                manager.Start(new NestedEnumerator<T, U>(this, routine, instruction.ValueEnumerator), routine.cancellator);
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
#if DEBUG
                    Debug.Assert(Application.platform != RuntimePlatform.WebGLPlayer);
#endif
                    if (routine.MoveNext())
                    {
                        ValueYieldInstruction instruction = routine.Current;
                        switch (instruction.Mode)
                        {
                            case ValueYieldInstruction.Type.ToUpdate:
                                onUpdate.ConcurrentAdd(routine);
                                break;
                            case ValueYieldInstruction.Type.ToLateUpdate:
                                onLateUpdate.ConcurrentAdd(routine);
                                break;
                            case ValueYieldInstruction.Type.ToFixedUpdate:
                                onFixedUpdate.ConcurrentAdd(routine);
                                break;
                            case ValueYieldInstruction.Type.ToEndOfFrame:
                                onEndOfFrame.ConcurrentAdd(routine);
                                break;
                            case ValueYieldInstruction.Type.ForSeconds:
                                onTime.ConcurrentAdd((instruction.Float + Time.time, routine));
                                break;
                            case ValueYieldInstruction.Type.ForRealtimeSeconds:
                                onRealtime.ConcurrentAdd((instruction.Float + Time.realtimeSinceStartup, routine));
                                break;
                            case ValueYieldInstruction.Type.Until:
                                onUntil.ConcurrentAdd((instruction.FuncBool, routine));
                                break;
                            case ValueYieldInstruction.Type.While:
                                onWhile.ConcurrentAdd((instruction.FuncBool, routine));
                                break;
                            case ValueYieldInstruction.Type.CustomYieldInstruction:
                                onCustom.ConcurrentAdd((instruction.CustomYieldInstruction, routine));
                                break;
                            case ValueYieldInstruction.Type.ValueTask:
                                onTask.ConcurrentAdd((instruction.ValueTask, routine));
                                break;
                            case ValueYieldInstruction.Type.JobHandle:
                                onJobHandle.ConcurrentAdd((instruction.JobHandle, routine));
                                break;
                            case ValueYieldInstruction.Type.ValueEnumerator:
                                manager.ConcurrentStart(new NestedEnumeratorBackground<T, U>(this, routine, instruction.ValueEnumerator, mode), routine.cancellator, mode);
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
                                onValueCoroutine.ConcurrentAdd((instruction.ValueCoroutine, routine));
                                break;
                            case ValueYieldInstruction.Type.ToUnity:
#if DEBUG
                                Debug.LogWarning($"{nameof(Yield)}.{nameof(Yield.ToUnity)} was yielded and it allocates memory. Alternatively you could use other methods such as {nameof(Yield)}.{nameof(Yield.ToUpdate)} which doesn't allocate and has a similar effect.");
#endif
                                // TODO: Allocations can be reduced.
                                UnityThread.RunLater(() => Next(routine));
                                break;
                            case ValueYieldInstruction.Type.ToBackground:
#if DEBUG
                                Debug.Assert(Application.platform != RuntimePlatform.WebGLPlayer);
#endif
                                if (mode == ShortThread)
                                {
#if DEBUG
                                    Debug.LogWarning($"{nameof(Yield)}.{nameof(Yield.ToBackground)} was yielded from a background thread. This will be ignored.");
#endif
                                    NextBackground(routine, mode);
                                }
                                else
                                    Task.Factory.StartNew(nextLongBackgroundAction, routine);
                                break;
                            case ValueYieldInstruction.Type.ToLongBackground:
#if DEBUG
                                Debug.Assert(Application.platform != RuntimePlatform.WebGLPlayer);
#endif
                                if (mode == LongThread)
                                {
#if DEBUG
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
                    onUpdate.Dispose();
                    onFixedUpdate.Dispose();
                    onLateUpdate.Dispose();
                    onEndOfFrame.Dispose();
                    onPoll.Dispose();
                    onCustom.Dispose();
                    onWhile.Dispose();
                    onUntil.Dispose();
                    onTask.Dispose();
                    onJobHandle.Dispose();
                    onTime.Dispose();
                    onRealtime.Dispose();
                    onValueCoroutine.Dispose();

                    RawList<(U, UnityEngine.Coroutine)> onUnityCoroutine = this.onUnityCoroutine;
                    for (int i = 0; i < onUnityCoroutine.Count; i++)
                        manager.StopUnityCoroutine(onUnityCoroutine[i].Item2);
                    this.onUnityCoroutine.Clear();

                    ConcurrentBag<(U, IEnumerator)> onUnityCoroutineBag = this.onUnityCoroutineBag;
                    // TODO: In .Net Standard 2.1, ConcurrentBag<T>.Clear() method exists.
                    while (onUnityCoroutineBag.TryTake(out _)) ;
                }
            }
        }
    }
}