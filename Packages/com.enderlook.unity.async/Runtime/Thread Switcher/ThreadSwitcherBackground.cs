using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using UnityEngine;

namespace Enderlook.Unity.Threading
{
    /// <summary>
    /// Defines a switcher to jump to a pooled task thread.
    /// </summary>
    public struct ThreadSwitcherBackground : IThreadSwitcher
    {
        // https://stackoverflow.com/a/58470597/7655838 from https://stackoverflow.com/questions/58469468/what-does-unitymainthreaddispatcher-do

        // Alternatively we may do something like https://github.com/svermeulen/Unity3dAsyncAwaitUtil/blob/master/UnityProject/Assets/Plugins/AsyncAwaitUtil/Source/WaitForBackgroundThread.cs

        private static readonly IThreadSwitcher hasSwitchedGlobal = new ThreadSwitcherBackground() { hasSwitched = true };
#if !UNITY_WEBGL
        private static readonly IThreadSwitcher hasNotSwitchedGlobal = new ThreadSwitcherBackground();
#endif

        private bool hasSwitched;

        /// <inheritdoc cref="IThreadSwitcher.GetAwaiter"/>
        public ThreadSwitcherBackground GetAwaiter() => this;

        /// <inheritdoc cref="IThreadSwitcher.IsCompleted"/>
        public bool IsCompleted {
            get {
#if UNITY_WEBGL
#if DEBUG
                Debug.LogWarning("Threading is not supported on this platform. A fallback to main thread has been used. Be warned that this may produce deadlocks very easily.");
#endif
                return true;
#else
                return !UnityThread.IsMainThread;
#endif
            }
        }

        /// <inheritdoc cref="IThreadSwitcher.GetResult"/>
        public bool GetResult() => hasSwitched;

        /// <inheritdoc cref="INotifyCompletion.OnCompleted(Action)"/>
        public void OnCompleted(Action continuation)
        {
            if (continuation is null)
                Switch.ThrowArgumentNullException_Continuation();

#if UNITY_WEBGL
            Debug.Assert(!hasSwitched);
#if DEBUG
            Debug.LogWarning("Threading is not supported on this platform. A fallback to main thread has been used. Be warned that this may produce deadlocks very easily.");
#endif
            continuation();
#else
            hasSwitched = UnityThread.IsMainThread;
            if (hasSwitched)
                Task.Factory.StartNew(continuation);
            else
            {
#if DEBUG
                Debug.Log("Already in a non-main thread, we don't need to change of thread so we will not.");
#endif
                continuation();
            }
#endif
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