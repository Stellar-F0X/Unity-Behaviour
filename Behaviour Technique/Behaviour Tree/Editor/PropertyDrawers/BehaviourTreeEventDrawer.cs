using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace BehaviourTechnique.BehaviourTreeEditor
{
    [CustomPropertyDrawer(typeof(BehaviourTreeEvent))]
    public class BehaviourTreeEventDrawer : PropertyDrawer
    {
        private BehaviourActor _behaviourActor;
        private SerializedObject _serializedRuntimeTree;
        private SerializedProperty _serializedEvent;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (this.CachingSerializedRuntimeTreeElement())
            {
                float height = EditorGUIUtility.singleLineHeight;
                height += EditorGUI.GetPropertyHeight(_serializedEvent.FindPropertyRelative("onEnterEvent"));
                
                height += EditorGUIUtility.standardVerticalSpacing;
                height += EditorGUI.GetPropertyHeight(_serializedEvent.FindPropertyRelative("onUpdateEvent"));
                
                height += EditorGUIUtility.standardVerticalSpacing;
                height += EditorGUI.GetPropertyHeight(_serializedEvent.FindPropertyRelative("onExitEvent"));
                return height;
            }
            else
            {
                return EditorGUIUtility.standardVerticalSpacing;
            }
        }


        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (this.CachingSerializedRuntimeTreeElement())
            {
                using (var scope = new EditorGUI.PropertyScope(position, label, property))
                {
                    EditorGUI.BeginChangeCheck();

                    position.y += EditorGUIUtility.singleLineHeight;
                    
                    this.CreatePropertyField(ref position, "onEnterEvent");
                    this.CreatePropertyField(ref position, "onUpdateEvent");
                    this.CreatePropertyField(ref position, "onExitEvent");

                    if (EditorGUI.EndChangeCheck())
                    {
                        _serializedEvent.serializedObject.ApplyModifiedProperties();
                    }
                }
            }
            else
            {
                EditorGUI.LabelField(position, "The event requires the BehaviourTreeActor in Scene");
            }
        }


        private bool CachingSerializedRuntimeTreeElement()
        {
            _behaviourActor = _behaviourActor ?? Resources.FindObjectsOfTypeAll<BehaviourActor>().First(actor => {
                return ReferenceEquals(actor.runtimeTree, BehaviourTreeEditorWindow.editorWindow?.tree) && 
                       !ReferenceEquals(actor.runtimeTree, null);
            });
            
            if (! ReferenceEquals(_behaviourActor, null))
            {
                _serializedRuntimeTree ??= new SerializedObject(_behaviourActor);
                _serializedEvent ??= _serializedRuntimeTree.FindProperty("behaviourEvents");
                return true;
            }

            return false;
        }


        private void CreatePropertyField(ref Rect position, string findPropertyName)
        {
            SerializedProperty property = _serializedEvent.FindPropertyRelative(findPropertyName);
            float spacingHeigth = EditorGUI.GetPropertyHeight(property);
            EditorGUI.PropertyField(position, property);

            position.y += spacingHeigth + EditorGUIUtility.standardVerticalSpacing;
            position.height = spacingHeigth;
        }
    }
}
