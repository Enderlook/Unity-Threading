using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    /// <summary>
    /// Allow get awaiter from <see cref="Coroutine"/>.
    /// </summary>
    public static class CoroutineAwaiterExtension
    {
        /// <summary>
        /// Convert a <see cref="Coroutine"/> to a task.
        /// </summary>
        /// <param name="coroutine">Coroutine to convert.</param>
        /// <returns>Task wrapper of a Unity coroutine.</returns>
        public static CoroutineAwaiter GetAwaiter(this Coroutine coroutine) => new CoroutineAwaiter(coroutine);
    }
}