using System.Threading;

using UnityEditor;

using UnityEngine;

namespace Assets.Enderlook.Unity.Threading
{
    /// <summary>
    /// A helper class to gather the thread id and synchronization context of Unity main thread.
    /// </summary>
    public static class UnitySynchronizationContextUtility
    {
        /// <summary>
        /// Synchronization context used by Unity.
        /// </summary>
        public static SynchronizationContext UnitySynchronizationContext { get; private set; }

        /// <summary>
        /// Thread Id used by Unity main thread.
        /// </summary>
        public static int UnityThreadId { get; private set; }

        // https://github.com/svermeulen/Unity3dAsyncAwaitUtil/blob/master/UnityProject/Assets/Plugins/AsyncAwaitUtil/Source/Internal/SyncContextUtil.cs

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            UnitySynchronizationContext = SynchronizationContext.Current;
            UnityThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        /// <summary>
        /// Determines if we are in the unity synchronization context.
        /// </summary>
        public static bool IsInUnitySynchronizationContext => SynchronizationContext.Current == UnitySynchronizationContext;

        /// <summary>
        /// Determines if we are in the unity thread.
        /// </summary>
        public static bool IsInUnityThread => Thread.CurrentThread.ManagedThreadId == UnityThreadId;
    }
}
