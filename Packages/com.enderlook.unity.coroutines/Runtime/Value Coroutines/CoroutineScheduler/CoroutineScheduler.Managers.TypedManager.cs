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
            private static readonly Action<Task> onFaultTask = e => Debug.LogException(e.Exception);

            private partial class TypedManager<T> : ManagerBase where T : IValueCoroutineEnumerator
            {
                private static readonly Action<(TypedManager<T> manager, ValueCoroutineStateBoxed state, T routine)> shortBackground =
                    e => e.manager.NextBackground(e.routine, new BackgroundShortNextCallback(e.state), e.state, ThreadMode.Short);

                private static readonly Action<(TypedManager<T> manager, ValueCoroutineStateBoxed state, T routine)> longBackground =
                     e => e.manager.NextBackground(e.routine, new BackgroundShortNextCallback(e.state), e.state, ThreadMode.Long);

                private readonly Managers manager;

                // TODO: Cache locality may be improved by using a circular buffer instead of two lists when iterating each one. Futher reasearch is required.

                private PackList<T> onUpdate = PackList<T>.Create();
                private PackList<T> onFixedUpdate = PackList<T>.Create();
                private PackList<T> onLateUpdate = PackList<T>.Create();
                private PackList<T> onEndOfFrame = PackList<T>.Create();
                private PackQueue<T> onUnityPoll = PackQueue<T>.Create();
                private PackList<(CustomYieldInstruction, T)> onCustom = PackList<(CustomYieldInstruction, T)>.Create();
                private PackList<(Func<bool>, T)> onWhile = PackList<(Func<bool>, T)>.Create();
                private PackList<(Func<bool>, T)> onUntil = PackList<(Func<bool>, T)>.Create();
                private PackList<(ValueTask, T)> onTask = PackList<(ValueTask, T)>.Create();
                private PackList<(JobHandle, T)> onJobHandle = PackList<(JobHandle, T)>.Create();
                // Waiter with timer can be reduced in time complexity by using priority queues.
                private PackList<(float, T)> onWaitSeconds = PackList<(float, T)>.Create();
                private PackList<(float, T)> onWaitRealtimeSeconds = PackList<(float, T)>.Create();
                private PackList<(ValueCoroutine, T)> onValueCoroutine = PackList<(ValueCoroutine, T)>.Create();
                private PackList<(Coroutine, T)> onUnityCoroutine = PackList<(Coroutine, T)>.Create();

                private PackList<T> suspendedEntry = PackList<T>.Create();

                private readonly ConcurrentQueue<(ValueCoroutineStateBoxed, T)> suspendedBackgroundShort;
                private readonly ConcurrentQueue<(ValueCoroutineStateBoxed, T)> suspendedBackgroundLong;
                private readonly ConcurrentQueue<Task> backgroundTasks;

                private RawList<T> tmpT = RawList<T>.Create();
                private RawQueue<T> tmpTQueue = RawQueue<T>.Create();
                private RawList<(CustomYieldInstruction, T)> tmpCustom = RawList<(CustomYieldInstruction, T)>.Create();
                private RawList<(float, T)> tmpFloat = RawList<(float, T)>.Create();
                private RawList<(Func<bool>, T)> tmpFuncBool = RawList<(Func<bool>, T)>.Create();
                private RawList<(ValueTask, T)> tmpTask = RawList<(ValueTask, T)>.Create();
                private RawList<(JobHandle, T)> tmpJobHandle = RawList<(JobHandle, T)>.Create();
                private RawList<(ValueCoroutine, T)> tmpValueCoroutine = RawList<(ValueCoroutine, T)>.Create();
                private RawList<(Coroutine, T)> tmpUnityCoroutine = RawList<(Coroutine, T)>.Create();

                public TypedManager(Managers manager)
                {
                    this.manager = manager;
                    if (Application.platform != RuntimePlatform.WebGLPlayer)
                    {
                        suspendedBackgroundShort = new ConcurrentQueue<(ValueCoroutineStateBoxed, T)>();
                        suspendedBackgroundLong = new ConcurrentQueue<(ValueCoroutineStateBoxed, T)>();
                        backgroundTasks = new ConcurrentQueue<Task>();
                    }
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Start(T coroutine)
                    => Next(coroutine, new EntryNextCallback());

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void ConcurrentStart(T coroutine, ThreadMode mode)
                    => NextBackground(coroutine, new EntryNextCallback(), manager.state, mode);

                private void OnEntry()
                {
                    RawList<T> local = suspendedEntry.Swap(tmpT);

                    for (int i = 0; i < local.Count; i++)
                        Next(local[i], new EntryNextCallback());

                    ConcurrentBag<T> bag = onUpdate.Concurrent;
                    while (bag.TryTake(out T routine))
                        Next(routine, new EntryNextCallback());

                    local.Clear();
                    tmpT = local;
                }

                public override void OnUpdate()
                {
                    OnEntry();

                    RawList<T> local = onUpdate.Swap(tmpT);

                    for (int i = 0; i < local.Count; i++)
                        Next(local[i], new UpdateNextCallback());

                    ConcurrentBag<T> bag = onUpdate.Concurrent;
                    while (bag.TryTake(out T routine))
                        Next(routine, new UpdateNextCallback());

                    local.Clear();
                    tmpT = local;

                    OnOthers();
                }

                public override void OnLateUpdate()
                {
                    RawList<T> local = onLateUpdate.Swap(tmpT);

                    for (int i = 0; i < local.Count; i++)
                        Next(local[i], new LateUpdateNextCallback());

                    ConcurrentBag<T> bag = onLateUpdate.Concurrent;
                    while (bag.TryTake(out T routine))
                        Next(routine, new LateUpdateNextCallback());

                    local.Clear();
                    tmpT = local;
                }

                public override void OnFixedUpdate()
                {
                    RawList<T> local = onFixedUpdate.Swap(tmpT);

                    for (int i = 0; i < local.Count; i++)
                        Next(local[i], new FixedUpdateNextCallback());

                    ConcurrentBag<T> bag = onFixedUpdate.Concurrent;
                    while (bag.TryTake(out T routine))
                        Next(routine, new FixedUpdateNextCallback());

                    local.Clear();
                    tmpT = local;
                }

                public override void OnEndOfFrame()
                {
                    RawList<T> local = onEndOfFrame.Swap(tmpT);

                    for (int i = 0; i < local.Count; i++)
                        Next(local[i], new EndOfFrameNextCallback());

                    ConcurrentBag<T> bag = onEndOfFrame.Concurrent;
                    while (bag.TryTake(out T routine))
                        Next(routine, new EndOfFrameNextCallback());

                    local.Clear();
                    tmpT = local;
                }

                public override int PollCount() => onUnityPoll.Count;

                public override bool OnPoll(int until, ref int i, int to)
                {
                    onUnityPoll.DrainConcurrent();
                    RawQueue<T> local = onUnityPoll.Swap(tmpTQueue);

                    bool completed = true;
                    while (local.TryDequeue(out T routine))
                    {
                        i++;
                        Next(routine, new PollNextCallback());
                        if (DateTime.Now.Millisecond >= until && i < to)
                        {
                            completed = false;
                            break;
                        }
                    }

                    RawQueue<T> tmp = onUnityPoll.Queue;
                    // TODO: This may be improved with Array.Copy or similar.
                    while (tmp.TryDequeue(out T routine))
                        local.Enqueue(routine);

                    onUnityPoll.Queue = local;
                    tmpTQueue = tmp;

                    return completed;
                }

                private void OnWaitForSeconds()
                {
                    RawList<(float, T)> local = onWaitSeconds.Swap(tmpFloat);

                    for (int i = 0; i < local.Count; i++)
                    {
                        (float condition, T routine) tmp = local[i];
                        if (tmp.condition <= Time.time)
                            Next(tmp.routine, new WaitForSecondsNextCallback(tmp.condition));
                        else
                            onWaitSeconds.Add(tmp);
                    }

                    ConcurrentBag<(float condition, T routine)> bag = onWaitSeconds.Concurrent;
                    while (bag.TryTake(out (float condition, T routine) tmp))
                    {
                        if (tmp.condition <= Time.time)
                            Next(tmp.routine, new WaitForSecondsNextCallback(tmp.condition));
                        else
                            onWaitSeconds.Add(tmp);
                    }

                    local.Clear();
                    tmpFloat = local;
                }

                private void OnWaitForRealtimeSeconds()
                {
                    RawList<(float, T)> local = onWaitRealtimeSeconds.Swap(tmpFloat);

                    for (int i = 0; i < local.Count; i++)
                    {
                        (float condition, T routine) tmp = local[i];
                        if (tmp.condition <= Time.realtimeSinceStartup)
                            Next(tmp.routine, new WaitForRealtimeSecondsNextCallback(tmp.condition));
                        else
                            onWaitRealtimeSeconds.Add(tmp);
                    }

                    ConcurrentBag<(float condition, T routine)> bag = onWaitRealtimeSeconds.Concurrent;
                    while (bag.TryTake(out (float condition, T routine) tmp))
                    {
                        if (tmp.condition <= Time.realtimeSinceStartup)
                            Next(tmp.routine, new WaitForRealtimeSecondsNextCallback(tmp.condition));
                        else
                            onWaitSeconds.Add(tmp);
                    }

                    local.Clear();
                    tmpFloat = local;
                }

                private void OnCustom()
                {
                    RawList<(CustomYieldInstruction, T)> local = onCustom.Swap(tmpCustom);

                    for (int i = 0; i < local.Count; i++)
                    {
                        (CustomYieldInstruction condition, T routine) tmp = local[i];
                        if (tmp.condition.keepWaiting)
                            onCustom.Add(tmp);
                        else
                            Next(tmp.routine, new CustomNextCallback(tmp.condition));
                    }

                    ConcurrentBag<(CustomYieldInstruction condition, T routine)> bag = onCustom.Concurrent;
                    while (bag.TryTake(out (CustomYieldInstruction condition, T routine) tmp))
                    {
                        if (tmp.condition.keepWaiting)
                            onCustom.Add(tmp);
                        else
                            Next(tmp.routine, new CustomNextCallback(tmp.condition));
                    }

                    local.Clear();
                    tmpCustom = local;
                }

                private void OnWhile()
                {
                    RawList<(Func<bool>, T)> local = onWhile.Swap(tmpFuncBool);

                    for (int i = 0; i < local.Count; i++)
                    {
                        (Func<bool> condition, T routine) tmp = local[i];
                        switch (tmp.routine.State)
                        {
                            case ValueCoroutineState.Continue:
                                if (tmp.condition())
                                    onWhile.Add(tmp);
                                else
                                    Next(tmp.routine, new WhileNextCallback(tmp.condition));
                                break;
                            case ValueCoroutineState.Suspended:
                                onWhile.Add(tmp);
                                break;
                        }
                    }

                    ConcurrentBag<(Func<bool> condition, T routine)> bag = onWhile.Concurrent;
                    while (bag.TryTake(out (Func<bool> condition, T routine) tmp))
                    {
                        switch (tmp.routine.State)
                        {
                            case ValueCoroutineState.Continue:
                                if (tmp.condition())
                                    onWhile.Add(tmp);
                                else
                                    Next(tmp.routine, new WhileNextCallback(tmp.condition));
                                break;
                            case ValueCoroutineState.Suspended:
                                onWhile.Add(tmp);
                                break;
                        }
                    }

                    local.Clear();
                    tmpFuncBool = local;
                }

                private void OnUntil()
                {
                    RawList<(Func<bool>, T)> local = onUntil.Swap(tmpFuncBool);

                    for (int i = 0; i < local.Count; i++)
                    {
                        (Func<bool> condition, T routine) tmp = local[i];
                        switch (tmp.routine.State)
                        {
                            case ValueCoroutineState.Continue:
                                if (!tmp.condition())
                                    onWhile.Add(tmp);
                                else
                                    Next(tmp.routine, new UntilNextCallback(tmp.condition));
                                break;
                            case ValueCoroutineState.Suspended:
                                onWhile.Add(tmp);
                                break;
                        }
                    }

                    ConcurrentBag<(Func<bool> condition, T routine)> bag = onUntil.Concurrent;
                    while (bag.TryTake(out (Func<bool> condition, T routine) tmp))
                    {
                        switch (tmp.routine.State)
                        {
                            case ValueCoroutineState.Continue:
                                if (!tmp.condition())
                                    onWhile.Add(tmp);
                                else
                                    Next(tmp.routine, new UntilNextCallback(tmp.condition));
                                break;
                            case ValueCoroutineState.Suspended:
                                onWhile.Add(tmp);
                                break;
                        }
                    }

                    local.Clear();
                    tmpFuncBool = local;
                }

                private void OnTask()
                {
                    RawList<(ValueTask, T)> local = onTask.Swap(tmpTask);

                    for (int i = 0; i < local.Count; i++)
                    {
                        (ValueTask condition, T routine) tmp = local[i];
                        if (tmp.condition.IsFaulted)
                            Debug.LogException(tmp.condition.AsTask().Exception);
                        else if (tmp.condition.IsCompleted)
                            Next(tmp.routine, new ValueTaskNextCallback(tmp.condition));
                        else
                            onTask.Add(tmp);
                    }

                    ConcurrentBag<(ValueTask condition, T routine)> bag = onTask.Concurrent;
                    while (bag.TryTake(out (ValueTask condition, T routine) tmp))
                    {
                        if (tmp.condition.IsFaulted)
                            Debug.LogException(tmp.condition.AsTask().Exception);
                        else if (tmp.condition.IsCompleted)
                            Next(tmp.routine, new ValueTaskNextCallback(tmp.condition));
                        else
                            onTask.Add(tmp);
                    }

                    local.Clear();
                    tmpTask = local;
                }

                private void OnJobHandle()
                {
                    RawList<(JobHandle, T)> local = onJobHandle.Swap(tmpJobHandle);

                    for (int i = 0; i < local.Count; i++)
                    {
                        (JobHandle condition, T routine) tmp = local[i];
                        if (tmp.condition.IsCompleted)
                        {
                            tmp.condition.Complete();
                            Next(tmp.routine, new JobHandleNextCallback(tmp.condition));
                        }
                        else
                            onJobHandle.Add(tmp);
                    }

                    ConcurrentBag<(JobHandle condition, T routine)> bag = onJobHandle.Concurrent;
                    while (bag.TryTake(out (JobHandle condition, T routine) tmp))
                    {
                        if (tmp.condition.IsCompleted)
                        {
                            tmp.condition.Complete();
                            Next(tmp.routine, new JobHandleNextCallback(tmp.condition));
                        }
                        else
                            onJobHandle.Add(tmp);
                    }

                    local.Clear();
                    tmpJobHandle = local;
                }

                private void OnValueCoroutine()
                {
                    RawList<(ValueCoroutine, T)> local = onValueCoroutine.Swap(tmpValueCoroutine);

                    for (int i = 0; i < local.Count; i++)
                    {
                        (ValueCoroutine condition, T routine) tmp = local[i];
                        if (tmp.condition.IsCompleted)
                            Next(tmp.routine, new ValueCoroutineNextCallback(tmp.condition));
                        else
                            onValueCoroutine.Add(tmp);
                    }

                    ConcurrentBag<(ValueCoroutine condition, T routine)> bag = onValueCoroutine.Concurrent;
                    while (bag.TryTake(out (ValueCoroutine condition, T routine) tmp))
                    {
                        if (tmp.condition.IsCompleted)
                            Next(tmp.routine, new ValueCoroutineNextCallback(tmp.condition));
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
                    RawList<(Coroutine, T)> local = onUnityCoroutine.Swap(tmpUnityCoroutine);
                    for (int i = local.Count - 1; i >= 0; i--)
                    {
                        (Coroutine coroutine, T condition) tmp = local[i];
                        switch (tmp.condition.State)
                        {
                            case ValueCoroutineState.Finalized:
                                manager.StopUnityCoroutine(tmp.coroutine);
                                local.RemoveAt(i);
                                break;
                            case ValueCoroutineState.Suspended:
                                Debug.LogWarning("A value coroutine that yielded a Unity coroutine cannot be suspended until the Unity coroutine ends.");
                                break;
                        }
                    }

                    local.Clear();
                    tmpUnityCoroutine = local;

                    ConcurrentBag<(Coroutine, T)> bag = onUnityCoroutine.Concurrent;
                    while (bag.TryTake(out (Coroutine coroutine, T condition) tmp))
                    {
                        switch (tmp.condition.State)
                        {
                            case ValueCoroutineState.Finalized:
                                manager.StopUnityCoroutine(tmp.coroutine);
                                break;
                            case ValueCoroutineState.Suspended:
                                onUnityCoroutine.Add(tmp);
                                Debug.LogWarning("A value coroutine that yielded a Unity coroutine cannot be suspended until the Unity coroutine ends.");
                                break;
                            case ValueCoroutineState.Continue:
                                onUnityCoroutine.Add(tmp);
                                break;
                        }
                    }
                }

                public override void OnBackground()
                {
                    ConcurrentQueue<Task> tasks = backgroundTasks;
                    int count = tasks.Count;
                    while (count-- > 0 && tasks.TryDequeue(out Task task))
                    {
                        if (task.IsFaulted)
                            Debug.LogException(task.Exception);
                        else if (!task.IsCompleted)
                            tasks.Enqueue(task);
                    }

                    ConcurrentQueue<(ValueCoroutineStateBoxed, T)> local = suspendedBackgroundShort;
                    count = local.Count;
                    while (count-- > 0 && local.TryDequeue(out (ValueCoroutineStateBoxed state, T routine) tmp))
                    {
                        switch (tmp.state.State)
                        {
                            case ValueCoroutineState.Continue:
                                Task.Factory.StartNew(shortBackground, (this, tmp.state, tmp.routine));
                                break;
                            case ValueCoroutineState.Finalized:
                                tmp.routine.Dispose();
                                break;
                            case ValueCoroutineState.Suspended:
                                local.Enqueue(tmp);
                                break;
                        }
                    }

                    local = suspendedBackgroundLong;
                    count = local.Count;
                    while (count-- > 0 && local.TryDequeue(out (ValueCoroutineStateBoxed state, T routine) tmp))
                    {
                        switch (tmp.state.State)
                        {
                            case ValueCoroutineState.Continue:
                                Task.Factory.StartNew(longBackground, (this, tmp.state, tmp.routine));
                                break;
                            case ValueCoroutineState.Finalized:
                                tmp.routine.Dispose();
                                break;
                            case ValueCoroutineState.Suspended:
                                local.Enqueue(tmp);
                                break;
                        }
                    }
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Next<U>(T routine, U callback) where U : INextCallback<T>
                {
                    start:
                    ValueYieldInstruction instruction = routine.Next();
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
                            onWaitSeconds.Add((instruction.Float + Time.time, routine));
                            break;
                        case ValueYieldInstruction.Type.ForRealtimeSeconds:
                            onWaitRealtimeSeconds.Add((instruction.Float + Time.realtimeSinceStartup, routine));
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
                            manager.Start(new NestedEnumerator<T, ValueCoroutineEnumerator<IEnumerator<ValueYieldInstruction>>, U>(this, routine, new ValueCoroutineEnumerator<IEnumerator<ValueYieldInstruction>>(instruction.ValueEnumerator), callback));
                            break;
                        case ValueYieldInstruction.Type.UnityEnumerator:
                            manager.Start(new NestedEnumerator<T, UnityCoroutineEnumerator, U>(this, routine, new UnityCoroutineEnumerator(instruction.UnityEnumerator), callback));
                            break;
                        case ValueYieldInstruction.Type.ValueCoroutine:
                            onValueCoroutine.Add((instruction.ValueCoroutine, routine));
                            break;
                        case ValueYieldInstruction.Type.UnityCoroutine:
                            onUnityCoroutine.Add((instruction.UnityCoroutine, routine));
                            break;
                        case ValueYieldInstruction.Type.ToUnity:
#if UNITY_EDITOR
                            Debug.LogWarning($"{nameof(Yield)}.{nameof(Yield.ToUnity)} was yielded from main thread. This will be ignored.");
#endif
                            goto start;
                        case ValueYieldInstruction.Type.ToBackground:
                            if (Application.platform == RuntimePlatform.WebGLPlayer)
                            {
#if UNITY_EDITOR
                                Debug.LogWarning($"{nameof(Yield)}.{nameof(Yield.ToBackground)} was yielded but this platform doesn't support multithreading. A fallback to {nameof(Yield)}.{nameof(Yield.Poll)} was used. Be warned that this may produce deadlocks very easily.");
#endif
                                onUnityPoll.Enqueue(routine);
                            }
                            else
                                Task.Factory.StartNew(shortBackground, (this, manager.state, routine));
                            break;
                        case ValueYieldInstruction.Type.ToLongBackground:
                            if (Application.platform == RuntimePlatform.WebGLPlayer)
                            {
#if UNITY_EDITOR
                                Debug.LogWarning($"{nameof(Yield)}.{nameof(Yield.ToBackground)} was yielded but this platform doesn't support multithreading. A fallback to {nameof(Yield)}.{nameof(Yield.Poll)} was used. Be warned that this may produce deadlocks very easily.");
#endif
                                onUnityPoll.Enqueue(routine);
                            }
                            else
                                Task.Factory.StartNew(longBackground, (this, manager.state, routine), TaskCreationOptions.LongRunning);
                            break;
                        case ValueYieldInstruction.Type.YieldInstruction:
                        {
                            ValueCoroutineStateBoxed state = manager.state;
                            onUnityCoroutine.Add((manager.StartUnityCoroutine(Work(instruction.YieldInstruction)), routine));
                            break;
                            IEnumerator Work(YieldInstruction yieldInstruction)
                            {
                                while (true)
                                {
                                    switch (state.State)
                                    {
                                        case ValueCoroutineState.Continue:
                                            switch (routine.State)
                                            {
                                                case ValueCoroutineState.Continue:
                                                    yield return yieldInstruction;
                                                    while (true)
                                                    {
                                                        switch (state.State)
                                                        {
                                                            case ValueCoroutineState.Continue:
                                                                switch (routine.State)
                                                                {
                                                                    case ValueCoroutineState.Continue:
                                                                        Next(routine, callback);
                                                                        yield break;
                                                                    case ValueCoroutineState.Suspended:
                                                                        yield return null;
                                                                        break;
                                                                    case ValueCoroutineState.Finalized:
                                                                        routine.Dispose();
                                                                        yield break;
                                                                }
                                                                break;
                                                            case ValueCoroutineState.Suspended:
                                                                yield return null;
                                                                break;
                                                            case ValueCoroutineState.Finalized:
                                                                routine.Dispose();
                                                                yield break;
                                                        }
                                                    }
                                                case ValueCoroutineState.Suspended:
                                                    yield return null;
                                                    break;
                                                case ValueCoroutineState.Finalized:
                                                    routine.Dispose();
                                                    yield break;
                                            }
                                            yield break;
                                        case ValueCoroutineState.Suspended:
                                            yield return null;
                                            break;
                                        case ValueCoroutineState.Finalized:
                                            routine.Dispose();
                                            yield break;
                                    }
                                }
                            }
                        }
                        case ValueYieldInstruction.Type.UnityPoll:
                            onUnityPoll.Enqueue(routine);
                            break;
                        case ValueYieldInstruction.Type.Finalized:
                            routine.Dispose();
                            break;
                        case ValueYieldInstruction.Type.Suspended:
                            callback.Suspend(this, routine);
                            return;
                    }
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void NextBackground<U>(T routine, U callback, ValueCoroutineStateBoxed state, ThreadMode mode) where U : INextCallback<T>
                {
#if DEBUG
                    Debug.Assert(Application.platform != RuntimePlatform.WebGLPlayer);
#endif
                    start:
                    switch (state.ConcurrentState)
                    {
                        case ValueCoroutineState.Finalized:
                            routine.Dispose();
                            break;
                        case ValueCoroutineState.Suspended:
                            callback.ConcurrentSuspend(this, routine);
                            break;
                        case ValueCoroutineState.Continue:
                            ValueYieldInstruction instruction = routine.ConcurrentNext(state, mode);
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
                                    onWaitSeconds.ConcurrentAdd((instruction.Float + Time.time, routine));
                                    break;
                                case ValueYieldInstruction.Type.ForRealtimeSeconds:
                                    onWaitRealtimeSeconds.ConcurrentAdd((instruction.Float + Time.realtimeSinceStartup, routine));
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
                                    manager.ConcurrentStart(new NestedEnumeratorBackground<T, ValueCoroutineEnumerator<IEnumerator<ValueYieldInstruction>>, U>(this, routine, new ValueCoroutineEnumerator<IEnumerator<ValueYieldInstruction>>(instruction.ValueEnumerator), callback, state, mode));
                                    break;
                                case ValueYieldInstruction.Type.UnityEnumerator:
                                    manager.ConcurrentStart(new NestedEnumerator<T, UnityCoroutineEnumerator, U>(this, routine, new UnityCoroutineEnumerator(instruction.UnityEnumerator), callback));
                                    break;
                                case ValueYieldInstruction.Type.ValueCoroutine:
                                    onValueCoroutine.ConcurrentAdd((instruction.ValueCoroutine, routine));
                                    break;
                                case ValueYieldInstruction.Type.UnityCoroutine:
                                    onUnityCoroutine.Add((instruction.UnityCoroutine, routine));
                                    break;
                                case ValueYieldInstruction.Type.ToUnity:
#if DEBUG
                                    Debug.LogWarning($"{nameof(Yield)}.{nameof(Yield.ToUnity)} was yielded and it allocates memory. Alternatively you could use other methods such as {nameof(Yield)}.{nameof(Yield.ToUpdate)} which doesn't allocate and has a similar effect.");
#endif
                                    // TODO: Allocations can be reduced.
                                    UnityThread.RunLater(() => Next(routine, callback));
                                    break;
                                case ValueYieldInstruction.Type.ToBackground:
#if DEBUG
                                    Debug.Assert(Application.platform != RuntimePlatform.WebGLPlayer);
#endif
                                    if (mode == ThreadMode.Short)
                                    {
#if DEBUG
                                        Debug.LogWarning($"{nameof(Yield)}.{nameof(Yield.ToBackground)} was yielded from a background thread. This will be ignored.");
#endif
                                        goto start;
                                    }
                                    else
                                        Task.Factory.StartNew(shortBackground, (this, state, routine));
                                    break;
                                case ValueYieldInstruction.Type.ToLongBackground:
#if DEBUG
                                    Debug.Assert(Application.platform != RuntimePlatform.WebGLPlayer);
#endif
                                    if (mode == ThreadMode.Long)
                                    {
#if DEBUG
                                        Debug.LogWarning($"{nameof(Yield)}.{nameof(Yield.ToBackground)} was yielded from a long background thread. This will be ignored.");
#endif
                                        goto start;
                                    }
                                    else
                                        Task.Factory.StartNew(longBackground, (this, state, routine), TaskCreationOptions.LongRunning);
                                    break;
                                case ValueYieldInstruction.Type.YieldInstruction:
                                {
                                    onUnityCoroutine.ConcurrentAdd((manager.StartUnityCoroutine(Work(instruction.YieldInstruction)), routine));
                                    break;
                                    IEnumerator Work(YieldInstruction yieldInstruction)
                                    {
                                        while (true)
                                        {
                                            switch (state.State)
                                            {
                                                case ValueCoroutineState.Continue:
                                                    switch (routine.State)
                                                    {
                                                        case ValueCoroutineState.Continue:
                                                            yield return yieldInstruction;
                                                            while (true)
                                                            {
                                                                switch (state.State)
                                                                {
                                                                    case ValueCoroutineState.Continue:
                                                                        switch (routine.State)
                                                                        {
                                                                            case ValueCoroutineState.Continue:
                                                                                NextBackground(routine, callback, state, mode);
                                                                                yield break;
                                                                            case ValueCoroutineState.Suspended:
                                                                                yield return null;
                                                                                break;
                                                                            case ValueCoroutineState.Finalized:
                                                                                routine.Dispose();
                                                                                yield break;
                                                                        }
                                                                        break;
                                                                    case ValueCoroutineState.Suspended:
                                                                        yield return null;
                                                                        break;
                                                                    case ValueCoroutineState.Finalized:
                                                                        routine.Dispose();
                                                                        yield break;
                                                                }
                                                            }
                                                        case ValueCoroutineState.Suspended:
                                                            yield return null;
                                                            break;
                                                        case ValueCoroutineState.Finalized:
                                                            routine.Dispose();
                                                            yield break;
                                                    }
                                                    yield break;
                                                case ValueCoroutineState.Suspended:
                                                    yield return null;
                                                    break;
                                                case ValueCoroutineState.Finalized:
                                                    routine.Dispose();
                                                    yield break;
                                            }
                                        }
                                    }
                                }
                                case ValueYieldInstruction.Type.UnityPoll:
                                    onUnityPoll.ConcurrentEnqueue(routine);
                                    break;
                                case ValueYieldInstruction.Type.Finalized:
                                    routine.Dispose();
                                    break;
                                case ValueYieldInstruction.Type.Suspended:
                                    callback.ConcurrentSuspend(this, routine);
                                    return;
                            }
                            break;
                    }
                }

                public override void Free()
                {
                    onUpdate.Dispose();
                    onFixedUpdate.Dispose();
                    onLateUpdate.Dispose();
                    onEndOfFrame.Dispose();
                    onUnityPoll.Dispose();
                    onCustom.Dispose();
                    onWhile.Dispose();
                    onUntil.Dispose();
                    onTask.Dispose();
                    onJobHandle.Dispose();
                    onWaitSeconds.Dispose();
                    onWaitRealtimeSeconds.Dispose();
                    onValueCoroutine.Dispose();
                    suspendedEntry.Dispose();

                    RawList<(Coroutine, T)> onUnityCoroutine = this.onUnityCoroutine.Swap(tmpUnityCoroutine);
                    for (int i = 0; i < onUnityCoroutine.Count; i++)
                    {
                        (Coroutine, T) tmp = onUnityCoroutine[i];
                        manager.StopUnityCoroutine(tmp.Item1);
                        tmp.Item2.Dispose();
                    }

                    onUnityCoroutine.Clear();
                    tmpUnityCoroutine = onUnityCoroutine;

                    ConcurrentBag<(Coroutine, T)> onUnityCoroutineBag = this.onUnityCoroutine.Concurrent;
                    while (onUnityCoroutineBag.TryTake(out (Coroutine, T) tmp))
                    {
                        manager.StopUnityCoroutine(tmp.Item1);
                        tmp.Item2.Dispose();
                    }

                    ConcurrentQueue<(ValueCoroutineStateBoxed, T)> suspendedBackgroundShort = this.suspendedBackgroundShort;
                    while (suspendedBackgroundShort.TryDequeue(out (ValueCoroutineStateBoxed, T) tmp))
                        tmp.Item2.Dispose();
                }
            }
        }
    }
}