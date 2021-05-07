using System.Runtime.CompilerServices;

namespace Enderlook.Unity.Coroutines
{
    internal class ValueCoroutineStateBoxed : IValueCoroutineState
    {
        public ValueCoroutineState State { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; set; }

        public ValueCoroutineState ConcurrentState {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => State;
        }
    }
}