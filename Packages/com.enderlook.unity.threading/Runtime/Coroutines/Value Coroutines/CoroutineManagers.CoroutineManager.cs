using Enderlook.Collections.LowLevel;
using Enderlook.Threading;
using Enderlook.Unity.Threading;

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Unity.Jobs;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    internal static partial class CoroutineManagers
    {
        internal static partial class CoroutineManager<T, U>
            where T : IEnumerator<ValueYieldInstruction>
            where U : ICancellable
        {
            private const byte UnknownThread = 0;
            private const byte ShortThread = 1;
            private const byte LongThread = 2;

            private readonly static MonoBehaviour monoBehaviour;
            private readonly static Action<Routine> nextShortBackgroundAction;
            private readonly static Action<Routine> nextLongBackgroundAction;

            // TODO: Cache locality may be improved by using a circular buffer instead of two lists when iterating each one. Futher reasearch is required.

            private static RawList<Routine> onUpdate = RawList<Routine>.Create();
            private static RawList<Routine> onLateUpdate = RawList<Routine>.Create();
            private static RawList<Routine> onFixedUpdate = RawList<Routine>.Create();
            private static RawList<Routine> onEndOfFrame = RawList<Routine>.Create();
            private static RawQueue<Routine> onPoll = RawQueue<Routine>.Create();
            private static RawList<(CustomYieldInstruction, Routine)> onCustom = RawList<(CustomYieldInstruction, Routine)>.Create();
            private static RawList<(Func<bool>, Routine)> onWhile = RawList<(Func<bool>, Routine)>.Create();
            private static RawList<(Func<bool>, Routine)> onUntil = RawList<(Func<bool>, Routine)>.Create();
            private static RawList<(ValueTask, Routine)> onTask = RawList<(ValueTask, Routine)>.Create();
            private static RawList<(JobHandle, Routine)> onJobHandle = RawList<(JobHandle, Routine)>.Create();
            // Waiter with timer can be reduced in time complexity by using priority queues.
            private static RawList<(float, Routine)> onTime = RawList<(float, Routine)>.Create();
            private static RawList<(float, Routine)> onRealtime = RawList<(float, Routine)>.Create();
            private static RawList<(ValueCoroutine, Routine)> onValueCoroutine = RawList<(ValueCoroutine, Routine)>.Create();

            private static RawList<(U, UnityEngine.Coroutine)> onUnityCoroutine = RawList<(U, UnityEngine.Coroutine)>.Create();

            private static RawList<Routine> tmpT = RawList<Routine>.Create();
            private static RawQueue<Routine> tmpTQueue = RawQueue<Routine>.Create();
            private static RawList<(CustomYieldInstruction, Routine)> tmpCustom = RawList<(CustomYieldInstruction, Routine)>.Create();
            private static RawList<(float, Routine)> tmpFloat = RawList<(float, Routine)>.Create();
            private static RawList<(Func<bool>, Routine)> tmpFuncBool = RawList<(Func<bool>, Routine)>.Create();
            private static RawList<(ValueTask, Routine)> tmpTask = RawList<(ValueTask, Routine)>.Create();
            private static RawList<(JobHandle, Routine)> tmpJobHandle = RawList<(JobHandle, Routine)>.Create();
            private static RawList<(ValueCoroutine, Routine)> tmpValueCoroutine = RawList<(ValueCoroutine, Routine)>.Create();

            private static ConcurrentBag<Routine> onUpdateBag;
            private static ConcurrentBag<Routine> onLateUpdateBag;
            private static ConcurrentBag<Routine> onFixedUpdateBag;
            private static ConcurrentBag<Routine> onEndOfFrameBag;
            private static ConcurrentQueue<Routine> onPollQueue;
            private static ConcurrentBag<(CustomYieldInstruction, Routine)> onCustomBag;
            private static ConcurrentBag<(Func<bool>, Routine)> onWhileBag;
            private static ConcurrentBag<(Func<bool>, Routine)> onUntilBag;
            private static ConcurrentBag<(ValueTask, Routine)> onTaskBag;
            private static ConcurrentBag<(JobHandle, Routine)> onJobHandleBag;
            private static ConcurrentBag<(float, Routine)> onTimeBag;
            private static ConcurrentBag<(float, Routine)> onRealtimeBag;
            private static ConcurrentBag<(U, UnityEngine.Coroutine)> onUnityCoroutineBag;
            private static ConcurrentBag<(ValueCoroutine, Routine)> onValueCoroutineBag;

            static CoroutineManager()
            {
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
                    onCustomBag = new ConcurrentBag<(CustomYieldInstruction, Routine)>();
                    onWhileBag = new ConcurrentBag<(Func<bool>, Routine)>();
                    onUntilBag = new ConcurrentBag<(Func<bool>, Routine)>();
                    onTaskBag = new ConcurrentBag<(ValueTask, Routine)>();
                    onJobHandleBag = new ConcurrentBag<(JobHandle, Routine)>();
                    onTimeBag = new ConcurrentBag<(float, Routine)>();
                    onRealtimeBag = new ConcurrentBag<(float, Routine)>();
                    onUnityCoroutineBag = new ConcurrentBag<(U, UnityEngine.Coroutine)>();
                    onValueCoroutineBag = new ConcurrentBag<(ValueCoroutine, Routine)>();
                }

                CoroutineManagers.onUpdate += OnUpdate;
                CoroutineManagers.onLateUpdate += OnLateUpdate;
                CoroutineManagers.onFixedUpdate += OnFixedUpdate;
                CoroutineManagers.onEndOfFrame += OnEndOfFrame;

                while (Interlocked.Exchange(ref onPollKey, 1) != 0) ;
                CoroutineManagers.onPoll.Add(OnPoll);
                Interlocked.Exchange(ref onPollKey, 0);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Start(U cancellator, T coroutine)
                => Next(new Routine(cancellator, coroutine));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void StartThreadSafe(U cancellator, T coroutine)
                => StartThreadSafe(cancellator, coroutine, UnknownThread);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static void StartThreadSafe(U cancellator, T coroutine, int mode)
                => NextBackground(new Routine(cancellator, coroutine), mode);

            public static void OnUpdate()
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

            public static void OnLateUpdate()
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

            public static void OnFixedUpdate()
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

            public static void OnEndOfFrame()
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

            public static bool OnPoll(int until)
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
                    if (DateTime.Now.Millisecond >= until && i / total >= .01f)
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

            private static void OnWaitForSeconds()
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

            private static void OnWaitForRealtimeSeconds()
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

            private static void OnCustom()
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

            private static void OnWhile()
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

            private static void OnUntil()
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

            private static void OnTask()
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

            private static void OnJobHandle()
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

            private static void OnValueCoroutine()
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

            private static void OnOthers()
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

            private static void CheckUnityCoroutines()
            {
                for (int i = onUnityCoroutine.Count - 1; i >= 0; i--)
                {
                    (U condition, UnityEngine.Coroutine coroutine) tmp = onUnityCoroutine[i];
                    if (tmp.condition.IsCancelationRequested)
                    {
                        monoBehaviour.StopCoroutine(tmp.coroutine);
                        onUnityCoroutine.RemoveAt(i);
                    }
                }

                ConcurrentBag<(U condition, UnityEngine.Coroutine coroutine)> bag = onUnityCoroutineBag;
                if (!(bag is null))
                {
                    while (bag.TryTake(out (U condition, UnityEngine.Coroutine coroutine) tmp))
                    {
                        if (tmp.condition.IsCancelationRequested)
                            monoBehaviour.StopCoroutine(tmp.coroutine);
                        else
                            onUnityCoroutine.Add(tmp);
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Next(Routine routine)
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
                            CoroutineManager<NestedEnumerator<T, U>, U>.Start(routine.cancellator, new NestedEnumerator<T, U>(routine, instruction.ValueEnumerator));
                            break;
                        case ValueYieldInstruction.Type.BoxedEnumerator:
                        {
                            onUnityCoroutine.Add((routine.cancellator, monoBehaviour.StartCoroutine(Work())));
                            break;
                            IEnumerator Work()
                            {
                                yield return instruction.BoxedEnumerator;
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
                            onUnityCoroutine.Add((routine.cancellator, monoBehaviour.StartCoroutine(Work())));
                            break;
                            IEnumerator Work()
                            {
                                yield return instruction.YieldInstruction;
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
            public static void NextBackground(Routine routine, int mode)
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
                            CoroutineManager<NestedEnumeratorBackground<T, U>, U>.StartThreadSafe(routine.cancellator, new NestedEnumeratorBackground<T, U>(routine, instruction.ValueEnumerator, mode), mode);
                            break;
                        case ValueYieldInstruction.Type.BoxedEnumerator:
                        {
                            onUnityCoroutineBag.Add((routine.cancellator, monoBehaviour.StartCoroutine(Work())));
                            break;
                            IEnumerator Work()
                            {
                                yield return instruction.BoxedEnumerator;
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
                            onUnityCoroutineBag.Add((routine.cancellator, monoBehaviour.StartCoroutine(Work())));
                            break;
                            IEnumerator Work()
                            {
                                yield return instruction.YieldInstruction;
                                Next(routine);
                            }
                        }
                    }
                }
            }
        }
    }
}