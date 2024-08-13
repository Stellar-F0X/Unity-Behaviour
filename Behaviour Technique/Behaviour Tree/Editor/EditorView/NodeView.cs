using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;

namespace BehaviourTechnique.BehaviourTreeEditor
{
    public class NodeView : UnityEditor.Experimental.GraphView.Node, INodeViewDeletable
    {
        public NodeView(Node node, VisualTreeAsset nodeUxml) : base(AssetDatabase.GetAssetPath(nodeUxml))
        {
            this.node = node;
            this.title = node.name;
            this.viewDataKey = node.guid;

            this.style.left = node.position.x;
            this.style.top = node.position.y;

            _nodeBorder = this.Q<VisualElement>("node-border");

            string nodeType = node.baseType.ToString();
            base.AddToClassList(nodeType.ToLower());
            
            this.CreateInputPorts();
            this.CreateOutputPorts();
        }
        
        
        public Action<NodeView> OnNodeSelected;
        public Action<NodeView> OnNodeUnselected;

        public Node node;
        public Port input;
        public Port output;

        private VisualElement _nodeBorder;

        private readonly Color _runningColor = new Color32(54, 154, 204, 255);
        private readonly Color _doneColor = new Color32(24, 93, 125, 255);

        
        
        public override void OnSelected() => OnNodeSelected?.Invoke(this);

        public override void OnUnselected() => OnNodeUnselected?.Invoke(this);
        

        private void CreateInputPorts()
        {
            switch (node.baseType)
            {
                case Node.eNodeType.Root:
                    break;

                case Node.eNodeType.Action:
                    input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                    break;

                case Node.eNodeType.Composite:
                    input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                    break;

                case Node.eNodeType.Decorator:
                    input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                    break;
            }

            if (input != null)
            {
                input.portName = string.Empty;
                input.style.flexDirection = FlexDirection.Column;
                base.inputContainer.Add(input);
            }
        }

        
        private void CreateOutputPorts()
        {
            switch (node.baseType)
            {
                case Node.eNodeType.Root:
                    output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
                    break;

                case Node.eNodeType.Action:
                    break;

                case Node.eNodeType.Composite:
                    output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
                    break;

                case Node.eNodeType.Decorator:
                    output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
                    break;
            }

            if (output != null)
            {
                output.portName = string.Empty;
                output.style.flexDirection = FlexDirection.ColumnReverse;
                base.outputContainer.Add(output);
            }
        }
        
        

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            Undo.RecordObject(node, "Behaviour Tree (Set Position)");

            node.position.x = newPos.xMin;
            node.position.y = newPos.yMin;

            UnityEditor.EditorUtility.SetDirty(node);
        }
        

        public void UpdateState()
        {
            if (Application.isPlaying)
            {
                if (node.started)
                {
                    _nodeBorder.style.borderBottomColor = _runningColor;
                    _nodeBorder.style.borderLeftColor = _runningColor;
                    _nodeBorder.style.borderRightColor = _runningColor;
                    _nodeBorder.style.borderTopColor = _runningColor;
                }
                else
                {
                    _nodeBorder.style.borderBottomColor = _doneColor;
                    _nodeBorder.style.borderLeftColor = _doneColor;
                    _nodeBorder.style.borderRightColor = _doneColor;
                    _nodeBorder.style.borderTopColor = _doneColor;
                }
            }
        }

        
        public void SortChildren()
        {
            if (this.node.baseType != Node.eNodeType.Composite)
            {
                return;
            }

            if (node is CompositeNode compositeNode)
            {
                compositeNode.children.Sort((l, r) => l.position.x < r.position.x ? -1 : 1);
            }
        }
        
        
        public void OnDeletedElementEvent(DeleteEventDetector evt)
        {
            Debug.Log("노드 삭제됨");
        }
    }
}
