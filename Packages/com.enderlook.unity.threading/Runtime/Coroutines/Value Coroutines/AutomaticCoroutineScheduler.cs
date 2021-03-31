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
        /// <summary>
        /// Amount of miliseconds spent in executing poll coroutines per frame.
        /// </summary>
        [Tooltip("Amount of miliseconds spent in executing poll coroutines per frame.")]
        public int MilisecondsExecutedPerFrameOnPoll;

        /// <summary>
        /// Percentage of total execution that must be executed on per frame regardless of <see cref="MilisecondsExecutedPerFrameOnPoll"/>.
        /// </summary>
        [Tooltip(" Percentage of total execution that must be executed on per frame regardless of Miliseconds Executed Per Frame On Poll.")]
        public float MinimumPercentOfExecutionsPerFrameOnPoll;

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
            manager.MilisecondsExecutedPerFrameOnPoll = MilisecondsExecutedPerFrameOnPoll;
            manager.MinimumPercentOfExecutionsPerFrameOnPoll = MinimumPercentOfExecutionsPerFrameOnPoll;
            manager.OnPoll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void FixedUpdate() => manager.OnFixedUpdate();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private void OnDestroy() => manager.Dispose();
    }
}