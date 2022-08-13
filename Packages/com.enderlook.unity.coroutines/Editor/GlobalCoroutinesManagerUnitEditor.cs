using Enderlook.Unity.Toolset.Utils;

using UnityEditor;

using UnityEngine;

namespace Enderlook.Unity.Coroutines
{
    [CustomEditor(typeof(GlobalCoroutinesManagerUnit))]
    internal sealed class GlobalCoroutinesManagerUnitEditor : Editor
    {
        private SerializedProperty milisecondsExecutedPerFrameOnPoll;
        private GUIContent milisecondsExecutedPerFrameOnPollGUI;

        private SerializedProperty minimumPercentOfExecutionsPerFrameOnPoll;
        private GUIContent minimumPercentOfExecutionsPerFrameOnPollGUI;

        private void OnEnable()
        {
            milisecondsExecutedPerFrameOnPoll = serializedObject.FindProperty("milisecondsExecutedPerFrameOnPoll");
            milisecondsExecutedPerFrameOnPollGUI = new GUIContent(milisecondsExecutedPerFrameOnPoll.displayName, milisecondsExecutedPerFrameOnPoll.tooltip);
            minimumPercentOfExecutionsPerFrameOnPoll = serializedObject.FindProperty("minimumPercentOfExecutionsPerFrameOnPoll");
            minimumPercentOfExecutionsPerFrameOnPollGUI = new GUIContent(minimumPercentOfExecutionsPerFrameOnPoll.displayName, minimumPercentOfExecutionsPerFrameOnPoll.tooltip);
        }

        public override void OnInspectorGUI()
        {
            this.DrawScriptField();

            CoroutineManager shared = CoroutineManager.Shared;
            if (shared is null)
            {
                EditorGUI.BeginChangeCheck();
                {
                    EditorGUILayout.PropertyField(milisecondsExecutedPerFrameOnPoll);
                    EditorGUILayout.PropertyField(minimumPercentOfExecutionsPerFrameOnPoll);
                }
                if (EditorGUI.EndChangeCheck())
                    serializedObject.ApplyModifiedProperties();
            }
            else
            {
                int milisecondsExecutedPerFrameOnPoll = shared.MilisecondsExecutedPerFrameOnPoll;
                int newMilisecondsExecutedPerFrameOnPoll = EditorGUILayout.IntField(milisecondsExecutedPerFrameOnPollGUI, milisecondsExecutedPerFrameOnPoll);
                newMilisecondsExecutedPerFrameOnPoll = Mathf.Max(newMilisecondsExecutedPerFrameOnPoll, 0);
                if (newMilisecondsExecutedPerFrameOnPoll != milisecondsExecutedPerFrameOnPoll)
                    shared.MilisecondsExecutedPerFrameOnPoll = newMilisecondsExecutedPerFrameOnPoll;

                float minimumPercentOfExecutionsPerFrameOnPoll = shared.MinimumPercentOfExecutionsPerFrameOnPoll;
                float newMinimumPercentOfExecutionsPerFrameOnPoll = EditorGUILayout.Slider(minimumPercentOfExecutionsPerFrameOnPollGUI, minimumPercentOfExecutionsPerFrameOnPoll, 0, 1);
                if (newMinimumPercentOfExecutionsPerFrameOnPoll != minimumPercentOfExecutionsPerFrameOnPoll)
                    shared.MinimumPercentOfExecutionsPerFrameOnPoll = newMinimumPercentOfExecutionsPerFrameOnPoll;

                EditorGUILayout.HelpBox("Modifications done will not be serialized in the scriptable object.", MessageType.Warning);
            }
        }
    }
}