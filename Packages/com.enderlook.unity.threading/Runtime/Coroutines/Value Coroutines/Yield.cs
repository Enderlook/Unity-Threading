﻿using Enderlook.Unity.Jobs;

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
        /// Amount of miliseconds spent in executing global poll coroutines per frame.
        /// </summary>
        public static int MilisecondsExecutedPerFrameOnPoll {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Manager.Shared.MilisecondsExecutedPerFrameOnPoll;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Manager.Shared.MilisecondsExecutedPerFrameOnPoll = value;
        }

        /// <summary>
        /// Percentage of total execution that must be executed on per frame regardless of <see cref="MilisecondsExecutedPerFrameOnPoll"/> for global poll coroutines.
        /// </summary>
        public static float MinimumPercentOfExecutionsPerFrameOnPoll {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Manager.Shared.MinimumPercentOfExecutionsPerFrameOnPoll;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Manager.Shared.MinimumPercentOfExecutionsPerFrameOnPoll = value;
        }

        /// <summary>
        /// Suspend coroutine execution until next update.
        /// </summary>
        public static ValueYieldInstruction ToUpdate {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new ValueYieldInstruction() { Mode = ValueYieldInstruction.Type.ForUpdate };
        }

        /// <summary>
        /// Suspend coroutine execution after the next update is executed.
        /// </summary>
        public static ValueYieldInstruction ToLateUpdate {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new ValueYieldInstruction() { Mode = ValueYieldInstruction.Type.ForLateUpdate };
        }

        /// <summary>
        /// Suspend coroutine execution until next fixed update.
        /// </summary>
        public static ValueYieldInstruction ToFixedUpdate {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new ValueYieldInstruction() { Mode = ValueYieldInstruction.Type.ForFixedUpdate };
        }

        /// <summary>
        /// Suspend coroutine execution until end of frame.
        /// </summary>
        public static ValueYieldInstruction ToEndOfFrame {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new ValueYieldInstruction() { Mode = ValueYieldInstruction.Type.ForEndOfFrame };
        }

        /// <summary>
        /// Suspend coroutine execution until reach Unity thread.
        /// </summary>
        public static ValueYieldInstruction ToUnity {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new ValueYieldInstruction() { Mode = ValueYieldInstruction.Type.ToUnity };
        }

        /// <summary>
        /// Suspend coroutine execution until reach a background thread.
        /// </summary>
        public static ValueYieldInstruction ToBackground {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new ValueYieldInstruction() { Mode = ValueYieldInstruction.Type.ToBackground };
        }

        /// <summary>
        /// Suspend coroutine execution until reach a long background thread.
        /// </summary>
        public static ValueYieldInstruction ToLongBackground {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new ValueYieldInstruction() { Mode = ValueYieldInstruction.Type.ToLongBackground };
        }

        /// <summary>
        /// Suspend coroutine execution if the frame is delayed.
        /// </summary>
        public static ValueYieldInstruction Poll {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new ValueYieldInstruction() { Mode = ValueYieldInstruction.Type.Poll };
        }

        /// <summary>
        /// Suspend coroutine execution during the given amount of seconds.
        /// </summary>
        /// <param name="seconds">Amount of seconds to wait.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueYieldInstruction ForSeconds(float seconds)
            => new ValueYieldInstruction() { Mode = ValueYieldInstruction.Type.ForSeconds, Float = seconds };

        /// <summary>
        /// Suspend coroutine execution during the given amount of realtime seconds.
        /// </summary>
        /// <param name="seconds">Amount of seconds to wait.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueYieldInstruction ForRealtimeSeconds(float seconds)
            => new ValueYieldInstruction() { Mode = ValueYieldInstruction.Type.ForRealtimeSeconds, Float = seconds };

        /// <summary>
        /// Suspend coroutine execution while <paramref name="predicate"/> returns <see langword="true"/>.
        /// </summary>
        /// <param name="predicate">Predicate to check.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueYieldInstruction While(Func<bool> predicate)
            => new ValueYieldInstruction() { Mode = ValueYieldInstruction.Type.While, FuncBool = predicate };

        /// <summary>
        /// Suspend coroutine execution until <paramref name="predicate"/> returns <see langword="true"/>.
        /// </summary>
        /// <param name="predicate">Predicate to check.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueYieldInstruction Until(Func<bool> predicate)
            => new ValueYieldInstruction() { Mode = ValueYieldInstruction.Type.Until, FuncBool = predicate };

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
#if UNITY_EDITOR
            if (coroutine is IEnumerator<ValueYieldInstruction>)
                Debug.LogError($"Using {nameof(Yield)}.{nameof(From)}({nameof(IEnumerator)}).\n"
                    + $"But concrete type is {nameof(IEnumerator<ValueYieldInstruction>)}.\n"
                    + $"Downcast {nameof(IEnumerator)} to {nameof(IEnumerator<ValueYieldInstruction>)} or the coroutine will fail in undefined behaviour.\n"
                    + "This becomes a silent error outside Unity Editor.");
#endif
            return new ValueYieldInstruction() { Mode = ValueYieldInstruction.Type.BoxedEnumerator, BoxedEnumerator = coroutine };
        }
    }
}