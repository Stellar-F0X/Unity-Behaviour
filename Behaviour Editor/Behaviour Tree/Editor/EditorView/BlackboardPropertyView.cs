using System;
using BehaviourSystem.BT;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BehaviourSystemEditor.BT
{
    public class BlackboardPropertyView : VisualElement
    {
        public BlackboardPropertyView(SerializedProperty property, VisualTreeAsset visualTreeAsset)
        {
            visualTreeAsset.CloneTree(this);

            this._property        = property;
            this._keyOfProperty   = property.FindPropertyRelative("_key");
            this._valueOfProperty = property.FindPropertyRelative("_value");

            this._imguiContainer = this.Q<IMGUIContainer>("IMGUIContainer");
            this._keyField       = this.Q<TextField>("name-field");
            this._button         = this.Q<Button>("delete-button");
            
            this.RegisterButtonEvent(() =>
            {
                this._keyField.UnregisterValueChangedCallback(this.OnKeyFieldChanged);
                this._imguiContainer.onGUIHandler -= this.DrawInspectorGUI;
            });

            this._keyField.value = _keyOfProperty.stringValue;
            this._keyField.RegisterValueChangedCallback(this.OnKeyFieldChanged);
            this._imguiContainer.onGUIHandler += this.DrawInspectorGUI;
        }
        
        private SerializedProperty _property;
        private SerializedProperty _keyOfProperty;
        private SerializedProperty _valueOfProperty;

        private IMGUIContainer _imguiContainer;
        private TextField      _keyField;
        private Button         _button;


        public SerializedProperty property
        {
            get { return this._property; }
        }

        public bool activeDeleteButton
        {
            get { return _button.enabledSelf; }
            set { _button.enabledSelf = value; }
        } 
        
        
        public void RegisterButtonEvent(Action action)
        {
            this._button.clicked += action;
        }


        public void UnregisterButtonEvent(Action action)
        {
            this._button.clicked -= action;
        }


        private void DrawInspectorGUI()
        {
            GUI.enabled = false;
            EditorGUILayout.PropertyField(_valueOfProperty, GUIContent.none);
            GUI.enabled = true;
        }


        private void OnKeyFieldChanged(ChangeEvent<string> evt)
        {
            if (string.IsNullOrEmpty(evt.newValue))
            {
                _keyOfProperty.stringValue = string.Empty;
            }
            else
            {
                _keyOfProperty.stringValue = evt.newValue;
            }

            _keyOfProperty.serializedObject.ApplyModifiedProperties();
        }
    }
}