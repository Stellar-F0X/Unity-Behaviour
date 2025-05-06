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
        public BlackboardPropertyView(IBlackboardProperty property, VisualTreeAsset visualTreeAsset)
        {
            visualTreeAsset.CloneTree(this);

            this._property       = property;
            this._imguiContainer = this.Q<IMGUIContainer>("IMGUIContainer");
            this._keyField       = this.Q<TextField>("name-field");
            this._button         = this.Q<Button>("delete-button");

            this._button.clicked += () => this.onRemove.Invoke(this);
            this._button.clicked += this.OnViewRemove;

            this._keyField.value = property.key;
            this._keyField.RegisterValueChangedCallback(this.OnKeyFieldChanged);
            this._imguiContainer.onGUIHandler = this.DrawInspectorGUI;
        }

        public event Action<BlackboardPropertyView> onRemove = delegate { };

        private IBlackboardProperty _property;
        private IMGUIContainer _imguiContainer;
        private TextField _keyField;
        private Button _button;


        public IBlackboardProperty property
        {
            get { return this._property; }
        }



        private void OnViewRemove()
        {
            this._keyField.UnregisterValueChangedCallback(this.OnKeyFieldChanged);
            this._imguiContainer.onGUIHandler =  null;
            this._button.clicked              -= this.OnViewRemove;
        }


        private void DrawInspectorGUI()
        {
            switch (_property.propertyType)
            {
                case EBlackboardPropertyType.Int:
                    GUI.enabled = false;
                    var intProperty = (BlackboardProperty<int>)_property;
                    EditorGUILayout.IntField(intProperty.value);
                    GUI.enabled = true;
                    break;

                case EBlackboardPropertyType.Float:
                    GUI.enabled = false;
                    var floatProperty = (BlackboardProperty<float>)_property;
                    EditorGUILayout.FloatField(floatProperty.value);
                    GUI.enabled = true;
                    break;

                case EBlackboardPropertyType.Bool:
                    GUI.enabled = false;
                    var boolProperty = (BlackboardProperty<bool>)_property;
                    EditorGUILayout.Toggle(boolProperty.value);
                    GUI.enabled = true;
                    break;
                
                case EBlackboardPropertyType.Object:
                    var objProperty = (BlackboardProperty<Object>)_property;
                    EditorGUILayout.LabelField(objProperty.value is null ? "UObject" : objProperty.GetType().Name); 
                    break;
            }
        }


        private void OnKeyFieldChanged(ChangeEvent<string> evt)
        {
            if (string.IsNullOrEmpty(evt.newValue))
            {
                _property.key = string.Empty;
            }
            else
            {
                _property.key = evt.newValue;
            }
        }
    }
}