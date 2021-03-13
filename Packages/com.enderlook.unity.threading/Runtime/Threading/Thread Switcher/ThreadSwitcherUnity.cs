using System;
using System.Runtime.CompilerServices;
using System.Threading;

using UnityEngine;

namespace Enderlook.Unity.Threading
{
    /// <summary>
    /// Defines a switcher to jump to the Unity thread.
    /// </summary>
    public struct ThreadSwitcherUnity : IThreadSwitcher
    {
        // https://stackoverflow.com/a/58470597/7655838 from https://stackoverflow.com/questions/58469468/what-does-unitymainthreaddispatcher-do

        private bool hasSwitched;

        /// <inheritdoc cref="IThreadSwitcher.GetAwaiter"/>
        public ThreadSwitcherUnity GetAwaiter() => this;

        /// <inheritdoc cref="IThreadSwitcher.IsCompleted"/>
        public bool IsCompleted => SynchronizationContext.Current == UnitySynchronizationContextUtility.UnitySynchronizationContext;

        /// <inheritdoc cref="IThreadSwitcher.GetResult"/>
        public bool GetResult() => hasSwitched;

        /// <inheritdoc cref="INotifyCompletion.OnCompleted(Action)"/>
        public void OnCompleted(Action continuation)
        {
            if (continuation == null)
                throw new ArgumentNullException(nameof(continuation));

            hasSwitched = !Switch.IsInMainThread;
            if (!hasSwitched)
            {
#if DEBUG
                Debug.Log("Already in main thread, this will do nothing.");
#endif
                continuation();
            }
            else
                UnitySynchronizationContextUtility.UnitySynchronizationContext.Post(Switch.callback, continuation);
        }

        /// <inheritdoc cref="IThreadSwitcher.GetAwaiter"/>
        IThreadSwitcher IThreadSwitcher.GetAwaiter() => this;
    }
}
