﻿using Enderlook.Collections.LowLevel;

using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

using UnityEngine;

namespace Enderlook.Unity.Threading.Coroutines
{
    /// <summary>
    /// Suspend the coroutine execution until the suplied task is completed.
    /// </summary>
    /// <typeparam name="T">Return type of task.</typeparam>
    public sealed class WaitForTaskComplete<T> : CustomYieldInstruction
    {
        private static readonly DynamicStack<WaitForTaskComplete<T>> pool = DynamicStack<WaitForTaskComplete<T>>.Create(Wait.POOL_CAPACITY);

        private Task<T> task;

        private WaitForTaskComplete(Task<T> task) => this.task = task;

        public override bool keepWaiting {
            get {
                if (task.IsCompleted)
                {
                    Task<T> task = this.task;
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

        /// <summary>
        /// Suspend the coroutine execution until the suplied task is completed.<br/>
        /// Object is drawn from a pool.
        /// </summary>
        /// <param name="task">Task to wait.</param>
        /// <returns>A waiter.</returns>
        internal static WaitForTaskComplete<T> Create(Task<T> task)
        {
            if (pool.TryPop(out WaitForTaskComplete<T> result))
            {
                result.task = task;
                return result;
            }
            return new WaitForTaskComplete<T>(task);
        }

        /// <inheritdoc cref="Create(Task{T})"/>
        public static implicit operator WaitForTaskComplete<T>(Task<T> task)
            => Create(task);
    }
}
