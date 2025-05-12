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
    public partial class BlackboardPropertyViewList : ListView
    {
        private SerializedObject _serializedObject;
        private SerializedProperty _serializedListProperty;

        private ToolbarMenu _addButton;
        private BlackboardData _blackboardData;


        public void SetUp(ToolbarMenu button)
        {
            _addButton = button;
        }



        public void ClearBlackboardPropertyViews()
        {
            this.hierarchy.Clear();
            _addButton.menu.ClearItems();
        }



        public void ChangeBehaviourTree(BehaviourTree tree)
        {
            if (BehaviourTreeEditorWindow.Instance is null)
            {
                return;
            }

            _blackboardData = tree.blackboardData;
            _serializedObject = new SerializedObject(tree.blackboardData);
            _serializedListProperty = _serializedObject.FindProperty("_properties");

            for (int i = 0; i < _serializedListProperty.arraySize; ++i)
            {
                this.AddProperty(_serializedListProperty.GetArrayElementAtIndex(i));
            }

            if (BehaviourTreeEditorWindow.Instance.CanEditTree)
            {
                var types = TypeCache.GetTypesDerivedFrom<IBlackboardProperty>();

                if (types.Count > 0)
                {
                    foreach (Type type in types)
                    {
                        if (type.IsAbstract == false)
                        {
                            _addButton.menu.AppendAction(type.Name, _ => this.MakeProperty(type));
                        }
                    }
                }
            }
        }


        private void MakeProperty(Type type)
        {
            int newIndex = _serializedListProperty.arraySize;
            _serializedListProperty.InsertArrayElementAtIndex(newIndex);

            var newElement = _serializedListProperty.GetArrayElementAtIndex(newIndex);
            newElement.managedReferenceValue = IBlackboardProperty.Create(type);
            this.AddProperty(newElement);

            _serializedObject.ApplyModifiedProperties();
        }


        private void AddProperty(SerializedProperty prop)
        {
            var propertyView = new BlackboardPropertyView(prop, BehaviourTreeEditorWindow.Settings.blackboardPropertyViewXml);

            propertyView.RegisterButtonEvent(() => this.OnPropertyRemoved(propertyView));
            propertyView.activeDeleteButton = BehaviourTreeEditorWindow.Instance.CanEditTree;

            this.hierarchy.Add(propertyView);
            base.RefreshItems();
        }


        private void OnPropertyRemoved(BlackboardPropertyView propertyView)
        {
            var prop = propertyView.property.boxedValue as IBlackboardProperty;
            int targetIndex = _blackboardData.IndexOf(prop);

            _blackboardData.RemoveAt(targetIndex);

            this.hierarchy.Remove(propertyView);
            base.RefreshItems();

            EditorUtility.SetDirty(_blackboardData);
            _serializedObject.ApplyModifiedProperties();
        }
    }
}