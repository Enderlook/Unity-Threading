using Enderlook.Unity.Threading;
using Enderlook.Unity.Toolset.Utils;

using System;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    internal sealed class CoroutinesViewer : EditorWindow
    {
        private static readonly GUIContent TITLE_CONTENT = new GUIContent("Coroutines Viewer", "Show coroutines information.");
        private static readonly GUIContent WAIT_HEADER_CONTENT = new GUIContent("Coroutines.Wait Pooled Objects", "Show amount of stored pooled objects.");
        private static readonly GUIContent WAIT_HEADER_CONTENT_METHOD_TITLE = new GUIContent("Method", "Name of the method which has the pooled content.");
        private static readonly GUIContent WAIT_HEADER_CONTENT_COUNT_TITLE = new GUIContent("Count", "Amount of pooled objects that are not being used.");
        private static readonly GUIContent WAIT_HEADER_CONTENT_CLEAR_TITLE = new GUIContent("Clear", "Clear the unused object of the pool.");

        private GUILayoutOption[] waitLayoutOptionsMethod;
        private GUILayoutOption[] waitLayoutOptionsCountAndSize;
        private GUILayoutOption[] waitLayoutOptionsClear;
        private GUILayoutOption[] waitLayoutOptionsScroll;

        private (int, string) waitForSeconds = (0, "0");
        private (int, string) waitForSecondsRealtime = (0, "0");
        private (int, string) waitUntilPooled = (0, "0");
        private (int, string) waitWhilePooled = (0, "0");
        private (int, string) waitForJobComplete = (0, "0");
        private (int, string) waitForTaskComplete = (0, "0");
        private (int, string) waitForValueTaskComplete = (0, "0");
        private Vector2 waitScrollPosition;

        [MenuItem("Enderlook/Coroutines Viewer")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0051:Remove unused private members", Justification = "Used by Unity.")]
        private static void ShowWindow()
        {
            CoroutinesViewer window = CreateWindow<CoroutinesViewer>();

            Rect position = window.position;
            position.width = (EditorGUIUtility.fieldWidth + EditorGUIUtility.labelWidth) * 2;
            position.height = EditorGUIUtility.singleLineHeight * 19.5f;
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
                const int ratio2 = 6;
                waitLayoutOptionsMethod = new GUILayoutOption[] { GUILayout.MinWidth(min - (min / ratio1) - (min / ratio2)), GUILayout.MaxWidth(position.width - (position.width / ratio1) - (position.width / ratio2)) };
                waitLayoutOptionsCountAndSize = new GUILayoutOption[] { GUILayout.MinWidth(min / ratio1), GUILayout.MaxWidth(position.width / ratio1) };
                waitLayoutOptionsClear = new GUILayoutOption[] { GUILayout.MinWidth(min / ratio2), GUILayout.MaxWidth(position.width / ratio2) };
                waitLayoutOptionsScroll = new GUILayoutOption[] { GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight * 9f) };
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
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField(WAIT_HEADER_CONTENT_COUNT_TITLE, EditorStyles.boldLabel, waitLayoutOptionsCountAndSize);
                        EditorGUILayout.LabelField(WAIT_HEADER_CONTENT_CLEAR_TITLE, EditorStyles.boldLabel, waitLayoutOptionsClear);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUIHelper.DrawHorizontalLine();

                waitScrollPosition = EditorGUILayout.BeginScrollView(waitScrollPosition, waitLayoutOptionsScroll);
                {
                    if (Draw("Wait.ForSeconds(float)", Wait.ForSecondsCount, ref waitForSeconds))
                        Wait.ForSecondsClear();
                    if (Draw("Wait.ForSecondsRealtime(float)", WaitForSecondsRealtimePooled.Count, ref waitForSecondsRealtime))
                        WaitForSecondsRealtimePooled.Clear();
                    if (Draw("Wait.Until(Func<bool>)", WaitUntilPooled.Count, ref waitUntilPooled))
                        WaitUntilPooled.Clear();
                    if (Draw("Wait.While(Func<bool>)", WaitWhilePooled.Count, ref waitWhilePooled))
                        WaitWhilePooled.Clear();
                    if (Draw("Wait.For(JobHandle)", WaitForJobComplete.Count, ref waitForJobComplete))
                        WaitForJobComplete.Clear();
                    if (Draw("Wait.For(Task)", WaitForTaskComplete.Count, ref waitForTaskComplete))
                        WaitForTaskComplete.Clear();
                    if (Draw("Wait.For(ValueTask)", WaitForValueTaskComplete.Count, ref waitForValueTaskComplete))
                        WaitForValueTaskComplete.Clear();

                    foreach (EditorPoolContainer container in Wait.ForTaskAndValueTaskComplete)
                    {
                        container.Get(out string name, out Action clear, out string count);
                        if (count == "0")
                            continue;
                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField(name, waitLayoutOptionsMethod);
                            EditorGUILayout.LabelField(count, waitLayoutOptionsCountAndSize);
                            if (GUILayout.Button("Clear", waitLayoutOptionsClear))
                                clear();
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                    bool Draw(string title, int count, ref (int, string) tmp)
                    {
                        bool value;
                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField(title, waitLayoutOptionsMethod);
                            if (tmp.Item1 != count)
                            {
                                tmp.Item2 = count.ToString();
                            }
                            EditorGUILayout.LabelField(tmp.Item2, waitLayoutOptionsCountAndSize);
                            value = GUILayout.Button("Clear", waitLayoutOptionsClear);
                        }
                        EditorGUILayout.EndHorizontal();
                        return value;
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }
    }
}