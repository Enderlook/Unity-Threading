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
    public struct ThreadSwitcherTask : IThreadSwitcher
    {
        // https://stackoverflow.com/a/58470597/7655838 from https://stackoverflow.com/questions/58469468/what-does-unitymainthreaddispatcher-do

        // Alternatively we may do something like https://github.com/svermeulen/Unity3dAsyncAwaitUtil/blob/master/UnityProject/Assets/Plugins/AsyncAwaitUtil/Source/WaitForBackgroundThread.cs

        /// <inheritdoc cref="IThreadSwitcher.GetAwaiter"/>
        public ThreadSwitcherTask GetAwaiter() => this;

        /// <inheritdoc cref="IThreadSwitcher.IsCompleted"/>
        public bool IsCompleted => SynchronizationContext.Current == null;

        /// <inheritdoc cref="IThreadSwitcher.GetResult"/>
        public void GetResult() { }

        /// <inheritdoc cref="INotifyCompletion.OnCompleted(Action)"/>
        public void OnCompleted(Action continuation)
        {
            if (continuation is null)
                throw new ArgumentNullException(nameof(continuation));

            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                Debug.LogWarning("Threading is not supported on WebGL platform. A fallback to main thread has been used. Be warned that this may produce deadlocks very easelly.");
                continuation();
            }
            else
                Task.Run(continuation);
        }

        /// <inheritdoc cref="IThreadSwitcher.GetAwaiter"/>
        IThreadSwitcher IThreadSwitcher.GetAwaiter() => this;
    }
}
