﻿using System.Runtime.CompilerServices;

namespace Enderlook.Unity.Threading
{
    /// <summary>
    /// Defines an object that switches to a thread.
    /// </summary>
    public interface IThreadSwitcher : INotifyCompletion
    {
        // https://stackoverflow.com/a/58470597/7655838 from https://stackoverflow.com/questions/58469468/what-does-unitymainthreaddispatcher-do

        /// <summary>
        /// Determines if this task has completed.
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Returns the awaiter of this task.
        /// </summary>
        /// <returns>Awaiter of this task.</returns>
        IThreadSwitcher GetAwaiter();

        /// <summary>
        /// Get the result of the task.
        /// </summary>
        /// <returns>Whenever we switched of thread or not (either because we already were in the requested thread or it was not possible, for example: in WebGL platform threading is not supported).</returns>
        bool GetResult();
    }
}
