using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;

namespace Enderlook.Unity.Threading
{
    /// <summary>
    /// Defines a switcher to jump to a task thread
    /// </summary>
    public struct ThreadSwitcherBackground : IThreadSwitcher
    {
        // https://stackoverflow.com/a/58470597/7655838 from https://stackoverflow.com/questions/58469468/what-does-unitymainthreaddispatcher-do

        // Alternatively we may do something like https://github.com/svermeulen/Unity3dAsyncAwaitUtil/blob/master/UnityProject/Assets/Plugins/AsyncAwaitUtil/Source/WaitForBackgroundThread.cs

        private bool hasSwitched;

        /// <inheritdoc cref="IThreadSwitcher.GetAwaiter"/>
        public ThreadSwitcherBackground GetAwaiter() => this;

        /// <inheritdoc cref="IThreadSwitcher.IsCompleted"/>
        public bool IsCompleted => SynchronizationContext.Current == null;

        /// <inheritdoc cref="IThreadSwitcher.GetResult"/>
        public bool GetResult() => hasSwitched;

        /// <inheritdoc cref="INotifyCompletion.OnCompleted(Action)"/>
        public void OnCompleted(Action continuation)
        {
            if (continuation is null)
                throw new ArgumentNullException(nameof(continuation));

            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                // We don't need to set `hasSwitched` to false because it's already false
                Debug.LogWarning("Threading is not supported on WebGL platform. A fallback to main thread has been used. Be warned that this may produce deadlocks very easily.");
                continuation();
            }
            else
            {
                hasSwitched = ThreadSwitcher.IsExecutingMainThread;
                if (hasSwitched)
                    Task.Run(continuation);
                else
                {
#if UNITY_EDITOR
                    Debug.Log("Already in a non-main thread, we don't need to change of thread so we will not.");
#endif
                    continuation();
                }
            }
        }

        /// <inheritdoc cref="IThreadSwitcher.GetAwaiter"/>
        IThreadSwitcher IThreadSwitcher.GetAwaiter() => this;
    }
}
