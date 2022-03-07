using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using UnityEngine;

namespace Enderlook.Unity.Threading
{
    /// <summary>
    /// A helper class to execute actions on the Unity main thread.
    /// </summary>
    public static class UnityThread
    {
        private static readonly SendOrPostCallback callback = e =>
        {
            Debug.Assert(e is Action);
            Unsafe.As<Action>(e)();
        };

        /// <summary>
        /// Synchronization context used by Unity.
        /// </summary>
        public static SynchronizationContext UnitySynchronizationContext => unitySynchronizationContext;
        private static readonly SynchronizationContext unitySynchronizationContext = UnitySynchronizationContextUtility.UnitySynchronizationContext;

        /// <summary>
        /// Thread Id used by Unity main thread.
        /// </summary>
        public static int UnityThreadId => unityThreadId;
        private static readonly int unityThreadId = UnitySynchronizationContextUtility.UnityThreadId;

        /// <summary>
        /// Task Scheduler used by Unity.
        /// </summary>
        private static readonly TaskScheduler unityTaskScheduler = UnitySynchronizationContextUtility.UnityTaskScheduler;
        public static TaskScheduler UnityTaskScheduler => unityTaskScheduler;

        /// <summary>
        /// A <see cref="TaskFactory"/> where all tasks are run on the main (Unity) thread by using <see cref="UnityTaskScheduler"/>.
        /// </summary>
        public static TaskFactory Factory => factory;
        private static readonly TaskFactory factory = new TaskFactory(UnitySynchronizationContextUtility.UnityTaskScheduler);

        /// <summary>
        /// Check if we are currently running in main (Unity) thread.
        /// </summary>
        /// <returns>Whenever we are running in main thread or not.</returns>
        public static bool IsMainThread => Thread.CurrentThread.ManagedThreadId == UnityThreadId;

        /// <summary>
        /// Determines if we are in the Unity synchronization context.
        /// </summary>
        public static bool IsUnitySynchronizationContext => SynchronizationContext.Current == unitySynchronizationContext;

        /// <summary>
        /// Subscribe delegates to execute in the Unity thread on each frame.
        /// </summary>
        public static event Action OnUpdate {
            add => Manager.OnUpdate += value;
            remove => Manager.OnUpdate -= value;
        }

        /// <summary>
        /// Subscribe delegates to execute in the Unity thread on each physics update.
        /// </summary>
        public static event Action OnFixedUpdate
        {
            add => Manager.OnFixedUpdate += value;
            remove => Manager.OnFixedUpdate -= value;
        }

        /// <summary>
        /// Subscribe delegates to execute in the Unity thread on each frame after update calls are executed.
        /// </summary>
        public static event Action OnLateUpdate
        {
            add => Manager.OnLateUpdate += value;
            remove => Manager.OnLateUpdate -= value;
        }

        /// <summary>
        /// Subscribe delegates to execute in the Unity thread on each end of frame.
        /// </summary>
        public static event Action OnEndOfFrame
        {
            add => Manager.OnEndOfFrame += value;
            remove => Manager.OnEndOfFrame -= value;
        }

        /// <summary>
        /// Executes the specified action on the Unity thread.<br/>
        /// The action will not be executed instantaneously, but later.
        /// </summary>
        /// <param name="action">Action to execute on the main thread.</param>
        public static void RunLater(Action action)
            => unitySynchronizationContext.Post(callback, action);

        /// <summary>
        /// Executes the specified action on the Unity thread.<br/>
        /// The action will not be executed instantaneously, but later.
        /// </summary>
        /// <param name="action">Action to execute on the main thread.</param>
        /// <param name="state">State of the action.</param>
        public static void RunLater(SendOrPostCallback action, object state)
            => unitySynchronizationContext.Post(action, state);

        /// <summary>
        /// Executes the specified action on the Unity thread.<br/>
        /// The action will not be executed instantaneously, but later.
        /// </summary>
        /// <typeparam name="T">Type of the state.</typeparam>
        /// <param name="action">Action to execute on the main thread.</param>
        /// <param name="state">State of the action.</param>
        public static void RunLater<T>(Action<T> action, T state)
        {
            if (typeof(T).IsValueType)
                unitySynchronizationContext.Post(ActionHelper<T>.ExecuteAndReturn, Tuple2<Action<T>, T>.Rent(action, state));
            else
                unitySynchronizationContext.Post(Unsafe.As<SendOrPostCallback>(action), state);
        }

