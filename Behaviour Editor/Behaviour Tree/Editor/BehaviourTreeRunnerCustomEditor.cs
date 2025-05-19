using BehaviourSystem.BT;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourSystemEditor.BT
{
    [CustomEditor(typeof(BehaviourTreeRunner))]
    public class BehaviourTreeRunnerCustomEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (target is BehaviourTreeRunner runner)
            {
                SerializedProperty runtimeTreeField = serializedObject.FindProperty("_runtimeTree");
                runtimeTreeField.objectReferenceValue = EditorGUILayout.ObjectField("Tree Asset", runner.runtimeTree, typeof(BehaviourTree), false);
                
                runner.useFixedUpdate = EditorGUILayout.Toggle("Use Fixed Update", runner.useFixedUpdate);
                runner.useGizmos = EditorGUILayout.Toggle("Use Gizmos Update", runner.useGizmos);

                EditorGUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();

                runner.useUpdateRate = EditorGUILayout.Toggle("Use Update Rate", runner.useUpdateRate);

                if (runner.useUpdateRate)
                {
                    int maxFPS = Application.targetFrameRate > 0 ? Application.targetFrameRate : (int)BehaviourTreeEditor.Settings.maxUpdateRate;
                    runner.updateRate = EditorGUILayout.IntSlider(runner.updateRate, 1, maxFPS);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.HelpBox($"This node executes with a time interval of {1f / runner.updateRate:F3} seconds.", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.EndHorizontal();
                }
                
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}