#if !UNITY_WEBGL
using System.Runtime.CompilerServices;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    public sealed partial class CoroutineManager
    {
        private struct NestedEnumeratorBackground<TEnumerator1, TEnumerator2, TCallback> : IValueCoroutineEnumerator
            where TEnumerator1 : IValueCoroutineEnumerator
            where TEnumerator2 : IValueCoroutineEnumerator
            where TCallback : INextCallback<TEnumerator1>
        {
            private readonly TypedManager<TEnumerator1> parentManager;
            private TEnumerator1 parent;
            private TEnumerator2 child;
            private TCallback callback;
            private ThreadMode mode;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public NestedEnumeratorBackground(TypedManager<TEnumerator1> parentManager, TEnumerator1 parent, TEnumerator2 child, TCallback callback, ThreadMode mode)
            {
                this.parentManager = parentManager;
                this.parent = parent;
                this.child = child;
                this.callback = callback;
                this.mode = mode;
            }

            public ValueCoroutineState State {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => ValueCoroutineStateHelper.Merge(parent.State, child.State);
            }

            public ValueCoroutineState ConcurrentState {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get {
                    switch (parent.ConcurrentState)
                    {
                        case ValueCoroutineState.Continue:
                            return child.ConcurrentState;
                        case ValueCoroutineState.Suspended:
                            switch (child.ConcurrentState)
                            {
                                case ValueCoroutineState.Finalized:
                                    return ValueCoroutineState.Finalized;
                                default:
                                    return ValueCoroutineState.Suspended;
                            }
                        case ValueCoroutineState.Finalized:
                            return ValueCoroutineState.Finalized;
                        default:
#if UNITY_EDITOR
                            Debug.Assert(false, "Impossible state.");
#endif
                            goto case ValueCoroutineState.Continue;
                    }
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() => child.Dispose();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ValueYieldInstruction Next()
            {
                switch (parent.State)
                {
                    case ValueCoroutineState.Continue:
                        ValueYieldInstruction instruction = child.Next();
                        if (instruction.Mode == ValueYieldInstruction.Type.Finalized)
                            parentManager.NextBackground(parent, callback, mode);
                        return instruction;
                    case ValueCoroutineState.Finalized:
                        return Yield.Finalized;
                    case ValueCoroutineState.Suspended:
                        return Yield.Suspended;
                    default:
#if UNITY_EDITOR
                        Debug.Assert(false, "Impossible state.");
#endif
                        goto case ValueCoroutineState.Continue;
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ValueYieldInstruction ConcurrentNext(ThreadMode mode)
            {
                switch (parent.ConcurrentState)
                {
                    case ValueCoroutineState.Continue:
                        ValueYieldInstruction instruction = child.ConcurrentNext(mode);
                        if (instruction.Mode == ValueYieldInstruction.Type.Finalized)
                            parentManager.NextBackground(parent, callback, mode);
                        return instruction;
                    case ValueCoroutineState.Finalized:
                        return Yield.Finalized;
                    case ValueCoroutineState.Suspended:
                        return Yield.Suspended;
                    default:
#if UNITY_EDITOR
                        Debug.Assert(false, "Impossible state.");
#endif
                        goto case ValueCoroutineState.Continue;
                }
            }
        }
    }
}
#endif