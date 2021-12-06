using Enderlook.Pools;

using System.Runtime.CompilerServices;

using Unity.Jobs;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    /// <summary>
    /// Suspend the coroutine execution until the supplied job handles is completed.
    /// </summary>
    public sealed class WaitForJobComplete : CustomYieldInstruction
    {
        private JobHandle handle;

        public override bool keepWaiting {
            get {
                if (handle.IsCompleted)
                {
                    handle.Complete();
                    handle = default;
                    ObjectPool<WaitForJobComplete>.Shared.Return(this);
                    return false;
                }
                return true;
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Unity Editor Only.
        /// </summary>
        internal static int Count => ObjectPool<WaitForJobComplete>.Shared.ApproximateCount();
#endif

        /// <summary>
        /// Suspend the coroutine execution until the supplied job handles is completed.<br/>
        /// The instance is draw from a pool.
        /// </summary>
        /// <param name="handle">Job handle to wait.</param>
        /// <returns>A waiter.</returns>
        internal static WaitForJobComplete Create(JobHandle handle)
        {
            WaitForJobComplete waiter = ObjectPool<WaitForJobComplete>.Shared.Rent();
            waiter.handle = handle;
            return waiter;
        }

        /// <inheritdoc cref="Create(JobHandle)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator WaitForJobComplete(JobHandle handle) => Create(handle);
    }
}
