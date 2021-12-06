using Enderlook.Collections.LowLevel;
using Enderlook.Pools;

using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    /// <summary>
    /// Suspend the coroutine execution until the supplied task is completed.
    /// </summary>
    /// <typeparam name="T">Return type of task.</typeparam>
    public sealed class WaitForValueTaskComplete<T> : CustomYieldInstruction
    {
        private ValueTask<T> task;

        public override bool keepWaiting {
            get {
                if (task.IsCompleted)
                {
                    ValueTask<T> task = this.task;
                    this.task = default;
                    ObjectPool<WaitForValueTaskComplete<T>>.Shared.Return(this);
                    if (task.IsFaulted)
                        ExceptionDispatchInfo.Capture(task.AsTask().Exception).Throw();
                    return false;
                }
                return true;
            }
        }

#if UNITY_EDITOR
        static WaitForValueTaskComplete()
        {
            Wait.AddWaitForTaskComplete($"Wait.For({typeof(WaitForValueTaskComplete<T>).Name})", () => ObjectPool<WaitForValueTaskComplete<T>>.Shared.ApproximateCount());
        }
#endif

        /// <summary>
        /// Suspend the coroutine execution until the supplied task is completed.<br/>
        /// Object is drawn from a pool.
        /// </summary>
        /// <param name="task">Task to wait.</param>
        /// <returns>A waiter.</returns>
        internal static WaitForValueTaskComplete<T> Create(ValueTask<T> task)
        {
            WaitForValueTaskComplete<T> waiter = ObjectPool<WaitForValueTaskComplete<T>>.Shared.Rent();
            waiter.task = task;
            return waiter;
        }

        /// <inheritdoc cref="Create(ValueTask{T})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator WaitForValueTaskComplete<T>(ValueTask<T> task)
            => Create(task);
    }
}
