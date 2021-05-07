using Enderlook.Unity.Threading;

using System.Runtime.CompilerServices;
using System.Threading;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    internal struct GameObjectFinalizeWhenNullAndSuspendWhenIsNotActive : IValueCoroutineState
    {
        private static readonly SendOrPostCallback callback = e =>
        {
            StrongBox<(GameObject, bool)> box = (StrongBox<(GameObject, bool)>)e;
            box.Value.Item2 = box.Value.Item1.activeInHierarchy;
        };

        private readonly GameObject gameObject;
        private StrongBox<(GameObject, bool)> box;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GameObjectFinalizeWhenNullAndSuspendWhenIsNotActive(GameObject gameObject)
        {
            this.gameObject = gameObject;
            box = null;
        }

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
                if (box is null)
                {
                    box = new StrongBox<(GameObject, bool)>();
                    box.Value.Item1 = gameObject;
                }
                UnityThread.RunNow(callback, box);
                if (!box.Value.Item2)
                    return ValueCoroutineState.Suspended;
                return ValueCoroutineState.Continue;
            }
        }
    }
}