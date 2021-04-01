using Enderlook.Unity.Threading;

using System.Collections;

namespace Enderlook.Unity.Coroutines
{
    /// <summary>
    /// Helper methods for coroutines.
    /// </summary>
    public static partial class Coroutine
    {
        public static class Unity
        {
            /// <summary>
            /// Start a Unity coroutine.
            /// </summary>
            /// <param name="routine">Coroutine to start.</param>
            public static void Start<T>(T routine) where T : IEnumerator =>
                // TODO: we could optimize this for value enumerators.
                Manager.Shared.StartCoroutine(routine);
        }
    }
}