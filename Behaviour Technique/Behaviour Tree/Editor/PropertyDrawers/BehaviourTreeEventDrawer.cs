using StateMachine.BT;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(BehaviourTreeEvent))]
public class BehaviourTreeEventDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight; // 높이를 설정
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        
        SerializedProperty bindGameobject = property.FindPropertyRelative("bindGameObject");
        SerializedProperty defaultObject = property.FindPropertyRelative("aObject");
        SerializedProperty stringMessage = property.FindPropertyRelative("message");
        position.y += EditorGUIUtility.singleLineHeight;
        
        EditorGUI.BeginChangeCheck();

        var gameobject = EditorGUI.ObjectField(position, bindGameobject.objectReferenceValue, typeof(GameObject), true);
        position.y += EditorGUIUtility.singleLineHeight;
        var dobject = EditorGUI.ObjectField(position, defaultObject.objectReferenceValue, typeof(Object), true);
        position.y += EditorGUIUtility.singleLineHeight;
        var message = EditorGUI.TextField(position, stringMessage.stringValue);

        if (EditorGUI.EndChangeCheck())
        {
            bindGameobject.objectReferenceValue = gameobject;
            defaultObject.objectReferenceValue = dobject;
            stringMessage.stringValue = message;
        }
        EditorGUI.EndProperty();
    }



    private VisualElement BuildUI(VisualElement rootElement, SerializedProperty property)
    {
        return rootElement;
    }
}
