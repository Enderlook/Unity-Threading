using Enderlook.Collections.LowLevel;

using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    /// <summary>
    /// Suspend the coroutine execution until the suplied task is completed.
    /// </summary>
    public sealed class WaitForTaskComplete : CustomYieldInstruction
    {
        private static RawStack<WaitForTaskComplete> pool = RawStack<WaitForTaskComplete>.Create(Wait.POOL_CAPACITY);

        private Task task;

        private WaitForTaskComplete(Task task) => this.task = task;

        public override bool keepWaiting {
            get {
                if (task.IsCompleted)
                {
                    Task task = this.task;
                    this.task = default;
                    if (pool.Count < Wait.POOL_CAPACITY)
                        pool.Push(this);
                    if (task.IsFaulted)
                        ExceptionDispatchInfo.Capture(task.Exception).Throw();
                    return false;
                }
                return true;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        internal static void Clear() => pool.Clear();

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
        private static void Initialize() => UnityEditor.EditorApplication.playModeStateChanged += (_) => Clear();

        /// <summary>
        /// Unity Editor Only.
        /// </summary>
        internal static int Count => pool.Count;
#endif

        /// <summary>
        /// Suspend the coroutine execution until the suplied task is completed.<br/>
        /// Object is drawn from a pool.
        /// </summary>
        /// <param name="task">Task to wait.</param>
        /// <returns>A waiter.</returns>
        internal static WaitForTaskComplete Create(Task task)
        {
            if (pool.TryPop(out WaitForTaskComplete result))
            {
                result.task = task;
                return result;
            }
            return new WaitForTaskComplete(task);
        }

        /// <inheritdoc cref="Create(Task)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator WaitForTaskComplete(Task task)
            => Create(task);
    }
}
