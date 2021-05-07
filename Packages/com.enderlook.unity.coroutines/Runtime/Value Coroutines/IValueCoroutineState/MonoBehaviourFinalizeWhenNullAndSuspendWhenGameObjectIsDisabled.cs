using Enderlook.Unity.Threading;

using System.Runtime.CompilerServices;
using System.Threading;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    internal struct MonoBehaviourFinalizeWhenNullAndSuspendWhenGameObjectIsDisabled : IValueCoroutineState
    {
        private static readonly SendOrPostCallback callback = e =>
        {
            StrongBox<(MonoBehaviour, bool)> box = (StrongBox<(MonoBehaviour, bool)>)e;
            box.Value.Item2 = box.Value.Item1.gameObject.activeInHierarchy;
        };

        private readonly MonoBehaviour monoBehaviour;
        private StrongBox<(MonoBehaviour, bool)> box;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public MonoBehaviourFinalizeWhenNullAndSuspendWhenGameObjectIsDisabled(MonoBehaviour monoBehaviour)
        {
            this.monoBehaviour = monoBehaviour;
            box = null;
        }

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
                if (box is null)
                {
                    box = new StrongBox<(MonoBehaviour, bool)>();
                    box.Value.Item1 = monoBehaviour;
                }
                UnityThread.RunNow(callback, box);
                if (!box.Value.Item2)
                    return ValueCoroutineState.Suspended;
                return ValueCoroutineState.Continue;
            }
        }
    }
}