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

        private ToolbarMenu _propertyAddMenu;
        private BlackboardData _blackboardData;


        public void Setup(ToolbarMenu toolbarMenu)
        {
            this._propertyAddMenu = toolbarMenu;
            this.makeItem = BehaviourTreeEditorWindow.Settings.blackboardPropertyViewXml.CloneTree;
            this.bindItem = this.BindItemToList;
            this.itemIndexChanged += this.OnPropertiesOrderSwapped;

            Undo.undoRedoPerformed += () =>
            {
                if (_serializedObject != null)
                {
                    this.RefreshItems();
                    _serializedObject.Update();
                }
            };
        }


        public void ClearBlackboardPropertyViews()
        {
            this.Clear();
            this.RefreshItems();

            _propertyAddMenu.menu.ClearItems();
        }


        public void ChangeBehaviourTree(BehaviourTree tree)
        {
            if (tree != null && BehaviourTreeEditorWindow.Instance != null)
            {
                this._blackboardData = tree.blackboardData;
                this._serializedObject = new SerializedObject(this._blackboardData);
                this._serializedListProperty = _serializedObject.FindProperty("_properties");

                this.itemsSource = this._blackboardData.properties;
                this.RefreshItems();

                if (BehaviourTreeEditorWindow.Instance.CanEditTree)
                {
                    TypeCache.GetTypesDerivedFrom<IBlackboardProperty>()
                             .Where(t => t.IsAbstract == false)
                             .ForEach(t => _propertyAddMenu.menu.AppendAction(t.Name, _ => this.MakeProperty(t)));
                }
            }
        }


        private void MakeProperty(Type type)
        {
            Undo.RecordObject(_blackboardData, "Behaviour Tree (AddBlackboardProperty)");

            itemsSource.Add(IBlackboardProperty.Create(type));
            _serializedObject.Update();
            _serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(_blackboardData);
            this.RefreshItems();
        }


        private void DeleteProperty(int index)
        {
            Undo.RecordObject(_blackboardData, "Behaviour Tree (RemoveBlackboardProperty)");

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

            buttonField.clickable = null; //reset all callback
            buttonField.clicked += () => this.DeleteProperty(index);
            buttonField.enabledSelf = BehaviourTreeEditorWindow.Instance.CanEditTree;

            imguiField.onGUIHandler = () => this.DrawIMGUIForItem(index);
            
            keyField.SetValueWithoutNotify(((IBlackboardProperty)itemsSource[index]).key);
            keyField.UnregisterValueChangedCallback(KeyChangeEvent);
            keyField.RegisterValueChangedCallback(KeyChangeEvent);

            #region Local function for caching
            
            void KeyChangeEvent(ChangeEvent<string> evt) => this.OnChangePropertyKey(evt.newValue, index);
                
            #endregion
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


        private void OnPropertiesOrderSwapped(int a, int b)
        {
            _serializedObject.Update();
            _serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(_blackboardData);
        }
    }
}