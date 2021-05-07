using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Unity.Jobs;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    internal struct UnityCoroutineEnumerator : IValueCoroutineEnumerator
    {
        private IEnumerator enumerator;

        public ValueCoroutineState State { get; private set; }

        public ValueCoroutineState ConcurrentState => State;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnityCoroutineEnumerator(IEnumerator enumerator)
        {
            this.enumerator = enumerator;
            State = ValueCoroutineState.Continue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
        {
            // Not required but may be useful.
            if (enumerator is IDisposable disposable)
                disposable.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueYieldInstruction Next()
        {
            if (enumerator.MoveNext())
            {
                switch (enumerator.Current)
                {
                    case ValueYieldInstruction instruction:
                        instruction.DebugAssertIsAllowedInEnumerators();
                        return instruction;
                    case null:
                        return Yield.ToUpdate;
                    case WaitForUpdate v:
                        return v;
                    case WaitForFixedUpdate v:
                        return v;
                    case WaitForSeconds v:
                        return v;
                    case WaitForSecondsRealtime v:
                        return v;
                    case WaitForSecondsRealtimePooled v:
                        return v;
                    case WaitForEndOfFrame v:
                        return v;
                    case CustomYieldInstruction v:
                        return v;
                    case ValueTask v:
                        return v;
                    case Task v:
                        return v;
                    case JobHandle v:
                        return v;
                    case ValueCoroutine v:
                        return v;
                    case Coroutine v:
                        return v;
                    case YieldInstruction v:
                        return v;
                    case IEnumerator<ValueYieldInstruction> v:
                        return Yield.From(v);
                    case IEnumerator v:
                        return Yield.From(v);
                    case IValueCoroutineEnumerator v:
                        return Yield.From(v);
                    default:
#if DEBUG
                        Debug.Log($"Unexpected yielded type for value coroutine that implements {nameof(IEnumerator)}. This will result in undefined behaviour.");
#endif
                        return Yield.ToUpdate;
                }
            }
            else
            {
                State = ValueCoroutineState.Finalized;
                return Yield.Finalized;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueYieldInstruction ConcurrentNext(ValueCoroutineStateBoxed state, ThreadMode mode) => Next();
    }
}