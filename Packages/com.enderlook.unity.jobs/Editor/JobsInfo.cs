using Enderlook.Unity.Toolset.Utils;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Jobs
{
    internal sealed class JobsInfo : EditorWindow
    {
        private static readonly GUIContent TITLE_CONTENT = new GUIContent("Jobs Information", "Show jobs information.");

        private static readonly GUIContent JOB_MANAGER_HEADER_CONTENT = new GUIContent("Threading.JobManager Workload", "Show amount of work related to Unity jobs must be done.");
        private static readonly GUIContent JOB_MANAGER_HEADER_CONTENT_METHOD_TITLE = new GUIContent("Method", "Name of the method which the related workload.");
        private static readonly GUIContent JOB_MANAGER_HEADER_CONTENT_COUNT_TITLE = new GUIContent("Waiting", "Amount of workload related to that method that is being watched to complete.");

        private GUILayoutOption[] jobManagerLayoutOptionsMethod;
        private GUILayoutOption[] jobManagerLayoutOptionsCount;
        private GUILayoutOption[] jobManagerLayoutOptionScroll;

        private (int, string) jobHandleWatchCompletition = (0, "0");
        private (int, string) jobHandleOnComplete = (0, "0");
        private Vector2 jobHandleScrollPosition;

        [MenuItem("Enderlook/Jobs Information")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private static void ShowWindow()
        {
            JobsInfo window = CreateWindow<JobsInfo>();

            Rect position = window.position;
            position.width = (EditorGUIUtility.fieldWidth + EditorGUIUtility.labelWidth) * 2;
            position.height = EditorGUIUtility.singleLineHeight * 6f;
            window.position = position;
        }

        public void OnGUI()
        {
            titleContent = TITLE_CONTENT;

            CreateLayoutOptions();
            DrawJobManager();
        }

        private void CreateLayoutOptions()
        {
            if (jobManagerLayoutOptionsMethod is null)
            {
                const int min = 100;
                const int ratio = 5;
                jobManagerLayoutOptionsMethod = new GUILayoutOption[] { GUILayout.MinWidth(min - (min / ratio)), GUILayout.MaxWidth(position.width - (position.width / ratio)) };
                jobManagerLayoutOptionsCount = new GUILayoutOption[] { GUILayout.MinWidth(min / ratio), GUILayout.MaxWidth(position.width / ratio) };
                jobManagerLayoutOptionScroll = new GUILayoutOption[] { GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight * 2.5f) };
            }
        }

        private void DrawJobManager()
        {
            EditorGUIHelper.Header(JOB_MANAGER_HEADER_CONTENT);

            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(JOB_MANAGER_HEADER_CONTENT_METHOD_TITLE, EditorStyles.boldLabel, jobManagerLayoutOptionsMethod);
                    EditorGUILayout.LabelField(JOB_MANAGER_HEADER_CONTENT_COUNT_TITLE, EditorStyles.boldLabel, jobManagerLayoutOptionsCount);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUIHelper.DrawHorizontalLine();

                jobHandleScrollPosition = EditorGUILayout.BeginScrollView(jobHandleScrollPosition, jobManagerLayoutOptionScroll);
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("JobManager.WatchCompletition(JobHandle)", jobManagerLayoutOptionsMethod);
                        if (jobHandleWatchCompletition.Item1 != JobManager.JobHandleCompleterCount)
                        {
                            jobHandleWatchCompletition.Item1 = JobManager.JobHandleCompleterCount;
                            jobHandleWatchCompletition.Item2 = jobHandleWatchCompletition.Item1.ToString();
                        }
                        EditorGUILayout.LabelField(jobHandleWatchCompletition.Item2, jobManagerLayoutOptionsCount);
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField("JobManager.OnComplete(JobHandle, Action, bool)", jobManagerLayoutOptionsMethod);
                        if (jobHandleOnComplete.Item1 != JobManager.JobTasksManagerCount)
                        {
                            jobHandleOnComplete.Item1 = JobManager.JobTasksManagerCount;
                            jobHandleOnComplete.Item2 = jobHandleOnComplete.Item1.ToString();
                        }
                        EditorGUILayout.LabelField(jobHandleOnComplete.Item2, jobManagerLayoutOptionsCount);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }
    }
}