using Enderlook.Collections.LowLevel;
using Enderlook.Unity.Threading;

using System;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    public sealed partial class CoroutineManager
    {
        /// <summary>
        /// Returns an aproximate number of coroutines that are waiting for poll.<br/>
        /// Since this method is not synchronized, due multithreading scenarios it may not give exact results.
        /// </summary>
        public int PollCount
        {
            get
            {
                int total = 0;
                RawList<ManagerBase> managers = GetManagersList();
                for (int i = 0; i < managers.Count; i++)
                    total += managers[i].PollCount;
                return total;
            }
        }

        /// <summary>
        /// Executes the update event of all coroutines.
        /// </summary>
        public void OnUpdate()
        {
            if (!UnityThread.IsMainThread)
                ThrowNonUnityThread();

            if (state != ValueCoroutineState.Continue)
                return;

            RawList<ManagerBase> managers = GetManagersList();
            for (int i = 0; i < managers.Count; i++)
                managers[i].OnUpdate();
        }

        /// <summary>
        /// Executes the late update event of all coroutines.
        /// </summary>
        public void OnLateUpdate()
        {
            if (!UnityThread.IsMainThread)
                ThrowNonUnityThread();

            if (state != ValueCoroutineState.Continue)
                return;

            RawList<ManagerBase> managers = GetManagersList();
            for (int i = 0; i < managers.Count; i++)
                managers[i].OnLateUpdate();
        }

        /// <summary>
        /// Executes the fixed update event of all coroutines.
        /// </summary>
        public void OnFixedUpdate()
        {
            if (!UnityThread.IsMainThread)
                ThrowNonUnityThread();

            if (state != ValueCoroutineState.Continue)
                return;

            RawList<ManagerBase> managers = GetManagersList();
            for (int i = 0; i < managers.Count; i++)
                managers[i].OnFixedUpdate();
        }

        /// <summary>
        /// Executes the end of frame event of all coroutines.
        /// </summary>
        public void OnEndOfFrame()
        {
            if (!UnityThread.IsMainThread)
                ThrowNonUnityThread();

            if (state != ValueCoroutineState.Continue)
                return;

            RawList<ManagerBase> managers = GetManagersList();
            for (int i = 0; i < managers.Count; i++)
                managers[i].OnEndOfFrame();
        }

        /// <summary>
        /// Executes the poll event of all coroutines.
        /// </summary>
        public void OnPoll()
        {
            if (!UnityThread.IsMainThread)
                ThrowNonUnityThread();

            if (state != ValueCoroutineState.Continue)
                return;

            int until = DateTime.Now.Millisecond + milisecondsExecutedPerFrameOnPoll;
            RawList<ManagerBase> managersList = GetManagersList();
            for (int i = 0; i < managersList.Count; i++)
                managersList[i].OnPoll(until, true);

            while (true)
            {
                // GetManagersList() is inside the loop because new managers may be added during the iterations.
                managersList = GetManagersList();
                if (managersList.Count == 0)
                    break;

                for (int i = 0; i < managersList.Count; i++)
                {
                    if (DateTime.Now.Millisecond > until)
                        break;
                    if (managersList[i].PollCount > 0)
                        goto next;
                }
                break;

            next:;
                for (int i = 0; i < managersList.Count; i++)
                {
                    if (DateTime.Now.Millisecond > until)
                        break;
                    managersList[i].OnPoll(until, false);
                }
            }
        }

        private RawList<ManagerBase> GetManagersList()
        {
            managerLock.ReadBegin();
            // Underlying arrays are not cleaned nor reaused during resize, nor coroutines implement auto-cleaning, so this is safe.
            RawList<ManagerBase> managers = managersList;
            managerLock.ReadEnd();
            return managers;
        }
    }
}