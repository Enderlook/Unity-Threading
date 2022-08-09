using System.Runtime.CompilerServices;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    internal static class ValueCoroutineStateHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutineState Merge(ValueCoroutineState a, ValueCoroutineState b)
        {
            switch (a | b)
            {
                case ValueCoroutineState.Finalized | ValueCoroutineState.Continue:
                case ValueCoroutineState.Finalized | ValueCoroutineState.Suspended:
                //case ValueCoroutineState.Finalized | ValueCoroutineState.Finalized:
                    return ValueCoroutineState.Finalized;
                case ValueCoroutineState.Suspended | ValueCoroutineState.Continue:
                //case ValueCoroutineState.Suspended | ValueCoroutineState.Suspended:
                    return ValueCoroutineState.Suspended;
                case ValueCoroutineState.Continue | ValueCoroutineState.Continue:
                    return ValueCoroutineState.Continue;
            }
            Debug.Assert(false, "Impossible state.");
            return default;
        }
    }
}