namespace Enderlook.Unity.Threading
{
    /// <summary>
    /// A helper class which allows to switch to a particular thread.
    /// </summary>
    public static class ThreadSwitcher
    {
        // https://stackoverflow.com/a/58470597/7655838 from https://stackoverflow.com/questions/58469468/what-does-unitymainthreaddispatcher-do

        /// <summary>
        /// Switches to a background thread.
        /// </summary>
        /// <returns>Object which switched to the thread.</returns>
        public static ThreadSwitcherBackground ResumeBackgroundAsync => new ThreadSwitcherBackground();

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
    }
}
