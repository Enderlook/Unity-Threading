﻿using Enderlook.Threading;

using System;
using System.Collections.Generic;

using Unity.Jobs;

using UnityEditor;

namespace Enderlook.Unity.Threading.Jobs
{
    public static partial class JobManager
    {
        private static JobTasksManager actionTaskManager = new JobTasksManager();

        private static Dictionary<Type, (object Instance, Delegate Add, Action Update)> managers = new Dictionary<Type, (object Instance, Delegate Add, Action Update)>();

        private static object[] parameters = new object[3];

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void Initialize() => EditorApplication.update += Update;
#endif

        internal static void Update()
        {
            JobHandleCompleter.Update();
            actionTaskManager.Update();
            foreach ((object Instance, Delegate Add, Action Update) manager in managers.Values)
                manager.Update();
        }

        /// <summary>
        /// Enqueues an action to be execute when the job handle <paramref name="jobHandle"/> completes.<br/>
        /// Note that this action will not be executed immediately after <c><paramref name="jobHandle"/>.Complete()</c>, but may execute on the current or next frame.
        /// </summary>
        /// <param name="jobHandle">Job handle to schedule action.</param>
        /// <param name="onJobComplete">Action to execute when job handle completes.</param>
        /// <param name="canCompleteImmediately">If the job handle is already completed, this value determines if the action should run immediately or later (which may be in this or in the next frame).</param>
        public static void OnComplete(this JobHandle jobHandle, Action onJobComplete, bool canCompleteImmediately = true)
            => actionTaskManager.Add(jobHandle, onJobComplete, canCompleteImmediately);

        /// <summary>
        /// Enqueues an action to be execute when the job handle <paramref name="jobHandle"/> completes.<br/>
        /// Note that this action will not be executed immediately after <c><paramref name="jobHandle"/>.Complete()</c>, but may execute on the current or next frame.
        /// </summary>
        /// <param name="jobHandle">Job handle to schedule action.</param>
        /// <param name="onJobComplete">Action to execute when job handle completes.</param>
        /// <param name="canCompleteImmediately">If the job handle is already completed, this value determines if the action should run immediately or later (which may be in this or in the next frame).</param>
        public static void OnComplete<TAction>(this JobHandle jobHandle, TAction onJobComplete, bool canCompleteImmediately = true)
            where TAction : IAction
        {
            if (!managers.TryGetValue(typeof(TAction), out (object Instance, Delegate Add, Action Update) value))
            {
                JobTasksManager<TAction> instance = new JobTasksManager<TAction>();
                value.Instance = instance;
                value.Add = (Action<JobHandle, TAction, bool>)instance.Add;
                value.Update = instance.Update;
            }
            parameters[0] = jobHandle;
            parameters[1] = onJobComplete;
            parameters[2] = canCompleteImmediately;
            value.Add.DynamicInvoke(parameters);
        }

        /// <summary>
        /// Automatically watches the completition of this job handle.<br/>
        /// Useful for fire and forget.
        /// </summary>
        /// <param name="jobHandle">Job handle to watch completition.</param>
        public static void WatchCompletition(this JobHandle jobHandle)
            => JobHandleCompleter.Add(jobHandle);

        /// <summary>
        /// Schedules and automatically watches the completition of this job.<br/>
        /// Useful for fire and forget.
        /// </summary>
        /// <typeparam name="T">Type of job.</typeparam>
        /// <param name="job">Job to schedule and watch.</param>
        /// <param name="dependsOn">Another job that must be executed before executing <paramref name="job"/>.</param>
        /// <returns>Job handle of the scheduled task.</returns>
        public static JobHandle ScheduleAndWatch<T>(this T job, JobHandle dependsOn = default)
            where T : unmanaged, IJob
        {
            JobHandle jobHandle = job.Schedule(dependsOn);
            jobHandle.WatchCompletition();
            return jobHandle;
        }
    }
}
