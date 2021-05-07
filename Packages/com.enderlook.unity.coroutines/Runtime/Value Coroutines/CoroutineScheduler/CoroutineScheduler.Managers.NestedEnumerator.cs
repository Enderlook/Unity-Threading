using System.Runtime.CompilerServices;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    public partial struct CoroutineScheduler
    {
        internal partial class Managers
        {
            private struct NestedEnumerator<TEnumerator1, TEnumerator2, TCallback> : IValueCoroutineEnumerator
                where TEnumerator1 : IValueCoroutineEnumerator
                where TEnumerator2 : IValueCoroutineEnumerator
                where TCallback : INextCallback<TEnumerator1>
            {
                private readonly TypedManager<TEnumerator1> parentManager;
                private TEnumerator1 parent;
                private TEnumerator2 child;
                private TCallback callback;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public NestedEnumerator(TypedManager<TEnumerator1> parentManager, TEnumerator1 parent, TEnumerator2 child, TCallback callback)
                {
                    this.parentManager = parentManager;
                    this.parent = parent;
                    this.child = child;
                    this.callback = callback;
                }

                public ValueCoroutineState State {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => ValueCoroutineStateHelper.Merge(parent.State, child.State);
                }

                public ValueCoroutineState ConcurrentState {
                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    get => ValueCoroutineStateHelper.Merge(parent.ConcurrentState, child.State);
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public void Dispose()
                {
                    if (parent.State == ValueCoroutineState.Finalized)
                        parent.Dispose();
                    child.Dispose();
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public ValueYieldInstruction Next()
                {
                    switch (parent.State)
                    {
                        case ValueCoroutineState.Finalized:
                            return Yield.Finalized;
                        case ValueCoroutineState.Suspended:
                            return Yield.Suspended;
                        case ValueCoroutineState.Continue:
                            ValueYieldInstruction instruction = child.Next();
                            if (instruction.Mode == ValueYieldInstruction.Type.Finalized)
                                parentManager.Next(parent, callback);
                            return instruction;
                        default:
                            Debug.Assert(false, "Impossible state.");
                            goto case ValueCoroutineState.Continue;
                    }
                }

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                public ValueYieldInstruction ConcurrentNext(ValueCoroutineStateBoxed state, ThreadMode mode)
                {
                    switch (parent.ConcurrentState)
                    {
                        case ValueCoroutineState.Finalized:
                            return Yield.Finalized;
                        case ValueCoroutineState.Suspended:
                            return Yield.Suspended;
                        case ValueCoroutineState.Continue:
                            ValueYieldInstruction instruction = child.ConcurrentNext(state, mode);
                            if (instruction.Mode == ValueYieldInstruction.Type.Finalized)
                                parentManager.NextBackground(parent, callback, state, mode);
                            return instruction;
                        default:
                            Debug.Assert(false, "Impossible state.");
                            goto case ValueCoroutineState.Continue;
                    }
                }
            }
        }
    }
}