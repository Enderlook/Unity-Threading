using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using UnityEngine;

namespace Enderlook.Unity.Threading
{
    /// <summary>
    /// Defines a switcher to jump to a long task thread.
    /// </summary>
    public struct ThreadSwitcherLongBackground : IThreadSwitcher
    {
        // https://stackoverflow.com/a/58470597/7655838 from https://stackoverflow.com/questions/58469468/what-does-unitymainthreaddispatcher-do

        // Alternatively we may do something like https://github.com/svermeulen/Unity3dAsyncAwaitUtil/blob/master/UnityProject/Assets/Plugins/AsyncAwaitUtil/Source/WaitForBackgroundThread.cs

        private static readonly IThreadSwitcher hasSwitchedGlobal = new ThreadSwitcherLongBackground() { hasSwitched = true };
#if !UNITY_WEBGL || UNITY_EDITOR
        private static readonly IThreadSwitcher hasNotSwitchedGlobal = new ThreadSwitcherLongBackground();
#endif

        private bool hasSwitched;

#if UNITY_EDITOR
#if UNITY_WEBGL
        private bool useEditorPreference;
#endif

        internal ThreadSwitcherLongBackground(bool useEditorPreference)
        {
#if UNITY_WEBGL
            this.useEditorPreference = useEditorPreference;
#endif
            hasSwitched = default;
        }
#endif

        /// <inheritdoc cref="IThreadSwitcher.GetAwaiter"/>
        public ThreadSwitcherLongBackground GetAwaiter() => this;

        /// <inheritdoc cref="IThreadSwitcher.IsCompleted"/>
        public bool IsCompleted {
            get {
#if UNITY_WEBGL
#if UNITY_EDITOR
                if (useEditorPreference)
                    return !UnityThread.IsMainThread;
#endif
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
            if (useEditorPreference)
                goto other;
            Debug.Assert(!hasSwitched);
#if DEBUG
            Debug.LogWarning("Threading is not supported on this platform. A fallback to main thread has been used. Be warned that this may produce deadlocks very easily.");
#endif
            continuation();
            return;
        other:
#endif

            hasSwitched = true;
            // We always spawn a new thread regardless if we are already in a background thread
            // because maybe that thread is from a pool and so it's not suitable for long running tasks.
            Task.Factory.StartNew(continuation, TaskCreationOptions.LongRunning);
        }

        /// <inheritdoc cref="IThreadSwitcher.GetAwaiter"/>
        IThreadSwitcher IThreadSwitcher.GetAwaiter()
        {
#if UNITY_WEBGL
#if UNITY_EDITOR

            if (useEditorPreference && hasSwitched)
                return hasSwitchedGlobal;
#endif
            return hasSwitchedGlobal;
#else
            return hasSwitched ? hasSwitchedGlobal : hasNotSwitchedGlobal;
#endif
        }
    }
}
