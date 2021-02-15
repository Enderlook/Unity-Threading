﻿using System;
using System.Threading;

namespace Enderlook.Unity.Threading
{
    /// <summary>
    /// A helper class which allows to switch to a particular thread.
    /// </summary>
    public static class ThreadSwitcher
    {
        // https://stackoverflow.com/a/58470597/7655838 from https://stackoverflow.com/questions/58469468/what-does-unitymainthreaddispatcher-do

        internal static readonly SendOrPostCallback callback = (obj) => ((Action)obj)();

        /// <summary>
        /// Switches to a background pool thread.
        /// </summary>
        /// <returns>Object which switched to the thread.</returns>
        public static ThreadSwitcherBackground ResumeBackgroundAsync => new ThreadSwitcherBackground();

        /// <summary>
        /// Switches to a background long duration thread.
        /// </summary>
        /// <returns>Object which switched to the thread.</returns>
        public static ThreadSwitcherLongBackground ResumeLongBackgroundAsync => new ThreadSwitcherLongBackground();

        /// <summary>
        /// Switch to the Unity thread.
        /// </summary>
        /// <returns>Object which switched to the thread.</returns>
        public static ThreadSwitcherUnity ResumeUnityAsync => new ThreadSwitcherUnity();

        /// <summary>
        /// Check if we are currently running in main thread.
        /// </summary>
        /// <returns>Whenever we are running in main thread or not.</returns>
        public static bool IsExecutingMainThread => UnitySynchronizationContextUtility.IsInUnitySynchronizationContext;

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
        /// The action will be immediately.
        /// </summary>
        /// <param name="action">Action to execute on the main thread.</param>
        public static void OnUnityNow(Action action)
            => UnitySynchronizationContextUtility.UnitySynchronizationContext.Send(callback, action);

        /// <summary>
        /// Executes the specified action on the Unity thread.<br/>
        /// The action will be immediately.
        /// </summary>
        /// <param name="action">Action to execute on the main thread.</param>
        /// <param name="state">State of the action.</param>
        public static void OnUnityNow(SendOrPostCallback action, object state)
            => UnitySynchronizationContextUtility.UnitySynchronizationContext.Send(action, state);
    }
}