        /// <summary>
        /// Executes the specified action on the Unity thread.<br/>
        /// The action will be immediately and this thread will wait until completion.
        /// </summary>
        /// <param name="action">Action to execute on the main thread.</param>
        public static void RunNow(Action action)
            => unitySynchronizationContext.Send(callback, action);

        /// <summary>
        /// Executes the specified action on the Unity thread.<br/>
        /// The action will be immediately and this thread will wait until completion.
        /// </summary>
        /// <param name="action">Action to execute on the main thread.</param>
        /// <param name="state">State of the action.</param>
        public static void RunNow(SendOrPostCallback action, object state)
            => unitySynchronizationContext.Send(action, state);

        /// <summary>
        /// Executes the specified action on the Unity thread.<br/>
        /// The action will be immediately and this thread will wait until completion.
        /// </summary>
        /// <typeparam name="T">Type of the state.</typeparam>
        /// <param name="action">Action to execute on the main thread.</param>
        /// <param name="state">State of the action.</param>
        public static void RunNow<T>(Action<T> action, T state)
        {
            if (typeof(T).IsValueType)
                unitySynchronizationContext.Send(ActionHelper<T>.ExecuteAndReturn, Tuple2<Action<T>, T>.Rent(action, state));
            else
                unitySynchronizationContext.Send(Unsafe.As<SendOrPostCallback>(action), state);
        }

        /// <summary>
        /// Executes the specified action on the Unity thread.<br/>
        /// The action will be immediately and this thread will wait until completion.
        /// </summary>
        /// <typeparam name="T">Type of the returned value.</typeparam>
        /// <param name="function">Action to execute on the main thread.</param>
        /// <returns>Return value of <paramref name="function"/>.</returns>
        public static T RunNow<T>(Func<T> function)
        {
            Tuple2<Func<T>, T> tuple = Tuple2<Func<T>, T>.Rent(function);
            unitySynchronizationContext.Send(FuncHelper<T>.Execute, tuple);
            T value = tuple.Item2;
            tuple.Item1 = null;
            // TODO: In .Net Standard 2.1 we can use RuntimeHelper.IsReferenceOrContainsReferences<T>().
            tuple.Item2 = default;
            tuple.Return();
            return value;
        }

        /// <summary>
        /// Executes the specified action on the Unity thread.<br/>
        /// The action will be immediately and this thread will wait until completion.
        /// </summary>
        /// <typeparam name="T">Type of the state.</typeparam>
        /// <typeparam name="U">Type of the returned value.</typeparam>
        /// <param name="function">Action to execute on the main thread.</param>
        /// <param name="state">State of the action.</param>
        /// <returns>Return value of <paramref name="function"/>.</returns>
        public static U RunNow<T, U>(Func<T, U> function, T state)
        {
            Tuple3<Func<T, U>, T, U> tuple = Tuple3<Func<T, U>, T, U>.Rent(function, state);
            unitySynchronizationContext.Send(FuncHelper<T, U>.Execute, tuple);
            U value = tuple.Item3;
            tuple.Item1 = null;
            // TODO: In .Net Standard 2.1 we can use RuntimeHelper.IsReferenceOrContainsReferences<T>().
            tuple.Item2 = default;
            tuple.Item3 = default;
            tuple.Return();
            return value;
        }

        private static class ActionHelper<T>
        {
            public static readonly SendOrPostCallback ExecuteAndReturn = e =>
            {
                Debug.Assert(e is Tuple2<Action<T>, T>);
                Tuple2<Action<T>, T> tuple = Unsafe.As<Tuple2<Action<T>, T>>(e);
                Action<T> action = tuple.Item1;
                T state = tuple.Item2;
                tuple.Return();
                action(state);
            };
        }

        private static class FuncHelper<T>
        {
            public static readonly SendOrPostCallback Execute = e =>
            {
                Debug.Assert(e is Tuple2<Func<T>, T>);
                Tuple2<Func<T>, T> tuple = Unsafe.As<Tuple2<Func<T>, T>>(e);
                tuple.Item2 = tuple.Item1();
            };
        }

        private static class FuncHelper<T, U>
        {
            public static readonly SendOrPostCallback Execute = e =>
            {
                Debug.Assert(e is Tuple3<Func<T, U>, T, U>);
                Tuple3<Func<T, U>, T, U> tuple = Unsafe.As<Tuple3<Func<T, U>, T, U>>(e);
                tuple.Item3 = tuple.Item1(tuple.Item2);
            };
        }
    }
}
