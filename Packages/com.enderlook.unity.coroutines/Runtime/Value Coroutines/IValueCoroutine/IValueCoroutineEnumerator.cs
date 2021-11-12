using System;

namespace Enderlook.Unity.Coroutines
{
    /// <summary>
    /// Represent a value coroutine enumerator.
    /// </summary>
    internal interface IValueCoroutineEnumerator : IDisposable
    {
        /// <summary>
        /// Determines the state of the coroutine.<br/>
        /// This will only be executed from Unity Thread.
        /// </summary>
        ValueCoroutineState State { get; }

#if !UNITY_WEBGL
        /// <summary>
        /// Determines the state of the coroutine.
        /// </summary>
        ValueCoroutineState ConcurrentState { get; }
#endif

        /// <summary>
        /// Advances the coroutine by one.<br/>
        /// This will only be executed from Unity Thread.
        /// </summary>
        /// <returns>Instruction of how coroutione should continue its execution.</returns>
        /// <remarks>While <see cref="State"/> is <see cref="ValueCoroutineState.Suspended"/> this must return <see cref="Yield.Suspended"/>.<br/>
        /// If <see cref="State"/> is <see cref="ValueCoroutineState.Finalized"/> this must return <see cref="Yield.Finalized"/>.</remarks>
        ValueYieldInstruction Next();

#if !UNITY_WEBGL
        /// <summary>
        /// Advances the coroutine by one.
        /// </summary>
        /// <param name="mode">Type of thread where this function is being executed.</param>
        /// <returns>Instruction of how coroutione should continue its execution.</returns>
        /// <remarks>While <see cref="State"/> is <see cref="ValueCoroutineState.Suspended"/> this must return <see cref="Yield.Suspended"/>.<br/>
        /// If <see cref="State"/> is <see cref="ValueCoroutineState.Finalized"/> this must return <see cref="Yield.Finalized"/>.</remarks>
        ValueYieldInstruction ConcurrentNext(ThreadMode mode);
#endif
    }
}