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
            Undo.RecordObject(_groupDataCollection, "Behaviour Tree (NodeGroupViewRenamed)");
            base.OnGroupRenamed(oldName, newName);
            _viewData.title = newName;
            EditorUtility.SetDirty(_groupDataCollection);
        }


        protected override void SetScopePositionOnly(Rect newPos)
        {
            Undo.RecordObject(_groupDataCollection, "Behaviour Tree (NodeGroupViewMoved)");
            base.SetScopePositionOnly(newPos);
            _viewData.position = newPos.position;
            EditorUtility.SetDirty(_groupDataCollection);
        }


        protected override void OnElementsAdded(IEnumerable<GraphElement> elements)
        {
            Undo.RecordObject(_groupDataCollection, "Behaviour Tree (AddNodeGuidToGroup)");

            foreach (GraphElement node in elements)
            {
                if (node.selected && node is NodeView view)
                {
                    bool isNotContained = _viewData.Contains(view.node.guid) == false;
                    bool isAvailable = string.IsNullOrEmpty(view.node.guid) == false;

                    if (isNotContained && isAvailable)
                    {
                        _viewData.AddNodeGuid(view.node.guid);
                    }
                }
            }

            EditorUtility.SetDirty(_groupDataCollection);
        }


        protected override void OnElementsRemoved(IEnumerable<GraphElement> elements)
        {
            Undo.RecordObject(_groupDataCollection, "Behaviour Tree (RemoveNodeGuidToGroup)");

            foreach (GraphElement node in elements)
            {
                if (node.selected && node is NodeView view)
                {
                    bool contained = _viewData.Contains(view.node.guid);
                    bool isAvailable = string.IsNullOrEmpty(view.node.guid) == false;

                    if (contained && isAvailable)
                    {
                        _viewData.RemoveNodeGuid(view.node.guid);
                    }
                }
            }

            EditorUtility.SetDirty(_groupDataCollection);
        }
    }
}