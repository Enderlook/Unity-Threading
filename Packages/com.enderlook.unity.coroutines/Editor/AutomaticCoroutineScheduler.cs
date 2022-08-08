using UnityEditor;

namespace Enderlook.Unity.Coroutines
{
    [CustomEditor(typeof(AutomaticCoroutineScheduler))]
    internal sealed class AutomaticCoroutineSchedulerEditor : Editor
    {
        private SerializedProperty milisecondsExecutedPerFrameOnPoll;
        private SerializedProperty minimumPercentOfExecutionsPerFrameOnPoll;

        private void OnEnable()
        {
            SerializedProperty manager = serializedObject.FindProperty("manager");
            milisecondsExecutedPerFrameOnPoll = manager.FindPropertyRelative("milisecondsExecutedPerFrameOnPoll");
            minimumPercentOfExecutionsPerFrameOnPoll = manager.FindPropertyRelative("minimumPercentOfExecutionsPerFrameOnPoll");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(milisecondsExecutedPerFrameOnPoll);
            EditorGUILayout.PropertyField(minimumPercentOfExecutionsPerFrameOnPoll);
            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        }
    }
}