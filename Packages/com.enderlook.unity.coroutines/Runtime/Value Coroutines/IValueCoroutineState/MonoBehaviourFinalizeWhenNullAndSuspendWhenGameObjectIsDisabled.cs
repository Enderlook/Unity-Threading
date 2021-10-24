using Enderlook.Unity.Threading;

using System;
using System.Runtime.CompilerServices;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    internal struct MonoBehaviourFinalizeWhenNullAndSuspendWhenGameObjectIsDisabled : IValueCoroutineState
    {
        private static readonly Func<MonoBehaviour, bool> callback = e => e.gameObject.activeInHierarchy;

        private readonly MonoBehaviour monoBehaviour;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MonoBehaviourFinalizeWhenNullAndSuspendWhenGameObjectIsDisabled(MonoBehaviour monoBehaviour)
            => this.monoBehaviour = monoBehaviour;

        public ValueCoroutineState State {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                if (monoBehaviour == null)
                    return ValueCoroutineState.Finalized;
                if (!monoBehaviour.gameObject.activeInHierarchy)
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