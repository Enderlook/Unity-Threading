using UnityEditor;

namespace Enderlook.Unity.Coroutines
{
    [CustomEditor(typeof(AutomaticCoroutineScheduler))]
    internal sealed class AutomaticCoroutineSchedulerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            SerializedProperty core = serializedObject.FindProperty("manager").FindPropertyRelative("core");
            SerializedProperty milisecondsExecutedPerFrameOnPoll = core.FindPropertyRelative("milisecondsExecutedPerFrameOnPoll");
            SerializedProperty minimumPercentOfExecutionsPerFrameOnPoll = core.FindPropertyRelative("minimumPercentOfExecutionsPerFrameOnPoll");

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(milisecondsExecutedPerFrameOnPoll);
            EditorGUILayout.PropertyField(minimumPercentOfExecutionsPerFrameOnPoll);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        }
    }
}