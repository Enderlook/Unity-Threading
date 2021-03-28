using System.Threading;

namespace Enderlook.Unity.Coroutines
{
    /// <summary>
    /// Cancelator of value coroutines.
    /// </summary>
    public interface ICancellable
    {
        /// <summary>
        /// Whenever the coroutine was requested to stop.
        /// </summary>
        bool IsCancelationRequested { get; }
    }

    /// <summary>
    /// An cancellable that will never request cancellation.
    /// </summary>
    internal readonly struct Uncancellable : ICancellable
    {
        /// <inheritdoc cref="ICancellable.IsCancelationRequested"/>
        public bool IsCancelationRequested => false;
    }

    /// <summary>
    /// Adaptor of <see cref="UnityEngine.Object"/> to <see cref="ICancellable"/>.
    /// </summary>
    internal readonly struct CancellableUnityObject : ICancellable
    {
        private readonly UnityEngine.Object obj;

        public CancellableUnityObject(UnityEngine.Object obj) => this.obj = obj;

        /// <inheritdoc cref="ICancellable.IsCancelationRequested"/>
        public bool IsCancelationRequested => obj == null;
    }

    /// <summary>
    /// Adaptor of <see cref="CancellationToken"/> to <see cref="ICancellable"/>.
    /// </summary>
    internal readonly struct CancellableCancellationToken : ICancellable
    {
        private readonly CancellationToken token;

        public CancellableCancellationToken(CancellationToken token) => this.token = token;

        /// <inheritdoc cref="ICancellable.IsCancelationRequested"/>
        public bool IsCancelationRequested => token.IsCancellationRequested;
    }
}