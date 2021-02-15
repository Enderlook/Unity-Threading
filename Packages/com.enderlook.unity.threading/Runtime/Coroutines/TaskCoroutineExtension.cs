using System.Collections;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Enderlook.Unity.Threading.Coroutines
{
    /// <summary>
    /// Extension method for tasks to use in coroutines.
    /// </summary>
    public static class TaskCoroutineExtension
    {
        // https://github.com/svermeulen/Unity3dAsyncAwaitUtil/blob/master/UnityProject/Assets/Plugins/AsyncAwaitUtil/Source/TaskExtensions.cs

        /// <summary>
        /// Convert a task into an enumerator.
        /// </summary>
        /// <param name="task">Task to convert.</param>
        /// <returns>Enumerator which wraps the task.</returns>
        public static IEnumerator AsIEnumerator(this Task task)
        {
            while (!task.IsCompleted)
                yield return null;

            if (task.IsFaulted)
                ExceptionDispatchInfo.Capture(task.Exception).Throw();
        }

        /// <summary>
        /// Convert a task into an enumerator.
        /// </summary>
        /// <param name="task">Task to convert.</param>
        /// <returns>Enumerator which wraps the task.</returns>
        public static IEnumerator AsIEnumerator<T>(this Task<T> task)
        {
            while (!task.IsCompleted)
                yield return null;

            if (task.IsFaulted)
                ExceptionDispatchInfo.Capture(task.Exception).Throw();
        }

        /// <summary>
        /// Convert a task into an enumerator.
        /// </summary>
        /// <param name="task">Task to convert.</param>
        /// <returns>Enumerator which wraps the task.</returns>
        public static IEnumerator AsIEnumerator(this ValueTask task)
        {
            while (!task.IsCompleted)
                yield return null;

            if (task.IsFaulted)
                ExceptionDispatchInfo.Capture(task.AsTask().Exception).Throw();
        }

        /// <summary>
        /// Convert a task into an enumerator.
        /// </summary>
        /// <param name="task">Task to convert.</param>
        /// <returns>Enumerator which wraps the task.</returns>
        public static IEnumerator AsIEnumerator<T>(this ValueTask<T> task)
        {
            while (!task.IsCompleted)
                yield return null;

            if (task.IsFaulted)
                ExceptionDispatchInfo.Capture(task.AsTask().Exception).Throw();
        }

        /// <summary>
        /// Wait task to complete.
        /// </summary>
        /// <param name="task">Task to complete.</param>
        /// <returns>Waiter for the task to complete.</returns>
        public static WaitForTaskComplete Wait(this Task task)
            => new WaitForTaskComplete(task);

        /// <summary>
        /// Wait task to complete.
        /// </summary>
        /// <typeparam name="T">Type of element returned by the task.</typeparam>
        /// <param name="task">Task to complete.</param>
        /// <returns>Waiter for the task to complete.</returns>
        public static WaitForTaskComplete<T> Wait<T>(this Task<T> task)
            => new WaitForTaskComplete<T>(task);

        /// <summary>
        /// Wait task to complete.
        /// </summary>
        /// <param name="task">Task to complete.</param>
        /// <returns>Waiter for the task to complete.</returns>
        public static WaitForValueTaskComplete Wait(this ValueTask task)
            => new WaitForValueTaskComplete(task);

        /// <summary>
        /// Wait task to complete.
        /// </summary>
        /// <typeparam name="T">Type of element returned by the task.</typeparam>
        /// <param name="task">Task to complete.</param>
        /// <returns>Waiter for the task to complete.</returns>
        public static WaitForValueTaskComplete<T> Wait<T>(this ValueTask<T> task)
            => new WaitForValueTaskComplete<T>(task);
    }
}
