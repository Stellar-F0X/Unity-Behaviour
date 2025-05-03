using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace BehaviourTechnique.BehaviourTreeEditor
{
    public class BlackboardPropertyViewList : ListView
    {
        public new class UxmlFactory : UxmlFactory<BlackboardPropertyViewList, UxmlTraits> { }

        public BlackboardPropertyViewList() { }

        
        private ToolbarMenu _addButton;
        private BlackboardData _blackboardData;


        public void SetUp(ToolbarMenu button)
        {
            _addButton = button;
            
            _addButton.menu.AppendAction(EBlackboardElement.Int.ToString(), _ => this.MakeProperty<int>(EBlackboardElement.Int));
            _addButton.menu.AppendAction(EBlackboardElement.Bool.ToString(), _ => this.MakeProperty<bool>(EBlackboardElement.Bool));
            _addButton.menu.AppendAction(EBlackboardElement.Float.ToString(), _ => this.MakeProperty<float>(EBlackboardElement.Float));
        }
        
        
        
        public void ClearBlackboardPropertyViews()
        {
            this.hierarchy.Clear();
        }
        


        public void ChangeBlackboardData(BlackboardData blackboardData)
        {
            this._blackboardData = blackboardData;

            for (int i = 0; i < _blackboardData.Count; ++i)
            {
                this.AddProperty(_blackboardData.GetProperty(i));
            }
        }


        public void AddProperty(IBlackboardProperty prop)
        {
            var propertyView = new BlackboardPropertyView(prop, BehaviourTreeEditorWindow.Settings.blackboardPropertyViewXml);

            propertyView.onRemove += this.OnPropertyRemoved;

            this.hierarchy.Add(propertyView);
            base.RefreshItems();
        }
        
        
        private void MakeProperty<T> (EBlackboardElement type) where T : struct
        {
            BlackboardProperty<T> prop = new BlackboardProperty<T>(string.Empty, default(T), type);
            
            this._blackboardData.AddProperty(prop);

            this.AddProperty(prop);
        }


        private void OnPropertyRemoved(BlackboardPropertyView property)
        {
            this.hierarchy.Remove(property);
            this._blackboardData.Remove(property.property);
            base.RefreshItems();
        }
    }
}