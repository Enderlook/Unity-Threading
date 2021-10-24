using Enderlook.Unity.Threading;

using System;
using System.Runtime.CompilerServices;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    internal struct GameObjectFinalizeWhenNullAndSuspendWhenIsNotActive : IValueCoroutineState
    {
        private static readonly Func<GameObject, bool> callback = e => e.activeInHierarchy;

        private readonly GameObject gameObject;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GameObjectFinalizeWhenNullAndSuspendWhenIsNotActive(GameObject gameObject)
            => this.gameObject = gameObject;

        public ValueCoroutineState State {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                if (gameObject == null)
                    return ValueCoroutineState.Finalized;
                if (!gameObject.activeInHierarchy)
                    return ValueCoroutineState.Suspended;
                return ValueCoroutineState.Continue;
            }
        }

        public ValueCoroutineState ConcurrentState {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get {
                if (gameObject == null)
                    return ValueCoroutineState.Finalized;
                if (!UnityThread.RunNow(callback, gameObject))
                    return ValueCoroutineState.Suspended;
                return ValueCoroutineState.Continue;
            }
        }
    }
}