using Enderlook.Unity.Threading.Tasks;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Enderlook.Unity.Threading
{
    /// <summary>
    /// A helper class to execute actions on the Unity main thread.
    /// </summary>
    public static class UnityThread
    {
        private static readonly SendOrPostCallback callback = (obj) => ((Action)obj)();

        /// <summary>
        /// Synchronization context used by Unity.
        /// </summary>
        public static readonly SynchronizationContext UnitySynchronizationContext = UnitySynchronizationContextUtility.UnitySynchronizationContext;

        /// <summary>
        /// Thread Id used by Unity main thread.
        /// </summary>
        public static readonly int UnityThreadId = UnitySynchronizationContextUtility.UnityThreadId;

        /// <summary>
        /// A <see cref="TaskFactory"/> where all tasks are run on the main thread.
        /// </summary>
        public static readonly TaskFactory Factory = new TaskFactory(UnityThreadTaskScheduler.Instance);

        /// <summary>
        /// Check if we are currently running in main (Unity) thread.
        /// </summary>
        /// <returns>Whenever we are running in main thread or not.</returns>
        public static bool IsMainThread => Thread.CurrentThread.ManagedThreadId == UnityThreadId;

        /// <summary>
        /// Determines if we are in the Unity synchronization context.
        /// </summary>
        public static bool IsUnitySynchronizationContext => SynchronizationContext.Current == UnitySynchronizationContext;

        /// <summary>
        /// Executes the specified action on the Unity thread.<br/>
        /// The action will not be executed instantaneously, but later.
        /// </summary>
        /// <param name="action">Action to execute on the main thread.</param>
        public static void RunLater(Action action)
            => UnitySynchronizationContextUtility.UnitySynchronizationContext.Post(callback, action);

        /// <summary>
        /// Executes the specified action on the Unity thread.<br/>
        /// The action will not be executed instantaneously, but later.
        /// </summary>
        /// <param name="action">Action to execute on the main thread.</param>
        /// <param name="state">State of the action.</param>
        public static void RunLater(SendOrPostCallback action, object state)
            => UnitySynchronizationContextUtility.UnitySynchronizationContext.Post(action, state);

        /// <summary>
        /// Executes the specified action on the Unity thread.<br/>
        /// The action will be immediately and this thread will wait until completition.
        /// </summary>
        /// <param name="action">Action to execute on the main thread.</param>
        public static void RunNow(Action action)
            => UnitySynchronizationContextUtility.UnitySynchronizationContext.Send(callback, action);

        /// <summary>
        /// Executes the specified action on the Unity thread.<br/>
        /// The action will be immediately and this thread will wait until completition.
        /// </summary>
        /// <param name="action">Action to execute on the main thread.</param>
        /// <param name="state">State of the action.</param>
        public static void RunNow(SendOrPostCallback action, object state)
            => UnitySynchronizationContextUtility.UnitySynchronizationContext.Send(action, state);
    }
}
