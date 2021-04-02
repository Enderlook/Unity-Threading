using System.Collections;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    /// <summary>
    /// A wrapper arround <see cref="CoroutineScheduler"/> to automatically execute its events.
    /// </summary>
    [AddComponentMenu("Enderlook/Automatic Coroutines Manager")] // Not show in menu
    [DefaultExecutionOrder(int.MaxValue)]
    public sealed class AutomaticCoroutineScheduler : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        private CoroutineScheduler manager;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            manager = CoroutineScheduler.Create(this);
            StartCoroutine(Work());
            IEnumerator Work()
            {
                while (true)
                {
                    yield return Wait.ForEndOfFrame;
                    manager.OnEndOfFrame();
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Update() => manager.OnUpdate();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void LateUpdate()
        {
            manager.OnLateUpdate();
            manager.OnPoll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void FixedUpdate() => manager.OnFixedUpdate();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void OnDestroy() => manager.Dispose();
    }
}