using Enderlook.Unity.Threading;
using Enderlook.Unity.Toolset.Utils;

using System;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    internal sealed class CoroutinesInfo : EditorWindow
    {
        private static readonly GUIContent TITLE_CONTENT = new GUIContent("Coroutines Information", "Show Unity coroutines information.");
        private static readonly GUIContent WAIT_HEADER_CONTENT = new GUIContent("Coroutines.Wait Pooled Objects", "Show amount of stored pooled objects.");
        private static readonly GUIContent WAIT_HEADER_CONTENT_METHOD_TITLE = new GUIContent("Method", "Name of the method which has the pooled content.");
        private static readonly GUIContent WAIT_HEADER_CONTENT_COUNT_TITLE = new GUIContent("Count", "Amount of pooled objects that are not being used.");
        private static readonly GUIContent GLOBAL_VALUE_COROUTINE_MANAGER_TITLE = new GUIContent("Global Value Coroutine Manager", "Configuration of the global value coroutines manager.");
        private static readonly GUIContent MILISECONDS_EXECUTED_PER_FRAME_ON_POLL_CONTENT = new GUIContent("Miliseconds Executed Per Frame On Poll", "Amount of miliseconds per frame spent in executing poll coroutines.");
        private static readonly GUIContent MINIMUM_PERCENT_OF_EXECUTIONS_PER_FRAME_ON_POLL_CONTENT = new GUIContent("Minimum Percent Of Executions Per Frame On Poll", "Percentage of total executions that must be executed per frame regardless of timeout");
        private static readonly GUIContent POLL_COUNT_CONTENT = new GUIContent("Poll Count", "Amount of coroutines that are waiting for poll.");

        private static GUILayoutOption[] waitLayoutOptionsMethod;
        private static GUILayoutOption[] waitLayoutOptionsCountAndSize;
        private static GUILayoutOption[] waitLayoutOptionsScroll;

        private (int, string) waitForSeconds = (0, "0");
        private (int, string) waitForSecondsRealtime = (0, "0");
        private (int, string) waitUntilPooled = (0, "0");
        private (int, string) waitWhilePooled = (0, "0");
        private (int, string) waitForJobComplete = (0, "0");
        private (int, string) waitForTaskComplete = (0, "0");
        private (int, string) waitForValueTaskComplete = (0, "0");
        private (int, GUIContent) pollCount = (0, new GUIContent("0"));
        private Vector2 waitScrollPosition;

        [MenuItem("Enderlook/Coroutines Information")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private static void ShowWindow()
        {
            CoroutinesInfo window = CreateWindow<CoroutinesInfo>();

            Rect position = window.position;
            position.width = (EditorGUIUtility.fieldWidth + EditorGUIUtility.labelWidth) * 2;
            position.height = EditorGUIUtility.singleLineHeight * 17f;
            window.position = position;
        }

        public void OnGUI()
        {
            titleContent = TITLE_CONTENT;

            CreateLayoutOptions();
            DrawWait();
        }

        private void CreateLayoutOptions()
        {
            if (waitLayoutOptionsMethod is null)
            {
                const int min = 100;
                const int ratio1 = 5;
                waitLayoutOptionsMethod = new GUILayoutOption[] { GUILayout.MinWidth(min - (min / ratio1)), GUILayout.MaxWidth(position.width - (position.width / ratio1)) };
                waitLayoutOptionsCountAndSize = new GUILayoutOption[] { GUILayout.MinWidth(min / ratio1), GUILayout.MaxWidth(position.width / ratio1) };
                waitLayoutOptionsScroll = new GUILayoutOption[] { GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight * 8f) };
            }
        }

        private void DrawWait()
        {
            EditorGUIHelper.Header(WAIT_HEADER_CONTENT);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(WAIT_HEADER_CONTENT_METHOD_TITLE, EditorStyles.boldLabel, waitLayoutOptionsMethod);
                    EditorGUILayout.LabelField(WAIT_HEADER_CONTENT_COUNT_TITLE, EditorStyles.boldLabel, waitLayoutOptionsCountAndSize);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUIHelper.DrawHorizontalLine();

                waitScrollPosition = EditorGUILayout.BeginScrollView(waitScrollPosition, waitLayoutOptionsScroll);
                {
                    Draw("Wait.ForSeconds(float)", Wait.ForSecondsCount, ref waitForSeconds);
                    Draw("Wait.ForSecondsRealtime(float)", WaitForSecondsRealtimePooled.Count, ref waitForSecondsRealtime);
                    Draw("Wait.Until(Func<bool>)", WaitUntilPooled.Count, ref waitUntilPooled);
                    Draw("Wait.While(Func<bool>)", WaitWhilePooled.Count, ref waitWhilePooled);
                    Draw("Wait.For(JobHandle)", WaitForJobComplete.Count, ref waitForJobComplete);
                    Draw("Wait.For(Task)", WaitForTaskComplete.Count, ref waitForTaskComplete);
                    Draw("Wait.For(ValueTask)", WaitForValueTaskComplete.Count, ref waitForValueTaskComplete);

                    foreach (EditorPoolContainer container in Wait.ForTaskAndValueTaskComplete)
                    {
                        container.Get(out string name, out string count);
                        if (count == "0")
                            continue;
                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField(name, waitLayoutOptionsMethod);
                            EditorGUILayout.LabelField(count, waitLayoutOptionsCountAndSize);
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                    void Draw(string title, int count, ref (int, string) tmp)
                    {
                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField(title, waitLayoutOptionsMethod);
                            if (tmp.Item1 != count)
                            {
                                tmp.Item1 = count;
                                tmp.Item2 = count.ToString();
                            }
                            EditorGUILayout.LabelField(tmp.Item2, waitLayoutOptionsCountAndSize);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();

            EditorGUIHelper.DrawHorizontalLine();

            EditorGUIHelper.Header(GLOBAL_VALUE_COROUTINE_MANAGER_TITLE);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                CoroutineManager shared = CoroutineManager.Shared;
                if (shared is null)
                {
                    GlobalCoroutinesManagerUnit[] managers = Resources.LoadAll<GlobalCoroutinesManagerUnit>("");
                    if (managers.Length > 1)
                        Debug.LogError($"Multiple instances of {nameof(GlobalCoroutinesManagerUnit)} were found in the Resources folder.");
                    else if (managers.Length == 1)
                    {
                        GlobalCoroutinesManagerUnit manager = managers[0];
                        SerializedObject serializedObject = new SerializedObject(manager);

                        EditorGUI.BeginChangeCheck();
                        {
                            SerializedProperty milisecondsExecutedPerFrameOnPoll = serializedObject.FindProperty("milisecondsExecutedPerFrameOnPoll");
                            EditorGUILayout.PropertyField(milisecondsExecutedPerFrameOnPoll);

                            SerializedProperty minimumPercentOfExecutionsPerFrameOnPoll = serializedObject.FindProperty("minimumPercentOfExecutionsPerFrameOnPoll");
                            EditorGUILayout.PropertyField(minimumPercentOfExecutionsPerFrameOnPoll);
                        }
                        if (EditorGUI.EndChangeCheck())
                            serializedObject.ApplyModifiedProperties();
                        goto end;
                    }
                }

                EditorGUI.BeginDisabledGroup(shared is null);
                {
                    int milisecondsExecutedPerFrameOnPoll = shared?.MilisecondsExecutedPerFrameOnPoll ?? CoroutineManager.MILISECONDS_EXECUTED_PER_FRAME_ON_POLL_DEFAULT_VALUE;
                    milisecondsExecutedPerFrameOnPoll = EditorGUILayout.IntField(MILISECONDS_EXECUTED_PER_FRAME_ON_POLL_CONTENT, milisecondsExecutedPerFrameOnPoll);
                    milisecondsExecutedPerFrameOnPoll = Math.Max(milisecondsExecutedPerFrameOnPoll, 0);
                    if (!(shared is null))
                        shared.MilisecondsExecutedPerFrameOnPoll = milisecondsExecutedPerFrameOnPoll;

                    float minimumPercentOfExecutionsPerFrameOnPoll = shared?.MinimumPercentOfExecutionsPerFrameOnPoll ?? CoroutineManager.MINIMUM_PERCENT_OF_EXECUTIONS_PER_FRAME_ON_POLL_DEFAULT_VALUE;
                    minimumPercentOfExecutionsPerFrameOnPoll = EditorGUILayout.Slider(MINIMUM_PERCENT_OF_EXECUTIONS_PER_FRAME_ON_POLL_CONTENT, minimumPercentOfExecutionsPerFrameOnPoll, 0, 1);
                    minimumPercentOfExecutionsPerFrameOnPoll = Math.Max(minimumPercentOfExecutionsPerFrameOnPoll, 0);
                    if (!(shared is null))
                        shared.MinimumPercentOfExecutionsPerFrameOnPoll = minimumPercentOfExecutionsPerFrameOnPoll;

                    if (!(shared is null))
                        EditorGUILayout.HelpBox("Modifications done will not be serialized in the scriptable object.", MessageType.Warning);
                }
                EditorGUI.EndDisabledGroup();

            end:;
                int count = shared?.PollCount ?? 0;
                if (pollCount.Item1 != count)
                {
                    pollCount.Item1 = count;
                    pollCount.Item2.text = count.ToString();
                }
                EditorGUILayout.LabelField(POLL_COUNT_CONTENT, pollCount.Item2);
            }
            EditorGUILayout.EndVertical();
        }
    }
}