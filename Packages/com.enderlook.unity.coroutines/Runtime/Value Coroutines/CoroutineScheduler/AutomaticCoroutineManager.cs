using System.Collections;
using System.Threading.Tasks;

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
        private CoroutineManager manager;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void Awake()
        {
            manager = new CoroutineManager(this);
            StartCoroutine(Work());
            IEnumerator Work()
            {
                while (true)
                {
                    yield return Wait.ForEndOfFrame;
                    manager.OnEndOfFrame();
                }
            }
            if (Application.platform != RuntimePlatform.WebGLPlayer)
            {
                Task.Factory.StartNew(async () =>
                {
                    while (manager != null)
                    {
                        manager.OnBackground();
                        await Task.Delay(5);
                    }
                }, TaskCreationOptions.LongRunning);
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
        private void OnDestroy()
        {
            CoroutineManager m = manager;
            manager = null;
            m.Dispose();
        }
    }
}