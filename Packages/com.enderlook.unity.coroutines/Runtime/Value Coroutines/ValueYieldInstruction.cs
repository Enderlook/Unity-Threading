using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Unity.Jobs;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    /// <summary>
    /// Represent a yield instruction for value coroutines.
    /// </summary>
    public struct ValueYieldInstruction
    {
        private static readonly FieldInfo waitForSecondsTimer = typeof(WaitForSeconds).GetField("m_Seconds", BindingFlags.NonPublic | BindingFlags.Instance);

        internal Type Mode;
        private object obj;
        private Union union;

        [StructLayout(LayoutKind.Explicit, Size = 16)]
        private struct Union
        {
            [FieldOffset(0)]
            public float Float;
            [FieldOffset(0)]
            public ValueTask ValueTask;
            [FieldOffset(0)]
            public JobHandle JobHandle;
            [FieldOffset(0)]
            public uint UInt;
        }

        internal Func<bool> FuncBool {
            get => (Func<bool>)obj;
            set => obj = value;
        }

        internal YieldInstruction YieldInstruction {
            get => (YieldInstruction)obj;
            set => obj = value;
        }

        internal CustomYieldInstruction CustomYieldInstruction {
            get => (CustomYieldInstruction)obj;
            set => obj = value;
        }

        internal IEnumerator<ValueYieldInstruction> ValueEnumerator {
            get => (IEnumerator<ValueYieldInstruction>)obj;
            set => obj = value;
        }

        internal IValueCoroutineEnumerator IValueCoroutine {
            get => (IValueCoroutineEnumerator)obj;
            set => obj = value;
        }

        internal IEnumerator UnityEnumerator {
            get => (IEnumerator)obj;
            set => obj = value;
        }

        internal Coroutine UnityCoroutine {
            get => (Coroutine)obj;
            set => obj = value;
        }

        internal float Float {
            get => union.Float;
            set => union.Float = value;
        }

        internal ValueTask ValueTask {
            get => union.ValueTask;
            set => union.ValueTask = value;
        }

        internal JobHandle JobHandle {
            get => union.JobHandle;
            set => union.JobHandle = value;
        }

        internal ValueCoroutine ValueCoroutine {
            get => new ValueCoroutine((ValueCoroutine.Handle)obj, union.UInt);
            set {
                obj = value.handle;
                union.UInt = value.generation;
            }
        }

        internal enum Type
        {
            ToUpdate = 0,
            BackgroundPoll,
            CustomYieldInstruction,
            IValueCoroutine,
            JobHandle,
            ToBackground,
            ToEndOfFrame,
            ToLateUpdate,
            ToLongBackground,
            ToFixedUpdate,
            ToUnity,
            ForRealtimeSeconds,
            ForSeconds,
            UnityCoroutine,
            UnityEnumerator,
            UnityPoll,
            Until,
            While,
            ValueEnumerator,
            ValueTask,
            YieldInstruction,
            ValueCoroutine,
            Finalized,
            Suspended,
        }

        /// <summary>
        /// Convert a <see cref="WaitForUpdate"/> into a <see cref="ValueYieldInstruction"/>.
        /// </summary>
        /// <param name="source">Yield instruction.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ValueYieldInstruction(WaitForUpdate source)
        {
#if DEBUG
            if (source is null)
                Debug.LogWarning($"{nameof(source)} is null. Note that this function won't fail if {nameof(source)} is null.");
#endif
            return new ValueYieldInstruction() { Mode = Type.ToUpdate };
        }

        /// <summary>
        /// Convert a <see cref="WaitForFixedUpdate"/> into a <see cref="ValueYieldInstruction"/>.
        /// </summary>
        /// <param name="source">Yield instruction.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ValueYieldInstruction(WaitForFixedUpdate source)
        {
#if DEBUG
            if (source is null)
                Debug.LogWarning($"{nameof(source)} is null. Note that this function won't fail if {nameof(source)} is null.");
#endif
            return new ValueYieldInstruction() { Mode = Type.ToFixedUpdate };
        }

        /// <summary>
        /// Convert a <see cref="WaitForSeconds"/> into a <see cref="ValueYieldInstruction"/>.
        /// </summary>
        /// <param name="source">Yield instruction.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ValueYieldInstruction(WaitForSeconds source)
        {
            try
            {
                return new ValueYieldInstruction() { Mode = Type.ForSeconds, Float = (float)waitForSecondsTimer.GetValue(source) + Time.time };
            }
            catch (ArgumentNullException)
            {
                ThrowSourceIsNull();
                return default;
            }
        }

        /// <summary>
        /// Convert a <see cref="WaitForSecondsRealtime"/> into a <see cref="ValueYieldInstruction"/>.
        /// </summary>
        /// <param name="source">Yield instruction.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ValueYieldInstruction(WaitForSecondsRealtime source)
        {
            try
            {
                return new ValueYieldInstruction() { Mode = Type.ForRealtimeSeconds, Float = source.waitTime + Time.time };
            }
            catch (NullReferenceException)
            {
                ThrowSourceIsNull();
                return default;
            }
        }

        /// <summary>
        /// Convert a <see cref="WaitForSecondsRealtimePooled"/> into a <see cref="ValueYieldInstruction"/>.
        /// </summary>
        /// <param name="source">Yield instruction.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ValueYieldInstruction(WaitForSecondsRealtimePooled source)
        {
            try
            {
                return new ValueYieldInstruction() { Mode = Type.ForRealtimeSeconds, Float = source.waitUntil };
            }
            catch (NullReferenceException)
            {
                ThrowSourceIsNull();
                return default;
            }
        }

        /// <summary>
        /// Convert a <see cref="WaitForEndOfFrame"/> into a <see cref="ValueYieldInstruction"/>.
        /// </summary>
        /// <param name="source">Yield instruction.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ValueYieldInstruction(WaitForEndOfFrame source)
        {
#if DEBUG
            if (source is null)
                Debug.LogWarning($"{nameof(source)} is null. Note that this function won't fail if {nameof(source)} is null.");
#endif
            return new ValueYieldInstruction() { Mode = Type.ToEndOfFrame };
        }

        /// <summary>
        /// Convert a <see cref="CustomYieldInstruction"/> into a <see cref="ValueYieldInstruction"/>.
        /// </summary>
        /// <param name="source">Yield instruction.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ValueYieldInstruction(CustomYieldInstruction source)
        {
            if (source is null)
                ThrowSourceIsNull();
            return new ValueYieldInstruction() { Mode = Type.CustomYieldInstruction, CustomYieldInstruction = source };
        }

        /// <summary>
        /// Convert a <see cref="ValueTask"/> into a <see cref="ValueYieldInstruction"/>.
        /// </summary>
        /// <param name="source">Yield instruction.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ValueYieldInstruction(ValueTask source)
            => new ValueYieldInstruction() { Mode = Type.ValueTask, ValueTask = source };

        /// <summary>
        /// Convert a <see cref="ValueTask"/> into a <see cref="ValueYieldInstruction"/>.
        /// </summary>
        /// <param name="source">Yield instruction.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ValueYieldInstruction(Task source)
            => new ValueYieldInstruction() { Mode = Type.ValueTask, ValueTask = new ValueTask(source) };

        /// <summary>
        /// Convert a <see cref="JobHandle"/> into a <see cref="ValueYieldInstruction"/>.
        /// </summary>
        /// <param name="source">Yield instruction.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ValueYieldInstruction(JobHandle source)
            => new ValueYieldInstruction() { Mode = Type.JobHandle, JobHandle = source };

        /// <summary>
        /// Convert a <see cref="ValueCoroutine"/> into a <see cref="ValueYieldInstruction"/>.
        /// </summary>
        /// <param name="source">Yield instruction.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ValueYieldInstruction(ValueCoroutine source)
            => new ValueYieldInstruction() { Mode = Type.ValueCoroutine, ValueCoroutine = source };

        /// <summary>
        /// Convert a <see cref="Coroutine"/> into a <see cref="ValueYieldInstruction"/>.
        /// </summary>
        /// <param name="source">Yield instruction.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ValueYieldInstruction(Coroutine source)
            => new ValueYieldInstruction() { Mode = Type.UnityCoroutine, UnityCoroutine = source };

        /// <summary>
        /// Convert a <see cref="YieldInstruction"/> into a <see cref="ValueYieldInstruction"/>.
        /// </summary>
        /// <param name="source">Yield instruction.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ValueYieldInstruction(YieldInstruction source)
        {
            if (source is null)
                ThrowSourceIsNull();
#if DEBUG
            if (source is WaitForFixedUpdate)
                Debug.LogWarning($"Using implicit conversion from {nameof(UnityEngine.YieldInstruction)} to {nameof(ValueYieldInstruction)}.\n"
                    + $"But concrete type is {nameof(WaitForFixedUpdate)}.\n"
                    + $"Consider downcasting {nameof(UnityEngine.YieldInstruction)} to {nameof(WaitForFixedUpdate)} and later to {nameof(ValueYieldInstruction)} in order to avoid boxing.");
            else if (source is WaitForEndOfFrame)
                Debug.LogWarning($"Using implicit conversion from {nameof(UnityEngine.YieldInstruction)} to {nameof(ValueYieldInstruction)}.\n"
                    + $"But concrete type is {nameof(WaitForEndOfFrame)}.\n"
                    + $"Consider downcasting {nameof(UnityEngine.YieldInstruction)} to {nameof(WaitForEndOfFrame)} and later to {nameof(ValueYieldInstruction)} in order to avoid boxing.");
#endif
            return new ValueYieldInstruction() { Mode = Type.YieldInstruction, YieldInstruction = source };
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ThrowSourceIsNull() => throw new ArgumentNullException("source");

        [MethodImpl(MethodImplOptions.AggressiveInlining), System.Diagnostics.Conditional("DEBUG")]
        internal void DebugAssertIsAllowedInEnumerators()
        {
#if DEBUG
            switch (Mode)
            {
                case Type.Finalized:
                    Debug.LogError($"A value coroutine that implements {nameof(IEnumerator<ValueYieldInstruction>)} (or {nameof(IEnumerator)}) yielded {nameof(Yield)}.{nameof(Yield.Finalized)}. This is undefined behaviour.");
                    break;
                case Type.Suspended:
                    Debug.LogError($"A value coroutine that implements {nameof(IEnumerator<ValueYieldInstruction>)} (or {nameof(IEnumerator)}) yielded {nameof(Yield)}.{nameof(Yield.Suspended)}. This is undefined behaviour.");
                    break;
            }
#endif
        }
    }
}