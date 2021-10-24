using Enderlook.Unity.Threading;

using System;
using System.Runtime.CompilerServices;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    internal struct MonoBehaviourFinalizeWhenNullAndSuspendWhenIsNotActiveNorEnabled : IValueCoroutineState
    {
        private static readonly Func<MonoBehaviour, bool> callback = e => e.isActiveAndEnabled;

        private readonly MonoBehaviour monoBehaviour;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MonoBehaviourFinalizeWhenNullAndSuspendWhenIsNotActiveNorEnabled(MonoBehaviour monoBehaviour)
            => this.monoBehaviour = monoBehaviour;

        public ValueCoroutineState State {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                if (monoBehaviour == null)
                    return ValueCoroutineState.Finalized;
                if (!monoBehaviour.isActiveAndEnabled)
                    return ValueCoroutineState.Suspended;
                return ValueCoroutineState.Continue;
            }
        }

        public ValueCoroutineState ConcurrentState {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                if (monoBehaviour == null)
                    return ValueCoroutineState.Finalized;
                if (!UnityThread.RunNow(callback, monoBehaviour))
                    return ValueCoroutineState.Suspended;
                return ValueCoroutineState.Continue;
            }
        }
    }
}