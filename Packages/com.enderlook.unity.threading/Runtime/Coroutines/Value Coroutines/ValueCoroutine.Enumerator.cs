using System;
using System.Collections;
using System.Collections.Generic;

namespace Enderlook.Unity.Coroutines
{
    public readonly partial struct ValueCoroutine
    {
        private struct Enumerator<T> : IEnumerator<ValueYieldInstruction>
            where T : IEnumerator<ValueYieldInstruction>
        {
            private readonly Handle handler;
            private T enumerator;

            public Enumerator(Handle handler, T enumerator)
            {
                this.handler = handler;
                this.enumerator = enumerator;
            }

            ValueYieldInstruction IEnumerator<ValueYieldInstruction>.Current => enumerator.Current;

            object IEnumerator.Current => enumerator.Current;

            void IDisposable.Dispose() => enumerator.Dispose();

            bool IEnumerator.MoveNext()
            {
                if (enumerator.MoveNext())
                    return true;
                handler.Complete();
                return false;
            }

            void IEnumerator.Reset() => enumerator.Reset();
        }
    }
}