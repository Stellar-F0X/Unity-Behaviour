using System.Collections.Generic;
using System.Linq;
using BehaviourSystem;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BehaviourSystemEditor.BT
{
    public class NodeGroupView : Group
    {
        public NodeGroupView(GroupViewDataCollection collection, GroupViewData dataContainer)
        {
            this._groupDataCollection = collection;
            this._viewData = dataContainer;
        }

        private readonly GroupViewDataCollection _groupDataCollection;
        private readonly GroupViewData _viewData;


        public GroupViewData viewData
        {
            get { return _viewData; }
        }


        protected override void OnGroupRenamed(string oldName, string newName)
        {
            Undo.RecordObject(_viewData, "Behaviour Tree (NodeGroupViewNameChanged)");
            base.OnGroupRenamed(oldName, newName);
            _viewData.title = newName;
            EditorUtility.SetDirty(_groupDataCollection);
        }


        protected override void SetScopePositionOnly(Rect newPos)
        {
            //NodeView도 위치를 Record하는데, GroupView를 움직이면 NodeView도 움직이며 위치가 기록되어 Undo 기록이 중첩됨.
            //따라서 Group에 요소가 있는 상태로 움직인 후 GroupView가 정상적으로 동작하려면 여러번 Undo해야 되므로
            //또한 NodeView를 기준으로 GroupView 위치가 정해지기 때문에 Group에 요소가 없는 상태일 때만 기록시킴.  
            if (_viewData.count == 0)
            {
                Undo.RecordObject(_viewData, "Behaviour Tree (NodeGroupViewPositionChanged)");
            }

            base.SetScopePositionOnly(newPos);
            _viewData.position = newPos.position;

            if (_viewData.count == 0)
            {
                EditorUtility.SetDirty(_groupDataCollection);
            }
        }


        protected override void OnElementsAdded(IEnumerable<GraphElement> elements)
        {
            if (BehaviourTreeEditorWindow.Instance is not null && BehaviourTreeEditorWindow.Instance.CanEditTree && _viewData != null)
            {
                Undo.RecordObject(_viewData, "Behaviour Tree (AddNodeGuidToGroup)");

                foreach (GraphElement node in elements)
                {
                    if (node.selected && node is NodeView view)
                    {
                        if (_viewData.Contains(view.node.guid) == false && string.IsNullOrEmpty(view.node.guid) == false)
                        {
                            _viewData.AddNodeGuid(view.node.guid);
                        }
                    }
                }

                EditorUtility.SetDirty(_viewData);
            }
        }


        protected override void OnElementsRemoved(IEnumerable<GraphElement> elements)
        {
            if (BehaviourTreeEditorWindow.Instance is not null && BehaviourTreeEditorWindow.Instance.CanEditTree && _viewData != null)
            {
                Undo.RecordObject(_viewData, "Behaviour Tree (RemoveNodeGuidToGroup)");

                foreach (GraphElement node in elements)
                {
                    if (node.selected && node is NodeView view)
                    {
                        if (_viewData.Contains(view.node.guid) && string.IsNullOrEmpty(view.node.guid) == false)
                        {
                            _viewData.RemoveNodeGuid(view.node.guid);
                        }
                    }
                }

                EditorUtility.SetDirty(_viewData);
            }
        }
    }
}