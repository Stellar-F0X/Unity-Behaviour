using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BehaviourSystem.BT;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BehaviourSystemEditor.BT
{
    public class NodeGroupView : Group
    {
        public NodeGroupView(GroupDataSet dataSet, GroupData dataContainer) : base()
        {
            this._data = dataContainer;
            this._groupDataDataSet = dataSet;

            this.title = dataContainer.title;
            this.style.backgroundColor = BehaviourTreeEditor.Settings.nodeGroupColor;
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
            if (_data.containedNodeCount == 0)
            {
                Undo.RecordObject(_data, "Behaviour Tree (NodeGroupViewPositionChanged)");
            }

            _data.position = newPos.position;
            base.SetScopePositionOnly(newPos);

            if (_data.containedNodeCount == 0)
            {
                EditorUtility.SetDirty(_groupDataDataSet);
            }
        }


        protected override void OnElementsAdded(IEnumerable<GraphElement> elements)
        {
            if (BehaviourTreeEditor.Instance != null && BehaviourTreeEditor.CanEditTree && BehaviourTreeEditor.isInLoadingBTAsset == false && _data != null)
            {
                _data.AddNodeGuid(elements.Where(x => x.selected && x is NodeView).ConvertAll(x => ((NodeView)x).node));
            }
        }


        protected override void OnElementsRemoved(IEnumerable<GraphElement> elements)
        {
            if (BehaviourTreeEditor.Instance != null && BehaviourTreeEditor.CanEditTree && BehaviourTreeEditor.isInLoadingBTAsset == false && _data != null)
            {
                _data.RemoveNodeGuid(elements.Where(x => x.selected && x is NodeView).ConvertAll(x => ((NodeView)x).node));
            }
        }
    }
}