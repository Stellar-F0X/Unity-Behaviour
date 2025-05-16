using System;
using System.Linq;
using BehaviourSystem.BT;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourSystemEditor.BT
{
    [UxmlElement]
    public partial class BlackboardPropertyListView : ListView
    {
        private SerializedProperty _serializedListProperty;
        private SerializedObject _serializedObject;

        private ToolbarMenu _propertyAddMenu;
        private Blackboard _blackboard;


        public void Setup(ToolbarMenu toolbarMenu)
        {
            this._propertyAddMenu = toolbarMenu;
            this.makeItem = BehaviourTreeEditor.Settings.blackboardPropertyViewXml.CloneTree;
            this.bindItem = this.BindItemToList;
            this.itemIndexChanged += this.OnPropertyIndicesSwapped;

            Undo.undoRedoPerformed += () =>
            {
                if (_serializedObject != null)
                {
                    this.RefreshItems();
                    _serializedObject.Update();
                }
            };
        }


        public void ClearBlackboardView()
        {
            this.itemsSource = null;
            this._blackboard = null;
            this._serializedObject = null;
            this._serializedListProperty = null;
            
            _propertyAddMenu.menu.ClearItems();
            
            this.Clear();
            this.RefreshItems();
        }


        public void ChangeBehaviourTree(BehaviourTree tree)
        {
            if (tree != null && BehaviourTreeEditor.Instance != null)
            {
                this._blackboard = tree.blackboard;
                this._serializedObject = new SerializedObject(this._blackboard);
                this._serializedListProperty = _serializedObject.FindProperty("_properties");

                this.itemsSource = this._blackboard.properties;
                this.RefreshItems();

                if (BehaviourTreeEditor.Instance.CanEditTree)
                {
                    TypeCache.GetTypesDerivedFrom<IBlackboardProperty>()
                             .Where(t => t.IsAbstract == false)
                             .ForEach(t => _propertyAddMenu.menu.AppendAction(t.Name, _ => this.MakeProperty(t)));
                }
            }
        }


        private void MakeProperty(Type type)
        {
            Undo.RecordObject(_blackboard, "Behaviour Tree (AddBlackboardProperty)");

            itemsSource.Add(IBlackboardProperty.Create(type));
            _serializedObject.Update();
            _serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(_blackboard);
            this.RefreshItems();
        }


        private void DeleteProperty(int index)
        {
            Undo.RecordObject(_blackboard, "Behaviour Tree (RemoveBlackboardProperty)");

            itemsSource.RemoveAt(index);
            _serializedObject.Update();
            _serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(_blackboard);
            this.RefreshItems();
        }


        //아이템이 추가, 제거, 순서가 변경될 때마다 호출되어 콜백들을 다시 등록하므로 인덱스가 캐싱돼도 문제되지 않는다.
        private void BindItemToList(VisualElement element, int index)
        {
            IMGUIContainer imguiField = element.Q<IMGUIContainer>("IMGUIContainer");
            TextField keyField = element.Q<TextField>("name-field");
            Button buttonField = element.Q<Button>("delete-button");

            buttonField.clickable = null; //reset all callback
            buttonField.enabledSelf = BehaviourTreeEditor.Instance.CanEditTree;
            buttonField.clicked += () => this.DeleteProperty(index);
            imguiField.onGUIHandler = () => this.DrawIMGUIForItem(index);

            if (keyField.userData != null)
            {
                var previousCallback = (EventCallback<ChangeEvent<string>>)keyField.userData;
                keyField.UnregisterValueChangedCallback(previousCallback);
            }

            keyField.value = ((IBlackboardProperty)itemsSource[index]).key;
            var newCallback = new EventCallback<ChangeEvent<string>>(e => this.OnChangePropertyKey(e.newValue, index));
            keyField.RegisterValueChangedCallback(newCallback);
            keyField.userData = newCallback;
        }


        private void DrawIMGUIForItem(int index)
        {
            if (_serializedListProperty.arraySize <= index)
            {
                return;
            }

            SerializedProperty property = _serializedListProperty.GetArrayElementAtIndex(index);

            if (property.boxedValue == null)
            {
                return;
            }

            using (new EditorGUI.DisabledScope(true))
            {
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
                EditorUtility.SetDirty(_blackboard);
            }
        }


        private void OnPropertyIndicesSwapped(int a, int b)
        {
            _serializedObject.Update();
            _serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(_blackboard);
        }
    }
}