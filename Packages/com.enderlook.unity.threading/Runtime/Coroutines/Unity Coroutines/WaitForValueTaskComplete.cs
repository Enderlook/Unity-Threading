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
    public sealed class WaitForValueTaskComplete : CustomYieldInstruction
    {
        private static RawStack<WaitForValueTaskComplete> pool = RawStack<WaitForValueTaskComplete>.Create(Wait.POOL_CAPACITY);

        private ValueTask task;

        private WaitForValueTaskComplete(ValueTask task) => this.task = task;

        public override bool keepWaiting {
            get {
                if (task.IsCompleted)
                {
                    ValueTask task = this.task;
                    this.task = default;
                    if (pool.Count < Wait.POOL_CAPACITY)
                        pool.Push(this);
                    if (task.IsFaulted)
                        ExceptionDispatchInfo.Capture(task.AsTask().Exception).Throw();
                    return false;
                }
                return true;
            }
        }

        internal static void Clear() => pool.Clear();

#if UNITY_EDITOR
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
        internal static WaitForValueTaskComplete Create(ValueTask task)
        {
            if (pool.TryPop(out WaitForValueTaskComplete result))
            {
                result.task = task;
                return result;
            }
            return new WaitForValueTaskComplete(task);
        }

        /// <inheritdoc cref="Create(ValueTask)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator WaitForValueTaskComplete(ValueTask task)
            => Create(task);
    }
}
