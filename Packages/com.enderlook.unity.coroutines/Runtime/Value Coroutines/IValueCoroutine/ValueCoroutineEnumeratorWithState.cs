using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Enderlook.Unity.Coroutines
{
    internal struct ValueCoroutineEnumeratorWithState<T, U> : IValueCoroutineEnumerator
        where T : IEnumerator<ValueYieldInstruction>
        where U : IValueCoroutineState
    {
        private T enumerator;
        private U token;
        private ValueCoroutineState state;

        public ValueCoroutineState State {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ValueCoroutineStateHelper.Merge(token.State, state);
        }

        public ValueCoroutineState ConcurrentState {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ValueCoroutineStateHelper.Merge(token.ConcurrentState, state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueCoroutineEnumeratorWithState(T enumerator, U token)
        {
            this.enumerator = enumerator;
            this.token = token;
            state = ValueCoroutineState.Continue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose() => enumerator.Dispose();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueYieldInstruction Next()
        {
            switch (token.State)
            {
                case ValueCoroutineState.Finalized:
                    return Yield.Finalized;
                case ValueCoroutineState.Suspended:
                    return Yield.Suspended;
            }

            if (enumerator.MoveNext())
            {
                ValueYieldInstruction instruction = enumerator.Current;
                instruction.DebugAssertIsAllowedInEnumerators();
                return instruction;
            }

            state = ValueCoroutineState.Finalized;
            return Yield.Finalized;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueYieldInstruction ConcurrentNext(ThreadMode mode)
        {
            switch (token.ConcurrentState)
            {
                case ValueCoroutineState.Finalized:
                    return Yield.Finalized;
                case ValueCoroutineState.Suspended:
                    return Yield.Suspended;
            }

            if (enumerator.MoveNext())
            {
                ValueYieldInstruction instruction = enumerator.Current;
                instruction.DebugAssertIsAllowedInEnumerators();
                return instruction;
            }

            this.state = ValueCoroutineState.Finalized;
            return Yield.Finalized;
        }
    }
}