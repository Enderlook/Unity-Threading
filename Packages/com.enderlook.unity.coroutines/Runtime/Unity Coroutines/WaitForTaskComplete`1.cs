using Enderlook.Pools;

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
    public sealed class WaitForTaskComplete<T> : CustomYieldInstruction
    {
        private Task<T> task;

        public override bool keepWaiting {
            get {
                if (task.IsCompleted)
                {
                    Task<T> task = this.task;
                    this.task = default;
                    ObjectPool<WaitForTaskComplete<T>>.Shared.Return(this);
                    if (task.IsFaulted)
                        ExceptionDispatchInfo.Capture(task.Exception).Throw();
                    return false;
                }
                return true;
            }
        }

#if UNITY_EDITOR
        static WaitForTaskComplete()
        {
            Wait.AddWaitForTaskComplete($"Wait.For({typeof(WaitForTaskComplete<T>).Name})", () => ObjectPool<WaitForTaskComplete<T>>.Shared.ApproximateCount());
        }
#endif

        /// <summary>
        /// Suspend the coroutine execution until the supplied task is completed.<br/>
        /// Object is drawn from a pool.
        /// </summary>
        /// <param name="task">Task to wait.</param>
        /// <returns>A waiter.</returns>
        internal static WaitForTaskComplete<T> Create(Task<T> task)
        {
            WaitForTaskComplete<T> waiter = ObjectPool<WaitForTaskComplete<T>>.Shared.Rent();
            waiter.task = task;
            return waiter;
        }

        /// <inheritdoc cref="Create(Task{T})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator WaitForTaskComplete<T>(Task<T> task)
            => Create(task);
    }
}
