using System;

namespace Enderlook.Unity.Threading
{
    /// <summary>
    /// A helper class which allows to switch to a particular thread.
    /// </summary>
    public static class Switch
    {
        // https://stackoverflow.com/a/58470597/7655838 from https://stackoverflow.com/questions/58469468/what-does-unitymainthreaddispatcher-do

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

#if UNITY_EDITOR
        /// <summary>
        /// Switches to a background pool thread using the editor preferences instead of the standalone platform.<br/>
        /// This API only exists inside the Unity Editor.
        /// </summary>
        /// <returns>Object which switched to the thread.</returns>
        public static ThreadSwitcherBackground ToBackgroundEditor => new ThreadSwitcherBackground(true);

        /// <summary>
        /// Switches to a background long duration thread using the editor preferences instead of the standalone platform.<br/>
        /// This API only exists inside the Unity Editor.
        /// </summary>
        /// <returns>Object which switched to the thread.</returns>
        public static ThreadSwitcherLongBackground ToLongBackgroundEditor => new ThreadSwitcherLongBackground(true);
#endif

        /// <summary>
        /// Switch to the Unity thread.
        /// </summary>
        /// <returns>Object which switched to the thread.</returns>
        public static ThreadSwitcherUnity ToUnity => new ThreadSwitcherUnity();

        internal static void ThrowArgumentNullException_Continuation()
            => throw new ArgumentNullException("continuation");
    }
}
