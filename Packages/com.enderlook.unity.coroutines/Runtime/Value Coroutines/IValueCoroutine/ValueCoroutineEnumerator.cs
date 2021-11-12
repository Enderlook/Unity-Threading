using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Enderlook.Unity.Coroutines
{
    internal struct ValueCoroutineEnumerator<T> : IValueCoroutineEnumerator
        where T : IEnumerator<ValueYieldInstruction>
    {
        private T enumerator;

        public ValueCoroutineState State { get; private set; }

#if !UNITY_WEBGL
        public ValueCoroutineState ConcurrentState => State;
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueCoroutineEnumerator(T enumerator)
        {
            this.enumerator = enumerator;
            State = ValueCoroutineState.Continue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => enumerator.Dispose();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueYieldInstruction Next()
        {
            if (enumerator.MoveNext())
            {
                ValueYieldInstruction instruction = enumerator.Current;
                instruction.DebugAssertIsAllowedInEnumerators();
                return instruction;
            }
            State = ValueCoroutineState.Finalized;
            return Yield.Finalized;
        }

#if !UNITY_WEBGL
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueYieldInstruction ConcurrentNext(ThreadMode mode) => Next();
#endif
    }
}