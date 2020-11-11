using Enderlook.Threading;

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

        /// <inheritdoc cref="TaskFactory.StartNew(Action, TaskCreationOptions)"/>
        public static void Run(Action action, TaskCreationOptions creationOptions)
            => taskFactory.StartNew(action, creationOptions).GetAwaiter().GetResult();

        /// <inheritdoc cref="TaskFactory.StartNew(Action, CancellationToken)"/>
        public static void Run(Action action, CancellationToken cancellationToken)
            => taskFactory.StartNew(action, cancellationToken).GetAwaiter().GetResult();

        /// <inheritdoc cref="TaskFactory.StartNew(Action)"/>
        public static void Run(Action action)
            => taskFactory.StartNew(action).GetAwaiter().GetResult();

        /// <inheritdoc cref="TaskFactory.StartNew(Action{object}, object, TaskCreationOptions)"/>
        public static void Run(Action<object> action, object state, TaskCreationOptions creationOptions)
            => taskFactory.StartNew(action, state, creationOptions).GetAwaiter().GetResult();

        /// <inheritdoc cref="TaskFactory.StartNew(Action{object}, object)"/>
        public static void Run(Action<object> action, object state)
            => taskFactory.StartNew(action, state).GetAwaiter().GetResult();

        /// <inheritdoc cref="TaskFactory.StartNew(Action{object}, object, CancellationToken)"/>
        public static void Run(Action<object> action, object state, CancellationToken cancellationToken)
            => taskFactory.StartNew(action, state, cancellationToken).GetAwaiter().GetResult();

        /// <inheritdoc cref="TaskFactoryExtension.StartNew{TState}(TaskFactory, Action{TState}, TState, TaskCreationOptions)"/>
        public static void Run<TState>(Action<TState> action, TState state, TaskCreationOptions creationOptions)
            => taskFactory.StartNew(action, state, creationOptions).GetAwaiter().GetResult();

        /// <inheritdoc cref="TaskFactoryExtension.StartNew{TState}(TaskFactory, Action{TState}, TState)"/>
        public static void Run<TState>(Action<TState> action, TState state)
            => taskFactory.StartNew(action, state).GetAwaiter().GetResult();

        /// <inheritdoc cref="TaskFactoryExtension.StartNew{TState}(TaskFactory, Action{TState}, TState, CancellationToken)"/>
        public static void Run<TState>(Action<TState> action, TState state, CancellationToken cancellationToken)
            => taskFactory.StartNew(action, state, cancellationToken).GetAwaiter().GetResult();

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{TResult})"/>
        public static TResult Run<TResult>(Func<TResult> function)
            => taskFactory.StartNew(function).GetAwaiter().GetResult();

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object)"/>
        public static TResult Run<TResult>(Func<object, TResult> function, object state)
            => taskFactory.StartNew(function, state).GetAwaiter().GetResult();

        /// <inheritdoc cref="TaskFactoryExtension.StartNew{TState, TResult}(TaskFactory, Func{TState, TResult}, TState)"/>
        public static TResult Run<TState, TResult>(Func<TState, TResult> function, TState state)
            => taskFactory.StartNew(function, state).GetAwaiter().GetResult();

        /// <inheritdoc cref="TaskFactoryExtension.StartNew{TAction, TState}(TaskFactory, TAction, TaskCreationOptions)"/>
        public static void Run<TAction>(TAction action, TaskCreationOptions creationOptions)
            where TAction : IAction
            => taskFactory.StartNew(action, creationOptions).GetAwaiter().GetResult();

        /// <inheritdoc cref="TaskFactoryExtension.StartNew{TAction, TState}(TaskFactory, TAction, CancellationToken)"/>
        public static void Run<TAction>(TAction action, CancellationToken cancellationToken)
            where TAction : IAction
            => taskFactory.StartNew(action, cancellationToken).GetAwaiter().GetResult();

        /// <inheritdoc cref="TaskFactoryExtension.StartNew{TAction, TState}(TaskFactory, TAction)"/>
        public static void Run<TAction>(TAction action)
            where TAction : IAction
            => taskFactory.StartNew(action).GetAwaiter().GetResult();

        /// <inheritdoc cref="TaskFactoryExtension.StartNew{TAction, TState}(TaskFactory, TAction, TState, TaskCreationOptions)"/>
        public static void Run<TAction, TState>(TAction action, TState state, TaskCreationOptions creationOptions)
            where TAction : IAction<TState>
            => taskFactory.StartNew(action, state, creationOptions).GetAwaiter().GetResult();

        /// <inheritdoc cref="TaskFactoryExtension.StartNew{TAction, TState}(TaskFactory, TAction, TState)"/>
        public static void Run<TAction, TState>(TAction action, TState state)
            where TAction : IAction<TState>
            => taskFactory.StartNew(action, state).GetAwaiter().GetResult();

        /// <inheritdoc cref="TaskFactoryExtension.StartNew{TAction, TState}(TaskFactory, TAction, CancellationToken)"/>
        public static void Run<TAction, TState>(TAction action, TState state, CancellationToken cancellationToken)
            where TAction : IAction<TState>
            => taskFactory.StartNew(action, state, cancellationToken).GetAwaiter().GetResult();

        /// <inheritdoc cref="TaskFactoryExtension.StartNew{TFunc, TResult}(TaskFactory, TFunc)"/>
        public static TResult Run<TFunc, TResult>(TFunc function)
            where TFunc : IFunc<TResult>
            => taskFactory.StartNew<TFunc, TResult>(function).GetAwaiter().GetResult();

        /// <inheritdoc cref="TaskFactoryExtension.StartNew{TFunc, TState, TResult}(TaskFactory, TFunc, TState)"/>
        public static TResult Run<TFunc, TState, TResult>(TFunc function, TState state)
            where TFunc : IFunc<TState, TResult>
            => taskFactory.StartNew<TFunc, TState, TResult>(function, state).GetAwaiter().GetResult();
    }
}