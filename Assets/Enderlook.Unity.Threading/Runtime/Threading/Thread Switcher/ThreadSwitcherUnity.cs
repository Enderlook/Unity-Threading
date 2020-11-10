using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Enderlook.Unity.Threading
{
    /// <summary>
    /// Defines a switcher to jump to the Unity thread
    /// </summary>
    public struct ThreadSwitcherUnity : IThreadSwitcher
    {
        // https://stackoverflow.com/a/58470597/7655838 from https://stackoverflow.com/questions/58469468/what-does-unitymainthreaddispatcher-do

        /// <inheritdoc cref="IThreadSwitcher.GetAwaiter"/>
        public ThreadSwitcherUnity GetAwaiter() => this;

        /// <inheritdoc cref="IThreadSwitcher.IsCompleted"/>
        public bool IsCompleted => SynchronizationContext.Current == UnitySynchronizationContextUtility.UnitySynchronizationContext;

        /// <inheritdoc cref="IThreadSwitcher.GetResult"/>
        public void GetResult() { }

        /// <inheritdoc cref="INotifyCompletion.OnCompleted(Action)"/>
        public void OnCompleted(Action continuation)
        {
            if (continuation == null)
                throw new ArgumentNullException(nameof(continuation));

            UnitySynchronizationContextUtility.UnitySynchronizationContext.Post(_ => continuation(), null);
        }

        /// <inheritdoc cref="IThreadSwitcher.GetAwaiter"/>
        IThreadSwitcher IThreadSwitcher.GetAwaiter() => this;
    }
}
