using System.Collections.Generic;
using BehaviourSystem.BT;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BehaviourSystemEditor.BT
{
    public class NodeGroupView : Group
    {
        public NodeGroupView(GroupDataSet dataSet, GroupData dataContainer)
        {
            this.style.top = dataContainer.position.y;
            this.style.left = dataContainer.position.x;
            this.title = dataContainer.title;

            this._data = dataContainer;
            this._groupDataDataSet = dataSet;
        }

        private readonly GroupDataSet _groupDataDataSet;
        private readonly GroupData _data;


        public GroupData data
        {
            get { return _data; }
        }


        protected override void OnGroupRenamed(string oldName, string newName)
        {
            Undo.RecordObject(_data, "Behaviour Tree (NodeGroupViewNameChanged)");
            base.OnGroupRenamed(oldName, newName);
            _data.title = newName;
            EditorUtility.SetDirty(_groupDataDataSet);
        }


        protected override void SetScopePositionOnly(Rect newPos)
        {
            //NodeView도 위치를 Record하는데, GroupView를 움직이면 NodeView도 움직이며 위치가 기록되어 Undo 기록이 중첩됨.
            //따라서 Group에 요소가 있는 상태로 움직인 후 GroupView가 정상적으로 동작하려면 여러번 Undo해야 되므로
            //또한 NodeView를 기준으로 GroupView 위치가 정해지기 때문에 Group에 요소가 없는 상태일 때만 기록시킴.  
            if (_data.count == 0)
            {
                Undo.RecordObject(_data, "Behaviour Tree (NodeGroupViewPositionChanged)");
            }

            _data.position = newPos.position;
            base.SetScopePositionOnly(newPos);

            if (_data.count == 0)
            {
                EditorUtility.SetDirty(_groupDataDataSet);
            }
        }


        protected override void OnElementsAdded(IEnumerable<GraphElement> elements)
        {
            if (BehaviourTreeEditor.Instance is not null && BehaviourTreeEditor.Instance.CanEditTree && _data != null)
            {
                Undo.RecordObject(_data, "Behaviour Tree (AddNodeGuidToGroup)");

                foreach (GraphElement node in elements)
                {
                    if (node.selected && node is NodeView view && string.IsNullOrEmpty(view.node.guid) == false && _data.Contains(view.node.guid) == false)
                    {
                        _data.AddNodeGuid(view.node.guid);
                    }
                }

                EditorUtility.SetDirty(_data);
            }
        }


        protected override void OnElementsRemoved(IEnumerable<GraphElement> elements)
        {
            if (BehaviourTreeEditor.Instance is not null && BehaviourTreeEditor.Instance.CanEditTree && _data != null)
            {
                Undo.RecordObject(_data, "Behaviour Tree (RemoveNodeGuidToGroup)");

                foreach (GraphElement node in elements)
                {
                    if (node.selected && node is NodeView view && string.IsNullOrEmpty(view.node.guid) == false && _data.Contains(view.node.guid))
                    {
                        _data.RemoveNodeGuid(view.node.guid);
                    }
                }

                EditorUtility.SetDirty(_data);
            }
        }
    }
}