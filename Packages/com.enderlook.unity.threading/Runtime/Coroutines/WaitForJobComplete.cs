using Enderlook.Collections.LowLevel;
using Enderlook.Unity.Threading.Jobs;

using Unity.Jobs;

using UnityEngine;

namespace Enderlook.Unity.Threading.Coroutines
{
    /// <summary>
    /// Suspend the coroutine execution until the suplied job handles is completed.
    /// </summary>
    public sealed class WaitForJobComplete : CustomYieldInstruction
    {
        private static readonly DynamicStack<WaitForJobComplete> pool = DynamicStack<WaitForJobComplete>.Create(Wait.POOL_CAPACITY);

        private JobHandle handle;

        private WaitForJobComplete(JobHandle handle) => this.handle = handle;

        public override bool keepWaiting {
            get {
                if (handle.IsCompleted)
                {
                    handle.Complete();
                    handle = default;
                    if (pool.Count < Wait.POOL_CAPACITY)
                        pool.Push(this);
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Suspend the coroutine execution until the suplied job handles is completed.<br/>
        /// The instance is draw from a pool.
        /// </summary>
        /// <param name="handle">Job handle to wait.</param>
        /// <returns>A waiter.</returns>
        internal static WaitForJobComplete Create(JobHandle handle)
        {
            if (pool.TryPop(out WaitForJobComplete result))
            {
                result.handle = handle;
                return result;
            }
            return new WaitForJobComplete(handle);
        }

        /// <inheritdoc cref="Create(JobHandle)"/>
        public static implicit operator WaitForJobComplete(JobHandle handle)
            => Create(handle);

        /// <inheritdoc cref="Create(JobHandle)"/>
        public static implicit operator WaitForJobComplete(JobHandleAwaiter handle)
            => handle.Wait();
    }
}
