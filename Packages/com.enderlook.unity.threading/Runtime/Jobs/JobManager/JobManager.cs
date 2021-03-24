﻿using Enderlook.Collections.LowLevel;
using Enderlook.Threading;

using System;

using Unity.Jobs;

using UnityEditor;

namespace Enderlook.Unity.Threading.Jobs
{
    public static partial class JobManager
    {
        internal static readonly RawList<Action> updaters = RawList<Action>.Create();

        internal static void Update()
        {
            JobHandleCompleter.Update();
            JobTasksManager.Update();
            foreach (Action action in updaters)
                action();
        }

        /// <summary>
        /// Enqueues an action to be execute when the job handle <paramref name="jobHandle"/> completes.<br/>
        /// Note that this action will not be executed immediately after <c><paramref name="jobHandle"/>.Complete()</c>, but may execute on the current or next frame.
        /// </summary>
        /// <param name="jobHandle">Job handle to schedule action.</param>
        /// <param name="onJobComplete">Action to execute when job handle completes.</param>
        /// <param name="canCompleteImmediately">If the job handle is already completed, this value determines if the action should run immediately or later (which may be in this or in the next frame).</param>
        public static void OnComplete(this JobHandle jobHandle, Action onJobComplete, bool canCompleteImmediately = true)
            => JobTasksManager.Add(jobHandle, onJobComplete, canCompleteImmediately);

        /// <summary>
        /// Enqueues an action to be execute when the job handle <paramref name="jobHandle"/> completes.<br/>
        /// Note that this action will not be executed immediately after <c><paramref name="jobHandle"/>.Complete()</c>, but may execute on the current or next frame.
        /// </summary>
        /// <param name="jobHandle">Job handle to schedule action.</param>
        /// <param name="onJobComplete">Action to execute when job handle completes.</param>
        /// <param name="canCompleteImmediately">If the job handle is already completed, this value determines if the action should run immediately or later (which may be in this or in the next frame).</param>
        public static void OnComplete<TAction>(this JobHandle jobHandle, TAction onJobComplete, bool canCompleteImmediately = true)
            where TAction : IAction => JobTasksManager<TAction>.Add(jobHandle, onJobComplete, canCompleteImmediately);

        /// <summary>
        /// Automatically watches the completition of this job handle.<br/>
        /// Useful for fire and forget.
        /// </summary>
        /// <param name="jobHandle">Job handle to watch completition.</param>
        /// <returns><paramref name="jobHandle"/>.</returns>
        public static JobHandle WatchCompletition(this JobHandle jobHandle)
        {
            JobHandleCompleter.Add(jobHandle);
            return jobHandle;
        }

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void Initialize() => EditorApplication.update += Update;

        /// <summary>
        /// Unity Editor Only.
        /// </summary>
        internal static int JobHandleCompleterCount => JobHandleCompleter.Count;

        /// <summary>
        /// Unity Editor Only.
        /// </summary>
        internal static int JobTasksManagerCount => JobTasksManager.Count;

        private static RawList<Func<int>> jobTasksManagers = new RawList<Func<int>>();

        /// <summary>
        /// Unity Editor Only.
        /// </summary>
        internal static int JobTasksManagersCount {
            get {
                int total = 0;
                for (int i = 0; i < jobTasksManagers.Count; i++)
                    total += jobTasksManagers[i]();
                return total;
            }
        }
#endif
    }
}
