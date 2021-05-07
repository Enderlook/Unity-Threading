using System.Runtime.CompilerServices;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    internal struct UnityObjectFinalizeWhenNull : IValueCoroutineState
    {
        private readonly Object @object;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnityObjectFinalizeWhenNull(Object @object)
            => this.@object = @object;

        public ValueCoroutineState State {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => @object == null ? ValueCoroutineState.Finalized : ValueCoroutineState.Continue;
        }
        public ValueCoroutineState ConcurrentState {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => State;
        }
    }
}