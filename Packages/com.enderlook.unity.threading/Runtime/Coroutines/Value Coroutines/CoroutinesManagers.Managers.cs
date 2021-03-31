using Enderlook.Collections.LowLevel;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    public partial struct CoroutinesManager
    {
        internal partial class Managers
        {
            private const byte UnknownThread = 0;
            private const byte ShortThread = 1;
            private const byte LongThread = 2;

            private MonoBehaviour monoBehaviour;
            private RawList<(Type enumerator, Type cancellator, ManagerBase manager)> toAdd;
            private Dictionary<(Type enumerator, Type cancellator), ManagerBase> managers;
            private ReadWriterLock managerLock;
            private int toAddLock;
            private int disposing;

            public Managers()
            {
                managers = new Dictionary<(Type enumerator, Type cancellator), ManagerBase>();
                toAdd = RawList<(Type enumerator, Type cancellator, ManagerBase manager)>.Create();
            }

            public void SetMonoBehaviour(MonoBehaviour monoBehaviour)
            {
                if (monoBehaviour is null)
                    throw new ArgumentNullException(nameof(monoBehaviour));

                if (this.monoBehaviour is null)
                    this.monoBehaviour = monoBehaviour;
                else
                    throw new InvalidOperationException($"Already has set a {nameof(MonoBehaviour)}.");
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Start<T, U>(U cancellator, T routine)
                where T : IEnumerator<ValueYieldInstruction>
                where U : ICancellable
            {
                Type enumerator_ = typeof(T);
                Type cancellator_ = typeof(U);

                managerLock.ReadBegin();

                if (managers.TryGetValue((enumerator_, cancellator_), out ManagerBase manager))
                {
                    managerLock.ReadEnd();
                    ((TypedManager<T, U>)manager).Start(cancellator, routine);
                }
                else
                {
                    managerLock.ReadEnd();
                    TypedManager<T, U> manager_ = new TypedManager<T, U>(this);
                    while (Interlocked.Exchange(ref toAddLock, 1) != 0) ;
                    toAdd.Add((enumerator_, cancellator_, manager_));
                    toAddLock = 0;
                    manager_.Start(cancellator, routine);
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

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void StartThreadSafe<T, U>(U cancellator, T routine, int mode)
                where T : IEnumerator<ValueYieldInstruction>
                where U : ICancellable
            {
                Type enumerator_ = typeof(T);
                Type cancellator_ = typeof(U);

                managerLock.ReadBegin();

                if (managers.TryGetValue((enumerator_, cancellator_), out ManagerBase manager))
                {
                    managerLock.ReadEnd();
                    ((TypedManager<T, U>)manager).StartThreadSafe(cancellator, routine, mode);
                }
                else
                {
                    managerLock.ReadEnd();
                    TypedManager<T, U> manager_ = new TypedManager<T, U>(this);
                    while (Interlocked.Exchange(ref toAddLock, 1) != 0) ;
                    toAdd.Add((enumerator_, cancellator_, manager_));
                    toAddLock = 0;
                    manager_.StartThreadSafe(cancellator, routine, mode);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void AddManagers()
            {
                LockAll();
                AddManagersInner();
                UnlockAll();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void AddManagersInner()
            {
                RawList<(Type enumerator, Type cancellator, ManagerBase)> toAdd = this.toAdd;
                for (int i = 0; i < toAdd.Count; i++)
                {
                    (Type enumerator, Type cancellator, ManagerBase manager) tmp = toAdd[i];
                    managers.Add((tmp.enumerator, tmp.cancellator), tmp.manager);
                }
                this.toAdd.Clear();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void LockAll()
            {
                while (Interlocked.Exchange(ref toAddLock, 1) != 0) ;
                managerLock.WriteBegin();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void UnlockAll()
            {
                managerLock.WriteEnd();
                toAddLock = 0;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void OnUpdate()
            {
                AddManagers();
                // This read may not be actually needed, since the only method that writes is AddManagers().
                managerLock.ReadBegin();
                foreach (ManagerBase manager in managers.Values)
                    manager.OnUpdate();
                managerLock.WriteEnd();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void OnLateUpdate()
            {
                AddManagers();
                // This read may not be actually needed, since the only method that writes is AddManagers().
                managerLock.ReadBegin();
                foreach (ManagerBase manager in managers.Values)
                    manager.OnLateUpdate();
                managerLock.WriteEnd();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void OnFixedUpdate()
            {
                AddManagers();
                // This read may not be actually needed, since the only method that writes is AddManagers().
                managerLock.ReadBegin();
                foreach (ManagerBase manager in managers.Values)
                    manager.OnFixedUpdate();
                managerLock.WriteEnd();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void OnEndOfFrame()
            {
                AddManagers();
                // This read may not be actually needed, since the only method that writes is AddManagers().
                managerLock.ReadBegin();
                foreach (ManagerBase manager in managers.Values)
                    manager.OnEndOfFrame();
                managerLock.WriteEnd();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void OnPoll(int milisecondsExecuted, float percentOfExecutions)
            {
                if (milisecondsExecuted < 0)
                    ThrowOutOfRangeMilisecondsExecuted();
                if (percentOfExecutions < 0 || percentOfExecutions > 1)
                    ThrowOutOfRangePercentExecutions();

                AddManagers();
                int until = DateTime.Now.Millisecond + milisecondsExecuted;
                bool work;
                // This read may not be actually needed, since the only method that writes is AddManagers(). 
                managerLock.ReadBegin();
                do
                {
                    work = false;
                    foreach (ManagerBase manager in managers.Values)
                        work |= manager.OnPoll(until, percentOfExecutions);
                } while (until > DateTime.Now.Millisecond && work);
                managerLock.ReadEnd();
            }

            public void Free()
            {
                LockAll();
                AddManagersInner();
                foreach (ManagerBase manager in managers.Values)
                    manager.Free();
                monoBehaviour = null;
                UnlockAll();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static void ThrowOutOfRangePercentExecutions() => throw new ArgumentOutOfRangeException("percentOfExecutions", "Must be from 0 to 1.");

            [MethodImpl(MethodImplOptions.NoInlining)]
            private static void ThrowOutOfRangeMilisecondsExecuted() => throw new ArgumentOutOfRangeException("milisecondsExecuted", "Can't be negative.");

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void StopUnityCoroutine(UnityEngine.Coroutine coroutine)
                => monoBehaviour.StopCoroutine(coroutine);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private UnityEngine.Coroutine StartUnityCoroutine(IEnumerator coroutine)
                => monoBehaviour.StartCoroutine(coroutine);
        }
    }
}