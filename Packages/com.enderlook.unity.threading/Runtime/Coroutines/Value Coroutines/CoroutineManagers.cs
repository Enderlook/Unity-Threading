using Enderlook.Collections.LowLevel;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    internal static partial class CoroutineManagers
    {
        public static readonly MonoBehaviour monoBehaviour;
        private static Action onUpdate;
        private static Action onLateUpdate;
        private static Action onFixedUpdate;
        private static Action onEndOfFrame;
        private static RawList<Func<int, bool>> onPoll = RawList<Func<int, bool>>.Create();
        private static int onPollKey;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Start<T, U>(U cancellator, T routine)
            where T : IEnumerator<ValueYieldInstruction>
            where U : ICancellable
            => CoroutineManager<T, U>.Start(cancellator, routine);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine StartWithHandle<T, U>(U cancellator, T routine)
           where T : IEnumerator<ValueYieldInstruction>
           where U : ICancellable
            => ValueCoroutine.Start(cancellator, routine);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine StartWithHandleThreadSafe<T, U>(U cancellator, T routine)
           where T : IEnumerator<ValueYieldInstruction>
           where U : ICancellable
            => ValueCoroutine.StartThreadSafe(cancellator, routine);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StartThreadSafe<T, U>(U cancellator, T routine)
            where T : IEnumerator<ValueYieldInstruction>
            where U : ICancellable
            => CoroutineManager<T, U>.StartThreadSafe(cancellator, routine);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnUpdate() => onUpdate?.Invoke();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnLateUpdate() => onLateUpdate?.Invoke();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnFixedUpdate() => onFixedUpdate?.Invoke();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnEndOfFrame() => onEndOfFrame?.Invoke();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnPoll()
        {
            int until = DateTime.Now.Millisecond + Yield.MilisecondsExecutedPerFrameOnPoll;
            bool work;
            do
            {
                work = false;
                while (Interlocked.Exchange(ref onPollKey, 1) != 0);
                for (int i = 0; i < onPoll.Count; i++)
                    work |= onPoll[i].Invoke(until);
                Interlocked.Exchange(ref onPollKey, 0);
            } while (until > DateTime.Now.Millisecond && work);
        }
    }
}