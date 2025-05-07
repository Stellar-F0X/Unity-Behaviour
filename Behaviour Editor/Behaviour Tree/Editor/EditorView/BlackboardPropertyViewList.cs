using System;
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
        private SerializedObject   _serializedObject;
        private SerializedProperty _serializedListProperty;

        private ToolbarMenu _addButton;


        public void SetUp(ToolbarMenu button)
        {
            _addButton = button;

            var types = TypeCache.GetTypesDerivedFrom<IBlackboardProperty>();

            if (types.Count == 0)
            {
                return;
            }

            foreach (Type type in types)
            {
                if (type.IsAbstract == false)
                {
                    _addButton.menu.AppendAction(type.Name, _ => this.MakeProperty(type));
                }
            }
        }



        public void ClearBlackboardPropertyViews()
        {
            this.hierarchy.Clear();
        }



        public void ChangeBehaviourTree(BehaviourTree tree)
        {
            _serializedObject       = new SerializedObject(tree.blackboardData);
            _serializedListProperty = _serializedObject.FindProperty("_properties");

            for (int i = 0; i < _serializedListProperty.arraySize; ++i)
            {
                this.AddProperty(_serializedListProperty.GetArrayElementAtIndex(i));
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
        
        
        public void AddProperty(SerializedProperty prop)
        {
            var propertyView = new BlackboardPropertyView(prop, BehaviourTreeEditorWindow.Settings.blackboardPropertyViewXml);

            propertyView.onRemove += this.OnPropertyRemoved;

            this.hierarchy.Add(propertyView);
            base.RefreshItems();
        }


        private void OnPropertyRemoved(BlackboardPropertyView property)
        {
            for (int propertyIndex = 0; propertyIndex < _serializedListProperty.arraySize; ++propertyIndex)
            {
                var prop = _serializedListProperty.GetArrayElementAtIndex(propertyIndex);

                if (string.Compare(prop.propertyPath, property.property.propertyPath, StringComparison.Ordinal) == 0)
                {
                    this.hierarchy.Remove(property);
                    base.RefreshItems();
                    
                    this._serializedListProperty.DeleteArrayElementAtIndex(propertyIndex);
                    _serializedListProperty.serializedObject.ApplyModifiedProperties();
                    return;
                }
            }
            
            Debug.LogError("The property to be removed was not found in blackboardData.");
        }
    }
}