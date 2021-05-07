using System.Runtime.CompilerServices;
using System.Threading;

namespace Enderlook.Unity.Coroutines
{
    internal struct CancellationTokenFinalizer : IValueCoroutineState
    {
        private readonly CancellationToken token;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CancellationTokenFinalizer(CancellationToken token)
            => this.token = token;

        public ValueCoroutineState State {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => token.IsCancellationRequested ? ValueCoroutineState.Finalized : ValueCoroutineState.Continue;
        }

        public ValueCoroutineState ConcurrentState {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => State;
        }
    }
}