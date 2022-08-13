using System;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    /// <summary>
    /// Represent the global settings of the <see cref="CoroutineManager.Shared"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "Global Coroutines Manager Configuration", menuName = "Enderlook/Coroutines/Global Coroutines Manager Unit")]
    internal sealed class GlobalCoroutinesManagerUnit : ScriptableObject
    {
        // Keep names in sync with GlobalCoroutinesManagerUnitEditor and UnityCoroutinesPoolInfo

        [SerializeField, Min(0), Tooltip("Amount of miliseconds spent in executing poll coroutines.")]
        private int milisecondsExecutedPerFrameOnPoll = CoroutineManager.MILISECONDS_EXECUTED_PER_FRAME_ON_POLL_DEFAULT_VALUE;

        [SerializeField, Range(0, 1), Tooltip("Percentage of total executions that must be executed on poll coroutines regardless of timeout.")]
        private float minimumPercentOfExecutionsPerFrameOnPoll = CoroutineManager.MINIMUM_PERCENT_OF_EXECUTIONS_PER_FRAME_ON_POLL_DEFAULT_VALUE;

        public static void Load()
        {
            GlobalCoroutinesManagerUnit[] managers = Resources.LoadAll<GlobalCoroutinesManagerUnit>("");
            if (managers.Length > 1)
                throw new InvalidOperationException($"Multiple instances of {nameof(GlobalCoroutinesManagerUnit)} were found in the Resources folder.");
            else if (managers.Length == 1)
            {
                GlobalCoroutinesManagerUnit manager = managers[0];
                CoroutineManager.Shared.MilisecondsExecutedPerFrameOnPoll = manager.milisecondsExecutedPerFrameOnPoll;
                CoroutineManager.Shared.MinimumPercentOfExecutionsPerFrameOnPoll = manager.minimumPercentOfExecutionsPerFrameOnPoll;
            }
        }
    }
}