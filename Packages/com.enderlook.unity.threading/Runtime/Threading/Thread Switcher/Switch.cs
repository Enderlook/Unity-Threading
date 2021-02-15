using Enderlook.Unity.Threading.Tasks;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Enderlook.Unity.Threading
{
    /// <summary>
    /// A helper class which allows to switch to a particular thread.
    /// </summary>
    public static class Switch
    {
        // https://stackoverflow.com/a/58470597/7655838 from https://stackoverflow.com/questions/58469468/what-does-unitymainthreaddispatcher-do

        internal static readonly SendOrPostCallback callback = (obj) => ((Action)obj)();

        /// <summary>
        /// A <see cref="TaskFactory"/> where all tasks are run on the main thread.
        /// </summary>
        public static readonly TaskFactory OnUnity = new TaskFactory(UnityThreadTaskScheduler.Instance);

        /// <summary>
        /// Switches to a background pool thread.
        /// </summary>
        /// <returns>Object which switched to the thread.</returns>
        public static ThreadSwitcherBackground ToBackground => new ThreadSwitcherBackground();

        /// <summary>
        /// Switches to a background long duration thread.
        /// </summary>
        /// <returns>Object which switched to the thread.</returns>
        public static ThreadSwitcherLongBackground ToLongBackground => new ThreadSwitcherLongBackground();

        /// <summary>
        /// Switch to the Unity thread.
        /// </summary>
        /// <returns>Object which switched to the thread.</returns>
        public static ThreadSwitcherUnity ToUnity => new ThreadSwitcherUnity();

        /// <summary>
        /// Check if we are currently running in main thread.
        /// </summary>
        /// <returns>Whenever we are running in main thread or not.</returns>
        public static bool IsInMainThread => UnitySynchronizationContextUtility.IsInUnitySynchronizationContext;

        /// <summary>
        /// Executes the specified action on the Unity thread.<br/>
        /// The action will not be executed instantaneously, but later.
        /// </summary>
        /// <param name="action">Action to execute on the main thread.</param>
        public static void OnUnityLater(Action action)
            => UnitySynchronizationContextUtility.UnitySynchronizationContext.Post(callback, action);

        /// <summary>
        /// Executes the specified action on the Unity thread.<br/>
        /// The action will not be executed instantaneously, but later.
        /// </summary>
        /// <param name="action">Action to execute on the main thread.</param>
        /// <param name="state">State of the action.</param>
        public static void OnUnityLater(SendOrPostCallback action, object state)
            => UnitySynchronizationContextUtility.UnitySynchronizationContext.Post(action, state);

        /// <summary>
        /// Executes the specified action on the Unity thread.<br/>
        /// The action will be immediately and this thread will wait until completition.
        /// </summary>
        /// <param name="action">Action to execute on the main thread.</param>
        public static void OnUnityNow(Action action)
            => UnitySynchronizationContextUtility.UnitySynchronizationContext.Send(callback, action);

        /// <summary>
        /// Executes the specified action on the Unity thread.<br/>
        /// The action will be immediately and this thread will wait until completition.
        /// </summary>
        /// <param name="action">Action to execute on the main thread.</param>
        /// <param name="state">State of the action.</param>
        public static void OnUnityNow(SendOrPostCallback action, object state)
            => UnitySynchronizationContextUtility.UnitySynchronizationContext.Send(action, state);
    }
}
