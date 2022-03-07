using System;
using System.Threading;
using System.Threading.Tasks;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Threading
{
    /// <summary>
    /// A helper class to gather the thread id and synchronization context of Unity main thread.
    /// </summary>
    internal static class UnitySynchronizationContextUtility
    {
        /// <summary>
        /// Synchronization context used by Unity.
        /// </summary>
        public static SynchronizationContext UnitySynchronizationContext
        {
            get
            {
                if (!wasInitialized)
                    throw new InvalidOperationException("Can't get Unity Synchronization Context before Unity initializes the syncronization.");
                return unitySynchronizationContext;
            }
            private set => unitySynchronizationContext = value;
        }

        /// <summary>
        /// Thread Id used by Unity main thread.
        /// </summary>
        public static int UnityThreadId
        {
            get
            {
                if (!wasInitialized)
                    throw new InvalidOperationException("Can't get Unity Synchronization Context before Unity initializes the syncronization.");
                return unityThreadId;
            }
            private set => unityThreadId = value;
        }

        /// <summary>
        /// Task Scheduler used by Unity.
        /// </summary>
        public static TaskScheduler UnityTaskScheduler
        {
            get
            {
                if (!wasInitialized)
                    throw new InvalidOperationException("Can't get Unity Synchronization Context before Unity initializes the syncronization.");
                return unityTaskScheduler;
            }
            private set => unityTaskScheduler = value;
        }

        // https://github.com/svermeulen/Unity3dAsyncAwaitUtil/blob/master/UnityProject/Assets/Plugins/AsyncAwaitUtil/Source/Internal/SyncContextUtil.cs

        private static bool wasInitialized;
        private static SynchronizationContext unitySynchronizationContext;
        private static int unityThreadId;
        private static TaskScheduler unityTaskScheduler;

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize()
        {
            if (wasInitialized)
                return;
            wasInitialized = true;
            UnitySynchronizationContext = SynchronizationContext.Current;
            UnityThreadId = Thread.CurrentThread.ManagedThreadId;
            UnityTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
        }
    }
}