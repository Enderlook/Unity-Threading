namespace Enderlook.Unity.Coroutines
{
    /// <summary>
    /// Determines the state of the coroutine.
    /// </summary>
    internal enum ValueCoroutineState : byte
    {
        /// <summary>
        /// The coroutine can continue.
        /// </summary>
        Continue = 1 << 0,

        /// <summary>
        /// The coroutine has been finalized or cancelled.
        /// </summary>
        Finalized = 1 << 1,

        /// <summary>
        /// The coroutine has is suspended.
        /// </summary>
        Suspended = 1 << 2,
    }
}