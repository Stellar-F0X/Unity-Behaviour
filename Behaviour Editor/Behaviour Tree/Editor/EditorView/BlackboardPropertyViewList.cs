using System;
using System.Collections.Generic;
using System.Linq;
using BehaviourSystem.BT;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourSystemEditor.BT
{
    [UxmlElement]
    public partial class BlackboardPropertyViewList : ListView
    {
        private SerializedProperty _serializedListProperty;
        private SerializedObject _serializedObject;

        private ToolbarMenu _addButton;
        private BlackboardData _blackboardData;


        public void Setup(ToolbarMenu button)
        {
            this._addButton = button;
            
            this.makeItem = BehaviourTreeEditorWindow.Settings.blackboardPropertyViewXml.CloneTree;
            this.bindItem = this.BindItemToList;
            
            this.selectionType = SelectionType.Multiple;
        }


        public void ClearBlackboardPropertyViews()
        {
            this.Clear();
            this.RefreshItems();

            _addButton.menu.ClearItems();
        }


        public void ChangeBehaviourTree(BehaviourTree tree)
        {
            if (tree != null && BehaviourTreeEditorWindow.Instance != null)
            {
                this._blackboardData = tree.blackboardData;
                this._serializedObject = new SerializedObject(tree.blackboardData);
                this._serializedListProperty = _serializedObject.FindProperty("_properties");

                this.itemsSource = _blackboardData.properties;
                this.RefreshItems();

                if (BehaviourTreeEditorWindow.Instance.CanEditTree)
                {
                    TypeCache.GetTypesDerivedFrom<IBlackboardProperty>()
                             .Where(t => t.IsAbstract == false)
                             .ForEach(t => _addButton.menu.AppendAction(t.Name, _ => this.MakeProperty(t)));
                }
            }
        }
        
        
        private void MakeProperty(Type type)
        {
            itemsSource.Add(IBlackboardProperty.Create(type));
            _serializedObject.Update();
            _serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(_blackboardData);
            this.RefreshItems();
        }
        
        
        private void DeleteProperty(int index)
        {
            itemsSource.RemoveAt(index);
            _serializedObject.Update();
            _serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(_blackboardData);
            this.RefreshItems();
        }


        private void BindItemToList(VisualElement element, int index)
        {
            IMGUIContainer imguiField = element.Q<IMGUIContainer>("IMGUIContainer");
            TextField keyField = element.Q<TextField>("name-field");
            Button buttonField = element.Q<Button>("delete-button");
            
            buttonField.enabledSelf = BehaviourTreeEditorWindow.Instance.CanEditTree;
            
            buttonField.clickable = null;
            buttonField.clicked += () => this.DeleteProperty(index);
            imguiField.onGUIHandler = () => this.DrawIMGUIForItem(index);

            keyField.value = (itemsSource[index] as IBlackboardProperty)?.key ?? string.Empty;
            keyField.RegisterValueChangedCallback(evt => this.OnChangePropertyKey(evt.newValue, index));
        }


        private void DrawIMGUIForItem(int index)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                SerializedProperty property = _serializedListProperty.GetArrayElementAtIndex(index);
                EditorGUILayout.PropertyField(property.FindPropertyRelative("_value"), GUIContent.none);
            }
        }

        
        private void OnChangePropertyKey(string newKey, int index)
        {
            if (itemsSource[index] is IBlackboardProperty property)
            {
                bool isKeyValid = string.IsNullOrEmpty(newKey);
                property.key = isKeyValid ? string.Empty : newKey;
                
                _serializedObject.Update();
                _serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_blackboardData);
            }
        }
    }
}