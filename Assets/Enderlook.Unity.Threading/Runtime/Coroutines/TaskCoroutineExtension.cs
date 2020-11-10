using System.Collections;
using System.Collections.Generic;
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
        public static IEnumerator<T> AsIEnumerator<T>(this Task<T> task)
        {
            while (!task.IsCompleted)
                yield return default;

            if (task.IsFaulted)
                ExceptionDispatchInfo.Capture(task.Exception).Throw();

            yield return task.Result;
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
        public static IEnumerator<T> AsIEnumerator<T>(this ValueTask<T> task)
        {
            while (!task.IsCompleted)
                yield return default;

            if (task.IsFaulted)
                ExceptionDispatchInfo.Capture(task.AsTask().Exception).Throw();

            yield return task.Result;
        }
    }
}
