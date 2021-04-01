using System;
using System.Collections;
using System.Collections.Generic;

namespace Enderlook.Unity.Coroutines
{
    public partial struct CoroutineScheduler
    {
        internal partial class Managers
        {
            private struct NestedEnumerator<T, U> : IEnumerator<ValueYieldInstruction>
                where T : IEnumerator<ValueYieldInstruction>
                where U : ICancellable
            {
                private readonly TypedManager<T, U> manager;
                private TypedManager<T, U>.Routine routine;
                private IEnumerator<ValueYieldInstruction> enumerator;

                public NestedEnumerator(TypedManager<T, U> manager, TypedManager<T, U>.Routine routine, IEnumerator<ValueYieldInstruction> enumerator)
                {
                    this.manager = manager;
                    this.routine = routine;
                    this.enumerator = enumerator;
                }

                ValueYieldInstruction IEnumerator<ValueYieldInstruction>.Current => enumerator.Current;

                object IEnumerator.Current => ((IEnumerator)enumerator).Current;

                void IDisposable.Dispose() => enumerator.Dispose();

                bool IEnumerator.MoveNext()
                {
                    if (enumerator.MoveNext())
                        return true;
                    else
                    {
                        manager.Next(routine);
                        return false;
                    }
                }

                void IEnumerator.Reset() => enumerator.Reset();
            }

            private struct NestedEnumeratorBackground<T, U> : IEnumerator<ValueYieldInstruction>
                where T : IEnumerator<ValueYieldInstruction>
                where U : ICancellable
            {
                private readonly TypedManager<T, U> manager;
                private TypedManager<T, U>.Routine routine;
                private IEnumerator<ValueYieldInstruction> enumerator;
                private int mode;

                public NestedEnumeratorBackground(TypedManager<T, U> manager, TypedManager<T, U>.Routine routine, IEnumerator<ValueYieldInstruction> enumerator, int mode)
                {
                    this.manager = manager;
                    this.routine = routine;
                    this.enumerator = enumerator;
                    this.mode = mode;
                }

                ValueYieldInstruction IEnumerator<ValueYieldInstruction>.Current => enumerator.Current;

                object IEnumerator.Current => ((IEnumerator)enumerator).Current;

                void IDisposable.Dispose() => enumerator.Dispose();

                bool IEnumerator.MoveNext()
                {
                    if (enumerator.MoveNext())
                        return true;
                    else
                    {
                        manager.NextBackground(routine, mode);
                        return false;
                    }
                }

                void IEnumerator.Reset() => enumerator.Reset();
            }
        }
    }
}