using System;
using System.Collections;
using System.Collections.Generic;

namespace Enderlook.Unity.Coroutines
{
    internal static partial class CoroutineManagers
    {
        private struct NestedEnumerator<T, U> : IEnumerator<ValueYieldInstruction>
            where T : IEnumerator<ValueYieldInstruction>
            where U : ICancellable
        {
            private CoroutineManager<T, U>.Routine routine;
            private IEnumerator<ValueYieldInstruction> enumerator;

            public NestedEnumerator(CoroutineManager<T, U>.Routine routine, IEnumerator<ValueYieldInstruction> enumerator)
            {
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
                    CoroutineManager<T, U>.Next(routine);
                    return false;
                }
            }

            void IEnumerator.Reset() => enumerator.Reset();
        }

        private struct NestedEnumeratorBackground<T, U> : IEnumerator<ValueYieldInstruction>
            where T : IEnumerator<ValueYieldInstruction>
            where U : ICancellable
        {
            private CoroutineManager<T, U>.Routine routine;
            private IEnumerator<ValueYieldInstruction> enumerator;
            private int mode;

            public NestedEnumeratorBackground(CoroutineManager<T, U>.Routine routine, IEnumerator<ValueYieldInstruction> enumerator, int mode)
            {
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
                    CoroutineManager<T, U>.NextBackground(routine, mode);
                    return false;
                }
            }

            void IEnumerator.Reset() => enumerator.Reset();
        }
    }
}