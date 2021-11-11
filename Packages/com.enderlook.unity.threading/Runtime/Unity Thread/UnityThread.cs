using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Enderlook.Unity.Threading
{
    /// <summary>
    /// A helper class to execute actions on the Unity main thread.
    /// </summary>
    public static class UnityThread
    {
        private static readonly SendOrPostCallback callback = (obj) => Unsafe.As<Action>(obj)();
        private static readonly SendOrPostCallback callback2 = (obj) => Unsafe.As<Executor>(obj).Execute();

        /// <summary>
        /// Synchronization context used by Unity.
        /// </summary>
        public static readonly SynchronizationContext UnitySynchronizationContext = UnitySynchronizationContextUtility.UnitySynchronizationContext;

        /// <summary>
        /// Thread Id used by Unity main thread.
        /// </summary>
        public static readonly int UnityThreadId = UnitySynchronizationContextUtility.UnityThreadId;

        /// <summary>
        /// Task Scheduler used by Unity.
        /// </summary>
        public static readonly TaskScheduler UnityTaskScheduler = UnitySynchronizationContextUtility.UnityTaskScheduler;

        /// <summary>
        /// A <see cref="TaskFactory"/> where all tasks are run on the main (Unity) thread by using <see cref="UnityTaskScheduler"/>.
        /// </summary>
        public static readonly TaskFactory Factory = new TaskFactory(UnityTaskScheduler);

        /// <summary>
        /// Check if we are currently running in main (Unity) thread.
        /// </summary>
        /// <returns>Whenever we are running in main thread or not.</returns>
        public static bool IsMainThread => Thread.CurrentThread.ManagedThreadId == UnityThreadId;

        /// <summary>
        /// Determines if we are in the Unity synchronization context.
        /// </summary>
        public static bool IsUnitySynchronizationContext => SynchronizationContext.Current == UnitySynchronizationContext;

        /// <summary>
        /// Executes the specified action on the Unity thread.<br/>
        /// The action will not be executed instantaneously, but later.
        /// </summary>
        /// <param name="action">Action to execute on the main thread.</param>
        public static void RunLater(Action action)
            => UnitySynchronizationContextUtility.UnitySynchronizationContext.Post(callback, action);

        /// <summary>
        /// Executes the specified action on the Unity thread.<br/>
        /// The action will not be executed instantaneously, but later.
        /// </summary>
        /// <param name="action">Action to execute on the main thread.</param>
        /// <param name="state">State of the action.</param>
        public static void RunLater(SendOrPostCallback action, object state)
            => UnitySynchronizationContextUtility.UnitySynchronizationContext.Post(action, state);

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
                UnitySynchronizationContextUtility.UnitySynchronizationContext.Post(callback2, StrongBox<T>.Get(action, state));
            else
                UnitySynchronizationContextUtility.UnitySynchronizationContext.Post(Unsafe.As<SendOrPostCallback>(action), state);
        }

        /// <summary>
        /// Executes the specified action on the Unity thread.<br/>
        /// The action will be immediately and this thread will wait until completition.
        /// </summary>
        /// <param name="action">Action to execute on the main thread.</param>
        public static void RunNow(Action action)
            => UnitySynchronizationContextUtility.UnitySynchronizationContext.Send(callback, action);

        /// <summary>
        /// Executes the specified action on the Unity thread.<br/>
        /// The action will be immediately and this thread will wait until completition.
        /// </summary>
        /// <param name="action">Action to execute on the main thread.</param>
        /// <param name="state">State of the action.</param>
        public static void RunNow(SendOrPostCallback action, object state)
            => UnitySynchronizationContextUtility.UnitySynchronizationContext.Send(action, state);

        /// <summary>
        /// Executes the specified action on the Unity thread.<br/>
        /// The action will be immediately and this thread will wait until completition.
        /// </summary>
        /// <typeparam name="T">Type of the state.</typeparam>
        /// <param name="action">Action to execute on the main thread.</param>
        /// <param name="state">State of the action.</param>
        public static void RunNow<T>(Action<T> action, T state)
        {
            if (typeof(T).IsValueType)
                UnitySynchronizationContextUtility.UnitySynchronizationContext.Send(callback2, StrongBox<T>.Get(action, state));
            else
                UnitySynchronizationContextUtility.UnitySynchronizationContext.Send(Unsafe.As<SendOrPostCallback>(action), state);
        }

        /// <summary>
        /// Executes the specified action on the Unity thread.<br/>
        /// The action will be immediately and this thread will wait until completition.
        /// </summary>
        /// <typeparam name="T">Type of the returned value.</typeparam>
        /// <param name="action">Action to execute on the main thread.</param>
        /// <returns>Return value of <paramref name="action"/>.</returns>
        public static T RunNow<T>(Func<T> action)
        {
            BoxWithReturn<T> box = BoxWithReturn<T>.Get(action);
            UnitySynchronizationContextUtility.UnitySynchronizationContext.Send(callback2, box);
            T value = box.value;
            box.Return();
            return value;
        }

        /// <summary>
        /// Executes the specified action on the Unity thread.<br/>
        /// The action will be immediately and this thread will wait until completition.
        /// </summary>
        /// <typeparam name="T">Type of the state.</typeparam>
        /// <typeparam name="U">Type of the returned value.</typeparam>
        /// <param name="action">Action to execute on the main thread.</param>
        /// <param name="state">State of the action.</param>
        /// <returns>Return value of <paramref name="action"/>.</returns>
        public static U RunNow<T, U>(Func<T, U> action, T state)
        {
            BoxWithReturn<T, U> box = BoxWithReturn<T, U>.Get(action, state);
            UnitySynchronizationContextUtility.UnitySynchronizationContext.Send(callback2, box);
            U value = box.value;
            box.Return();
            return value;
        }

        private abstract class Executor
        {
            public abstract void Execute();
        }

        private sealed class StrongBox<T> : Executor
        {
            private Action<T> action;
            private T value;

            public override void Execute()
            {
                Action<T> a = action;
                T v = value;
                action = null;
                value = default; // TODO: In .Net Standard 2.1 use RuntimeHelpers.IsReferenceOrContainsReferences<T>
                ConcurrentPool.Return(this);
                action(value);
            }

            public static StrongBox<T> Get(Action<T> action, T value)
            {
                StrongBox<T> box = ConcurrentPool.Rent<StrongBox<T>>();
                box.action = action;
                box.value = value;
                return box;
            }
        }

        private sealed class BoxWithReturn<T> : Executor
        {
            private Func<T> action;
            public T value;

            public override void Execute() => value = action();

            public void Return()
            {
                action = null;
                value = default; // TODO: In .Net Standard 2.1 use RuntimeHelpers.IsReferenceOrContainsReferences<T>
                ConcurrentPool.Return(this);
            }

            public static BoxWithReturn<T> Get(Func<T> action)
            {
                BoxWithReturn<T> box = ConcurrentPool.Rent<BoxWithReturn<T>>();
                box.action = action;
                return box;
            }
        }

        private sealed class BoxWithReturn<T, U>
        {
            private Func<T, U> action;
            private T parameter;
            public U value;

            public void Execute() => value = action(parameter);

            public void Return()
            {
                action = null;
                parameter = default; // TODO: In .Net Standard 2.1 use RuntimeHelpers.IsReferenceOrContainsReferences<T>
                value = default; // TODO: In .Net Standard 2.1 use RuntimeHelpers.IsReferenceOrContainsReferences<T>
                ConcurrentPool.Return(this);
            }

            public static BoxWithReturn<T, U> Get(Func<T, U> action, T parameter)
            {
                BoxWithReturn<T, U> box = ConcurrentPool.Rent<BoxWithReturn<T, U>>();
                box.action = action;
                box.parameter = parameter;
                return box;
            }
        }
    }
}
