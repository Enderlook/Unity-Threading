using System;
using System.Runtime.CompilerServices;

using UnityEngine;

namespace Enderlook.Unity.Threading
{
    /// <summary>
    /// Defines a switcher to jump to the Unity thread.
    /// </summary>
    public struct ThreadSwitcherUnity : IThreadSwitcher
    {
        // https://stackoverflow.com/a/58470597/7655838 from https://stackoverflow.com/questions/58469468/what-does-unitymainthreaddispatcher-do

        private static readonly IThreadSwitcher hasSwitchedGlobal = new ThreadSwitcherUnity() { hasSwitched = true };
#if !UNITY_WEBGL
        private static readonly IThreadSwitcher hasNotSwitchedGlobal = new ThreadSwitcherUnity();
#endif

        private bool hasSwitched;

        /// <inheritdoc cref="IThreadSwitcher.GetAwaiter"/>
        public ThreadSwitcherUnity GetAwaiter() => this;

        /// <inheritdoc cref="IThreadSwitcher.IsCompleted"/>
        public bool IsCompleted => UnityThread.IsMainThread;

        /// <inheritdoc cref="IThreadSwitcher.GetResult"/>
        public bool GetResult() => hasSwitched;

        /// <inheritdoc cref="INotifyCompletion.OnCompleted(Action)"/>
        public void OnCompleted(Action continuation)
        {
            if (continuation is null)
                Switch.ThrowArgumentNullException_Continuation();

            hasSwitched = !UnityThread.IsMainThread;
            if (!hasSwitched)
            {
#if DEBUG
                Debug.Log("Already in main thread, this will do nothing.");
#endif
                continuation();
            }
            else
                UnityThread.RunLater(continuation);
        }

        /// <inheritdoc cref="IThreadSwitcher.GetAwaiter"/>
        IThreadSwitcher IThreadSwitcher.GetAwaiter()
        {
#if UNITY_WEBGL
            return hasSwitchedGlobal;
#else
            return hasSwitched ? hasSwitchedGlobal : hasNotSwitchedGlobal;
#endif
        }
    }
}
