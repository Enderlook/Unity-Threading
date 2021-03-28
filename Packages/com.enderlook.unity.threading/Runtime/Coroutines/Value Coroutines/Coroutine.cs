using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    /// <summary>
    /// Helper methods for coroutines.
    /// </summary>
    public static partial class Coroutine
    {
        /// <summary>
        /// Start a value coroutine.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Start<T>(T routine) where T : IEnumerator<ValueYieldInstruction>
        {
            if (typeof(T).IsValueType)
                CoroutineManagers.Start(new Uncancellable(), routine);
            else
                CoroutineManagers.Start(new Uncancellable(), (IEnumerator<ValueYieldInstruction>)routine);
        }

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StartThreadSafe<T>(T routine) where T : IEnumerator<ValueYieldInstruction>
        {
            if (typeof(T).IsValueType)
                CoroutineManagers.StartThreadSafe(new Uncancellable(), routine);
            else
                CoroutineManagers.StartThreadSafe(new Uncancellable(), (IEnumerator<ValueYieldInstruction>)routine);
        }

        /// <summary>
        /// Start a value coroutine.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="cancellator">Cancellator of the coroutine.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Start<T, U>(T routine, U cancellator)
            where T : IEnumerator<ValueYieldInstruction>
            where U : ICancellable
        {
            if (typeof(T).IsValueType)
            {
                if (typeof(U).IsValueType)
                    CoroutineManagers.Start(cancellator, routine);
                else
                    CoroutineManagers.Start((ICancellable)cancellator, routine);
            }
            else
            {
                if (typeof(U).IsValueType)
                    CoroutineManagers.Start(new Uncancellable(), (IEnumerator<ValueYieldInstruction>)routine);
                else
                    CoroutineManagers.Start((ICancellable)cancellator, (IEnumerator<ValueYieldInstruction>)routine);
            }
        }

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="cancellator">Cancellator of the coroutine.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StartThreadSafe<T, U>(T routine, U cancellator)
            where T : IEnumerator<ValueYieldInstruction>
            where U : ICancellable
        {
            if (typeof(T).IsValueType)
            {
                if (typeof(U).IsValueType)
                    CoroutineManagers.StartThreadSafe(cancellator, routine);
                else
                    CoroutineManagers.StartThreadSafe((ICancellable)cancellator, routine);
            }
            else
            {
                if (typeof(U).IsValueType)
                    CoroutineManagers.StartThreadSafe(new Uncancellable(), (IEnumerator<ValueYieldInstruction>)routine);
                else
                    CoroutineManagers.StartThreadSafe((ICancellable)cancellator, (IEnumerator<ValueYieldInstruction>)routine);
            }
        }

        /// <summary>
        /// Start a value coroutine.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="source"><see cref="Object"/> from which this coroutine is attached to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Start<T>(T routine, Object source) where T : IEnumerator<ValueYieldInstruction>
        {
            if (typeof(T).IsValueType)
                CoroutineManagers.Start(new CancellableUnityObject(source), routine);
            else
                CoroutineManagers.Start(new CancellableUnityObject(source), (IEnumerator<ValueYieldInstruction>)routine);
        }

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="source"><see cref="Object"/> from which this coroutine is attached to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StartThreadSafe<T>(T routine, Object source) where T : IEnumerator<ValueYieldInstruction>
        {
            if (typeof(T).IsValueType)
                CoroutineManagers.StartThreadSafe(new CancellableUnityObject(source), routine);
            else
                CoroutineManagers.StartThreadSafe(new CancellableUnityObject(source), (IEnumerator<ValueYieldInstruction>)routine);
        }

        /// <summary>
        /// Start a value coroutine.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> of this coroutine.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Start<T>(T routine, CancellationToken cancellationToken) where T : IEnumerator<ValueYieldInstruction>
        {
            if (typeof(T).IsValueType)
                CoroutineManagers.Start(new CancellableCancellationToken(cancellationToken), routine);
            else
                CoroutineManagers.Start(new CancellableCancellationToken(cancellationToken), (IEnumerator<ValueYieldInstruction>)routine);
        }

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> of this coroutine.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StartThreadSafe<T>(T routine, CancellationToken cancellationToken) where T : IEnumerator<ValueYieldInstruction>
        {
            if (typeof(T).IsValueType)
                CoroutineManagers.StartThreadSafe(new CancellableCancellationToken(cancellationToken), routine);
            else
                CoroutineManagers.StartThreadSafe(new CancellableCancellationToken(cancellationToken), (IEnumerator<ValueYieldInstruction>)routine);
        }

        /// <summary>
        /// Start a value coroutine.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine StartWithHandle<T>(T routine) where T : IEnumerator<ValueYieldInstruction>
        {
            if (typeof(T).IsValueType)
                return CoroutineManagers.StartWithHandle(new Uncancellable(), routine);
            else
                return CoroutineManagers.StartWithHandle(new Uncancellable(), (IEnumerator<ValueYieldInstruction>)routine);
        }

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine StartWithHandleThreadSafe<T>(T routine) where T : IEnumerator<ValueYieldInstruction>
        {
            if (typeof(T).IsValueType)
                return CoroutineManagers.StartWithHandleThreadSafe(new Uncancellable(), routine);
            else
                return CoroutineManagers.StartWithHandleThreadSafe(new Uncancellable(), (IEnumerator<ValueYieldInstruction>)routine);
        }

        /// <summary>
        /// Start a value coroutine.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="source"><see cref="Object"/> from which this coroutine is attached to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine StartWithHandle<T>(T routine, Object source) where T : IEnumerator<ValueYieldInstruction>
        {
            if (typeof(T).IsValueType)
                return CoroutineManagers.StartWithHandle(new CancellableUnityObject(source), routine);
            else
                return CoroutineManagers.StartWithHandle(new CancellableUnityObject(source), (IEnumerator<ValueYieldInstruction>)routine);
        }

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="source"><see cref="Object"/> from which this coroutine is attached to.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine StartWithHandleThreadSafe<T>(T routine, Object source) where T : IEnumerator<ValueYieldInstruction>
        {
            if (typeof(T).IsValueType)
                return CoroutineManagers.StartWithHandleThreadSafe(new CancellableUnityObject(source), routine);
            else
                return CoroutineManagers.StartWithHandleThreadSafe(new CancellableUnityObject(source), (IEnumerator<ValueYieldInstruction>)routine);
        }

        /// <summary>
        /// Start a value coroutine.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> of this coroutine.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine StartWithHandle<T>(T routine, CancellationToken cancellationToken) where T : IEnumerator<ValueYieldInstruction>
        {
            if (typeof(T).IsValueType)
                return CoroutineManagers.StartWithHandle(new CancellableCancellationToken(cancellationToken), routine);
            else
                return CoroutineManagers.StartWithHandle(new CancellableCancellationToken(cancellationToken), (IEnumerator<ValueYieldInstruction>)routine);
        }

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="routine">Coroutine to start.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> of this coroutine.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine StartWithHandleThreadSafe<T>(T routine, CancellationToken cancellationToken) where T : IEnumerator<ValueYieldInstruction>
        {
            if (typeof(T).IsValueType)
                return CoroutineManagers.StartWithHandleThreadSafe(new CancellableCancellationToken(cancellationToken), routine);
            else
                return CoroutineManagers.StartWithHandleThreadSafe(new CancellableCancellationToken(cancellationToken), (IEnumerator<ValueYieldInstruction>)routine);
        }

        /// <summary>
        /// Start a value coroutine.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="source"><see cref="MonoBehaviour"/> from which this coroutine is attached to.</param>
        /// <param name="routine">Coroutine to start.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StartValueCoroutine<T>(this MonoBehaviour source, T routine) where T : IEnumerator<ValueYieldInstruction>
        {
            if (typeof(T).IsValueType)
                CoroutineManagers.Start(new CancellableUnityObject(source), routine);
            else
                CoroutineManagers.Start(new CancellableUnityObject(source), (IEnumerator<ValueYieldInstruction>)routine);
        }

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="source"><see cref="MonoBehaviour"/> from which this coroutine is attached to.</param>
        /// <param name="routine">Coroutine to start.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StartValueCoroutineThreadSafe<T>(this MonoBehaviour source, T routine) where T : IEnumerator<ValueYieldInstruction>
        {
            if (typeof(T).IsValueType)
                CoroutineManagers.StartThreadSafe(new CancellableUnityObject(source), routine);
            else
                CoroutineManagers.StartThreadSafe(new CancellableUnityObject(source), (IEnumerator<ValueYieldInstruction>)routine);
        }

        /// <summary>
        /// Start a value coroutine.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="source"><see cref="MonoBehaviour"/> from which this coroutine is attached to.</param>
        /// <param name="routine">Coroutine to start.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine StartValueCoroutineWithHandle<T>(this MonoBehaviour source, T routine) where T : IEnumerator<ValueYieldInstruction>
        {
            if (typeof(T).IsValueType)
                return CoroutineManagers.StartWithHandle(new CancellableUnityObject(source), routine);
            else
                return CoroutineManagers.StartWithHandle(new CancellableUnityObject(source), (IEnumerator<ValueYieldInstruction>)routine);
        }

        /// <summary>
        /// Start a value coroutine.<br/>
        /// This method is can be executed from any thread.
        /// </summary>
        /// <typeparam name="T">Type of routine to start.</typeparam>
        /// <param name="source"><see cref="MonoBehaviour"/> from which this coroutine is attached to.</param>
        /// <param name="routine">Coroutine to start.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueCoroutine StartValueCoroutineWithHandleThreadSafe<T>(this MonoBehaviour source, T routine) where T : IEnumerator<ValueYieldInstruction>
        {
            if (typeof(T).IsValueType)
                return CoroutineManagers.StartWithHandleThreadSafe(new CancellableUnityObject(source), routine);
            else
                return CoroutineManagers.StartWithHandleThreadSafe(new CancellableUnityObject(source), (IEnumerator<ValueYieldInstruction>)routine);
        }
    }
}