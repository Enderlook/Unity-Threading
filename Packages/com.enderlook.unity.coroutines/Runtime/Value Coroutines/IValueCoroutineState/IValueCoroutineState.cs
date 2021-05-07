namespace Enderlook.Unity.Coroutines
{
    /// <summary>
    /// Determines the state of a coroutine.
    /// </summary>
    internal interface IValueCoroutineState
    {
        /// <summary>
        /// State of the coroutine.<br/>
        /// This method must be executed from the Unity thread.
        /// </summary>
        ValueCoroutineState State { get; }

        /// <summary>
        /// State of the coroutine.
        /// </summary>
        ValueCoroutineState ConcurrentState { get; }
    }
}