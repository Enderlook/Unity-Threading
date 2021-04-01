using Enderlook.Collections.LowLevel;

using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    /// <summary>
    /// Suspend the coroutine execution until the suplied task is completed.
    /// </summary>
    /// <typeparam name="T">Return type of task.</typeparam>
    public sealed class WaitForValueTaskComplete<T> : CustomYieldInstruction
    {
        private static RawStack<WaitForValueTaskComplete<T>> pool = RawStack<WaitForValueTaskComplete<T>>.Create(Wait.POOL_CAPACITY);

        private ValueTask<T> task;

        private WaitForValueTaskComplete(ValueTask<T> task) => this.task = task;

        public override bool keepWaiting {
            get {
                if (task.IsCompleted)
                {
                    ValueTask<T> task = this.task;
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

        static WaitForValueTaskComplete()
        {
            Action clear = Clear;
            Wait.SubscribeClear(clear);
#if UNITY_EDITOR
            Wait.AddWaitForTaskComplete($"Wait.For({typeof(WaitForValueTaskComplete<T>).Name})", () => pool.Count, clear);
#endif
        }

        /// <summary>
        /// Suspend the coroutine execution until the suplied task is completed.<br/>
        /// Object is drawn from a pool.
        /// </summary>
        /// <param name="task">Task to wait.</param>
        /// <returns>A waiter.</returns>
        internal static WaitForValueTaskComplete<T> Create(ValueTask<T> task)
        {
            if (pool.TryPop(out WaitForValueTaskComplete<T> result))
            {
                result.task = task;
                return result;
            }
            return new WaitForValueTaskComplete<T>(task);
        }

        /// <inheritdoc cref="Create(ValueTask{T})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator WaitForValueTaskComplete<T>(ValueTask<T> task)
            => Create(task);
    }
}
