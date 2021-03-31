using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Enderlook.Unity.Coroutines
{
    public partial struct CoroutineScheduler
    {
        internal partial class Managers
        {
            private partial class TypedManager<T, U> where T : IEnumerator<ValueYieldInstruction>
                where U : ICancellable
            {
                internal struct Routine : IDisposable
                {
                    public readonly U cancellator;
                    private T enumerator;

                    public Routine(U cancellator, T enumerator)
                    {
                        this.cancellator = cancellator;
                        this.enumerator = enumerator;
                    }

                    public bool IsCancelationRequested {
                        [MethodImpl(MethodImplOptions.AggressiveInlining)]
                        get => cancellator.IsCancelationRequested;
                    }

                    public ValueYieldInstruction Current {
                        [MethodImpl(MethodImplOptions.AggressiveInlining)]
                        get => enumerator.Current;
                    }

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    public bool MoveNext() => enumerator.MoveNext();

                    public void Dispose() => enumerator.Dispose();
                }
            }
        }
    }
}