using System;
using System.Threading;
using System.Threading.Tasks;

namespace Enderlook.Unity.Threading.Tasks
{
    /// <summary>
    /// Helper methods for tasks.
    /// </summary>
    public static class UnityThreadRunner
    {
        private static readonly TaskFactory taskFactory = new TaskFactory(UnityThreadTaskScheduler.Instance);

        public static void Run(Action<object> action, object state, TaskCreationOptions creationOptions) => taskFactory.StartNew(action, state, creationOptions).GetAwaiter().GetResult();

        public static void Run(Action<object> action, object state)
            => taskFactory.StartNew(action, state).GetAwaiter().GetResult();

        public static void Run(Action action, TaskCreationOptions creationOptions)
            => taskFactory.StartNew(action, creationOptions).GetAwaiter().GetResult();

        public static void Run(Action action, CancellationToken cancellationToken)
            => taskFactory.StartNew(action, cancellationToken).GetAwaiter().GetResult();

        public static void Run(Action action)
            => taskFactory.StartNew(action).GetAwaiter().GetResult();

        public static TResult Run<TResult>(Func<TResult> function)
            => taskFactory.StartNew(function).GetAwaiter().GetResult();

        public static void Run(Action<object> action, object state, CancellationToken cancellationToken)
            => taskFactory.StartNew(action, state, cancellationToken).GetAwaiter().GetResult();
    }
}