using System;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine;

namespace BehaviourTechnique.BehaviourTreeEditor
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
            GUI.enabled = false;
            
            switch (_property.elementType)
            {
                case EBlackboardElement.Int:
                    var intProperty = (BlackboardProperty<int>)_property;
                    EditorGUILayout.IntField(intProperty.value);
                    break;

                case EBlackboardElement.Float:
                    var floatProperty = (BlackboardProperty<float>)_property;
                    EditorGUILayout.FloatField(floatProperty.value);
                    break;

                case EBlackboardElement.Bool:
                    var boolProperty = (BlackboardProperty<bool>)_property;
                    EditorGUILayout.Toggle(boolProperty.value);
                    break;
            }

            GUI.enabled = true;
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