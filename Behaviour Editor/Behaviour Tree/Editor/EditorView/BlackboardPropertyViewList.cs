using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourTechnique.BehaviourTreeEditor
{
    public class BlackboardPropertyViewList : ListView
    {
        public new class UxmlFactory : UxmlFactory<BlackboardPropertyViewList, UxmlTraits> { }

        public BlackboardPropertyViewList() { }

        
        private ToolbarMenu _addButton;
        private BehaviourTree _tree;


        public void SetUp(ToolbarMenu button)
        {
            _addButton = button;
            
            _addButton.menu.AppendAction(EBlackboardPropertyType.Int.ToString(), _ => this.MakeProperty<int>(EBlackboardPropertyType.Int));
            _addButton.menu.AppendAction(EBlackboardPropertyType.Bool.ToString(), _ => this.MakeProperty<bool>(EBlackboardPropertyType.Bool));
            _addButton.menu.AppendAction(EBlackboardPropertyType.Float.ToString(), _ => this.MakeProperty<float>(EBlackboardPropertyType.Float));
            _addButton.menu.AppendAction(EBlackboardPropertyType.Object.ToString(), _ => this.MakeProperty<UnityEngine.Object>(EBlackboardPropertyType.Object));
        }
        
        
        
        public void ClearBlackboardPropertyViews()
        {
            this.hierarchy.Clear();
        }
        


        public void ChangeBehaviourTree(BehaviourTree tree)
        {
            this._tree = tree;

            for (int i = 0; i < _tree.blackboardData.Count; ++i)
            {
                this.AddProperty(_tree.blackboardData.GetProperty(i));
            }
        }


        public void AddProperty(IBlackboardProperty prop)
        {
            var propertyView = new BlackboardPropertyView(prop, BehaviourTreeEditorWindow.Settings.blackboardPropertyViewXml);

            propertyView.onRemove += this.OnPropertyRemoved;

            this.hierarchy.Add(propertyView);
            base.RefreshItems();
        }
        
        
        private void MakeProperty<T> (EBlackboardPropertyType type)
        {
            BlackboardProperty<T> prop = new BlackboardProperty<T>(string.Empty, default(T), type);
            
            this._tree.blackboardData.AddProperty(prop);

            this.AddProperty(prop);

            EditorUtility.SetDirty(this._tree);
        }


        private void OnPropertyRemoved(BlackboardPropertyView property)
        {
            this.hierarchy.Remove(property);
            this._tree.blackboardData.Remove(property.property);
            base.RefreshItems();
            
            EditorUtility.SetDirty(this._tree);
        }
    }
}