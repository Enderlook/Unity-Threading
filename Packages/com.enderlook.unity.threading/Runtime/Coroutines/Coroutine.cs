using Enderlook.Unity.Jobs;

using System.Collections;

namespace Enderlook.Unity.Coroutines
{
    /// <summary>
    /// Helper methods for coroutines.
    /// </summary>
    public static class Coroutine
    {
        /// <summary>
        /// Start a coroutine.
        /// </summary>
        /// <param name="routine">Coroutine to start.</param>
        public static void Start<T>(T routine) where T : IEnumerator =>
            // TODO: we could optimize this for value enumerators.
            Manager.Shared.StartCoroutine(routine);

        /// <summary>
        /// Start a coroutine.
        /// </summary>
        /// <param name="routine">Coroutine to start.</param>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static void StartAsCoroutine<T>(this T routine) where T : IEnumerator =>
            // TODO: we could optimize this for value enumerators.
            Manager.Shared.StartCoroutine(routine);
    }
}