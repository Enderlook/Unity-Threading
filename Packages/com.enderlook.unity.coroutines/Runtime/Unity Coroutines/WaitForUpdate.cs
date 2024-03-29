﻿using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    /// <summary>
    /// Wait until the next frame, same as passing <see langword="null"/> to a coroutine.
    /// </summary>
    public sealed class WaitForUpdate : CustomYieldInstruction
    {
        // https://github.com/svermeulen/Unity3dAsyncAwaitUtil/blob/master/UnityProject/Assets/Plugins/AsyncAwaitUtil/Source/WaitForUpdate.cs

        /// <inheritdoc cref="CustomYieldInstruction.keepWaiting"/>
        public override bool keepWaiting => false;

        internal WaitForUpdate() { }
    }
}
