using Enderlook.Collections.LowLevel;
using Enderlook.Unity.Threading;

using System;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    public sealed partial class CoroutineManager
    {
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

            int total = 0;
            {
                managerLock.ReadBegin();
                RawList<ManagerBase> managers = managersList;
                for (int i = 0; i < managers.Count; i++)
                    total += managers[i].PollCount();
                managerLock.ReadEnd();
            }

            /* TODO: `total` can be lower than the real value (even it can be negative) 
            * since users can queue to remove multiple times the same task (or tasks that doesn't exists)
            * but currently `EventsQueue` is just substracting the removal queue from `total`. */

            int to = Mathf.CeilToInt(total * minimumPercentOfExecutionsPerFrameOnPoll);
            int index = 0;
            int until = DateTime.Now.Millisecond + milisecondsExecutedPerFrameOnPoll;

            while (true)
            {
                managerLock.ReadBegin();
                RawList<ManagerBase> managersList = this.managersList;
                managerLock.ReadEnd();
                int old = poolIndex;
                if (old < managersList.Count)
                {
                    poolIndex = old + 1;
                    if (!managersList[old].OnPoll(until, ref index, to))
                        break;
                }
                else
                    index = 0;
                break;
            }

            /* TODO: Since users can remove tasks from poll, `i` may actually never reach total
            * That won't be a deadlock but it will burn a lot of CPU until the timeout is reached. */
        }

        /// <summary>
        /// Executes the background event of all coroutines.<br/>
        /// It's not required to call this method on platforms which doesn't support multithreading.
        /// </summary>
        public void OnBackground()
        {
#if DEBUG
            if (UnityThread.IsMainThread)
                Debug.LogWarning("You are executing this function in the main thread. It's not an error... thought it doesn't make much sense. This message will not shown on release.");
#endif

            if (state != ValueCoroutineState.Continue)
                return;

#if !UNITY_WEBGL
            RawList<ManagerBase> managers = GetManagersList();
            for (int i = 0; i < managers.Count; i++)
                managers[i].OnBackground();
#endif
        }

        private RawList<ManagerBase> GetManagersList()
        {
            managerLock.ReadBegin();
            RawList<ManagerBase> managers = managersList;
            managerLock.ReadEnd();
            return managers;
        }
    }
}