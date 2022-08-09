using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    /// <summary>
    /// Provides a pool of waiters for value coroutines.
    /// </summary>
    public static class Yield
    {
        /// <summary>
        /// Executes the code in a background thread.<br/>
        /// If the platform doesn't support multithreading, this fallback to <see cref="Poll"/>.
        /// </summary>
        public static ValueYieldInstruction BackgroundPoll {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new ValueYieldInstruction() { Mode = ValueYieldInstruction.Type.BackgroundPoll };
        }

        /// <summary>
        /// Determines that coroutine has finalized (or has been cancelled).<br/>
        /// After this value, all yielded values from the coroutine must also be <see cref="Finalized"/>.<br/>
        /// Coroutines that implements <see cref="IEnumerator{T}"/> (or <see cref="IEnumerator"/>) instead of <see cref="IValueCoroutineEnumerator"/> shall not return this value or will result in undefined behaviour.
        /// </summary>
        internal static ValueYieldInstruction Finalized {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new ValueYieldInstruction() { Mode = ValueYieldInstruction.Type.Finalized };
        }

        /// <summary>
        /// Suspend coroutine execution during the given amount of realtime seconds.
        /// </summary>
        /// <param name="seconds">Amount of seconds to wait.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueYieldInstruction ForRealtimeSeconds(float seconds)
            => new ValueYieldInstruction() { Mode = ValueYieldInstruction.Type.ForRealtimeSeconds, Float = seconds };

        /// <summary>
        /// Suspend coroutine execution during the given amount of seconds.
        /// </summary>
        /// <param name="seconds">Amount of seconds to wait.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueYieldInstruction ForSeconds(float seconds)
            => new ValueYieldInstruction() { Mode = ValueYieldInstruction.Type.ForSeconds, Float = seconds };

        /// <summary>
        /// Suspend coroutine execution until the <paramref name="coroutine"/> finalizes.
        /// </summary>
        /// <param name="coroutine">Coroutine to run.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueYieldInstruction From(IEnumerator<ValueYieldInstruction> coroutine)
            => new ValueYieldInstruction() { Mode = ValueYieldInstruction.Type.ValueEnumerator, ValueEnumerator = coroutine };

        /// <summary>
        /// Suspend coroutine execution until the <paramref name="coroutine"/> finalizes.
        /// </summary>
        /// <param name="coroutine">Coroutine to run.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueYieldInstruction From(IEnumerator coroutine)
        {
#if DEBUG
            if (coroutine is IEnumerator<ValueYieldInstruction>)
                Debug.LogError($"Using {nameof(Yield)}.{nameof(From)}({nameof(IEnumerator)}).\n"
                    + $"But concrete type is {nameof(IEnumerator<ValueYieldInstruction>)}.\n"
                    + $"Downcast {nameof(IEnumerator)} to {nameof(IEnumerator<ValueYieldInstruction>)} or the coroutine will fail in undefined behaviour.");
#endif
            return new ValueYieldInstruction() { Mode = ValueYieldInstruction.Type.UnityEnumerator, UnityEnumerator = coroutine };
        }

        /// <summary>
        /// Suspend coroutine execution until the <paramref name="coroutine"/> finalizes.
        /// </summary>
        /// <param name="coroutine">Coroutine to run.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static ValueYieldInstruction From(IValueCoroutineEnumerator coroutine)
            => new ValueYieldInstruction() { Mode = ValueYieldInstruction.Type.IValueCoroutine, IValueCoroutine = coroutine };

        /// <summary>
        /// Suspend coroutine execution if the frame is delayed.<br/>
        /// Code is run in the main thread.
        /// </summary>
        public static ValueYieldInstruction Poll {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new ValueYieldInstruction() { Mode = ValueYieldInstruction.Type.UnityPoll };
        }

        /// <summary>
        /// Determines that coroutine is supended.<br/>
        /// The coroutine must yield this value as long as it's suspended.<br/>
        /// Coroutines that implements <see cref="IEnumerator{T}"/> (or <see cref="IEnumerator"/>) instead of <see cref="IValueCoroutineEnumerator"/> shall not return this value or will result in undefined behaviour.
        /// </summary>
        internal static ValueYieldInstruction Suspended {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new ValueYieldInstruction() { Mode = ValueYieldInstruction.Type.Suspended };
        }

        /// <summary>
        /// Suspend coroutine execution until reach a background thread.
        /// </summary>
        public static ValueYieldInstruction ToBackground {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new ValueYieldInstruction() { Mode = ValueYieldInstruction.Type.ToBackground };
        }

        /// <summary>
        /// Suspend coroutine execution until end of frame.
        /// </summary>
        public static ValueYieldInstruction ToEndOfFrame {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new ValueYieldInstruction() { Mode = ValueYieldInstruction.Type.ToEndOfFrame };
        }

        /// <summary>
        /// Suspend coroutine execution until next fixed update.
        /// </summary>
        public static ValueYieldInstruction ToFixedUpdate {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new ValueYieldInstruction() { Mode = ValueYieldInstruction.Type.ToFixedUpdate };
        }

        /// <summary>
        /// Suspend coroutine execution after the next update is executed.
        /// </summary>
        public static ValueYieldInstruction ToLateUpdate {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new ValueYieldInstruction() { Mode = ValueYieldInstruction.Type.ToLateUpdate };
        }

        /// <summary>
        /// Suspend coroutine execution until reach a long background thread.
        /// </summary>
        public static ValueYieldInstruction ToLongBackground {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new ValueYieldInstruction() { Mode = ValueYieldInstruction.Type.ToLongBackground };
        }

        /// <summary>
        /// Suspend coroutine execution until reach Unity thread.
        /// </summary>
        public static ValueYieldInstruction ToUnity {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new ValueYieldInstruction() { Mode = ValueYieldInstruction.Type.ToUnity };
        }

        /// <summary>
        /// Suspend coroutine execution until next update.
        /// </summary>
        public static ValueYieldInstruction ToUpdate {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new ValueYieldInstruction() { Mode = ValueYieldInstruction.Type.ToUpdate };
        }

        /// <summary>
        /// Suspend coroutine execution until <paramref name="predicate"/> returns <see langword="true"/>.
        /// </summary>
        /// <param name="predicate">Predicate to check.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueYieldInstruction Until(Func<bool> predicate)
            => new ValueYieldInstruction() { Mode = ValueYieldInstruction.Type.Until, FuncBool = predicate };

        /// <summary>
        /// Suspend coroutine execution while <paramref name="predicate"/> returns <see langword="true"/>.
        /// </summary>
        /// <param name="predicate">Predicate to check.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueYieldInstruction While(Func<bool> predicate)
            => new ValueYieldInstruction() { Mode = ValueYieldInstruction.Type.While, FuncBool = predicate };
    }
}