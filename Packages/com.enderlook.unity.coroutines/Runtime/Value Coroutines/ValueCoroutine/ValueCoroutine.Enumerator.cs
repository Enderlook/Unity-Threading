using System.Runtime.CompilerServices;

namespace Enderlook.Unity.Coroutines
{
    public readonly partial struct ValueCoroutine
    {
        private struct Coroutine<T> : IValueCoroutineEnumerator
            where T : IValueCoroutineEnumerator
        {
            private readonly Handle handler;
            private T coroutine;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Coroutine(Handle handler, T coroutine)
            {
                this.handler = handler;
                this.coroutine = coroutine;
            }

            public ValueCoroutineState State {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => coroutine.State;
            }

            public ValueCoroutineState ConcurrentState {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => coroutine.ConcurrentState;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() => coroutine.Dispose();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ValueYieldInstruction Next()
            {
                ValueYieldInstruction instruction = coroutine.Next();
                if (instruction.Mode == ValueYieldInstruction.Type.Finalized)
                    handler.Complete();
                return instruction;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public ValueYieldInstruction ConcurrentNext(ThreadMode mode)
            {
                ValueYieldInstruction instruction = coroutine.ConcurrentNext(mode);
                if (instruction.Mode == ValueYieldInstruction.Type.Finalized)
                    handler.Complete();
                return instruction;
            }
        }
    }
}