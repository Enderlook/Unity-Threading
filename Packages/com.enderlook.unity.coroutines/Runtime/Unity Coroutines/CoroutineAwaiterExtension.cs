namespace Enderlook.Unity.Coroutines
{
    /// <summary>
    /// Allow get awaiter from <see cref="UnityEngine.Coroutine"/>.
    /// </summary>
    public static class CoroutineAwaiterExtension
    {
        /// <summary>
        /// Convert a <see cref="UnityEngine.Coroutine"/> to a task.
        /// </summary>
        /// <param name="coroutine">Coroutine to convert.</param>
        /// <returns>Task wrapper of a Unity coroutine.</returns>
        public static CoroutineAwaiter GetAwaiter(this UnityEngine.Coroutine coroutine) => new CoroutineAwaiter(coroutine);
    }
}