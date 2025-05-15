using System;
using BehaviourSystem.BT;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;

namespace BehaviourSystemEditor.BT
{
    public class NodeView : Node
    {
        public NodeView(NodeBase node, VisualTreeAsset nodeUxml) : base(AssetDatabase.GetAssetPath(nodeUxml))
        {
            this.node = node;
            this.title = node.name;
            this.viewDataKey = node.guid;

            this.style.left = node.position.x;
            this.style.top = node.position.y;

            _nodeRunningColor = BehaviourTreeEditorWindow.Settings.nodeRunningColor;
            _nodeDoneColor = BehaviourTreeEditorWindow.Settings.nodeDoneColor;
            
            _edgeRunningColor = BehaviourTreeEditorWindow.Settings.edgeRunningColor;
            _edgeDoneColor = BehaviourTreeEditorWindow.Settings.edgeDoneColor;
            
            _nodeBorder = this.Q<VisualElement>("node-border");

            string nodeType = node.nodeType.ToString();

            this.AddToClassList(nodeType.ToLower());
            this.CreatePorts();
        }


        public event Action<NodeView> OnNodeSelected;
        public event Action<NodeView> OnNodeUnselected;

        public NodeBase node;
        public Port input;
        public Port output;
        
        public Edge toParentEdge;
        
        private VisualElement _inputElement;
        private VisualElement _outputElement;
        
        private ulong _lastRenderedNodeCount = 0;
        private readonly VisualElement _nodeBorder;

        private readonly Color _nodeRunningColor;
        private readonly Color _nodeDoneColor;
        
        private readonly Color _edgeRunningColor;
        private readonly Color _edgeDoneColor;


        public override void OnSelected() => OnNodeSelected?.Invoke(this);

        public override void OnUnselected() => OnNodeUnselected?.Invoke(this);


        private void CreatePorts()
        {
            switch (node.nodeType)
            {
                case NodeBase.ENodeType.Root: 
                    output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool)); 
                    _outputElement = output.Q<VisualElement>("cap");
                    break;

                case NodeBase.ENodeType.Action: 
                    input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool)); 
                    _inputElement = input.Q<VisualElement>("cap");
                    break;

                case NodeBase.ENodeType.Composite:
                    input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                    output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
                    _inputElement = input.Q<VisualElement>("cap");
                    _outputElement = output.Q<VisualElement>("cap");
                    break;

                case NodeBase.ENodeType.Decorator:
                    input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                    output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
                    _inputElement = input.Q<VisualElement>("cap");
                    _outputElement = output.Q<VisualElement>("cap");
                    break;
            }

            this.SetupPort(input, string.Empty, FlexDirection.Column, base.inputContainer);
            this.SetupPort(output, string.Empty, FlexDirection.ColumnReverse, base.outputContainer);
        }


        private void SetupPort(Port port, string portName, FlexDirection direction, VisualElement container)
        {
            if (port != null)
            {
                port.style.flexDirection = direction;
                port.portName = portName;
                container.Add(port);
            }
        }


        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            Undo.RecordObject(node, "Behaviour Tree (Set Position)");

            node.position.x = newPos.xMin;
            node.position.y = newPos.yMin;

            EditorUtility.SetDirty(node);
        }

        
        public void SortChildren()
        {
            if (this.node.nodeType != NodeBase.ENodeType.Composite)
            {
                return;
            }

            if (node is CompositeNode compositeNode)
            {
                compositeNode.children.Sort((l, r) => l.position.x < r.position.x ? -1 : 1);
            }
        }


        //상속받은 상위 클래스에서 Disconnect All이라는 ContextualMenu 생성을 방지하기 위해서 오버라이드
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) { }
        

        
        public void UpdateState()
        {
            if (Application.isPlaying)
            {
                if (node.callCount != this._lastRenderedNodeCount)
                {
                    this._lastRenderedNodeCount = node.callCount;

                    _nodeBorder.style.borderBottomColor = _nodeRunningColor;
                    _nodeBorder.style.borderLeftColor = _nodeRunningColor;
                    _nodeBorder.style.borderRightColor = _nodeRunningColor;
                    _nodeBorder.style.borderTopColor = _nodeRunningColor;
                    
                    if (toParentEdge != null)
                    {
                        toParentEdge.BringToFront();
                        toParentEdge.edgeControl.inputColor = _edgeRunningColor;
                        toParentEdge.edgeControl.outputColor = _edgeRunningColor;
                    }

                    if (input != null)
                    {
                        _inputElement.style.backgroundColor = _edgeRunningColor;
                    }
                    
                    if (output != null)
                    {
                        _outputElement.style.backgroundColor = _edgeRunningColor;
                    }
                }
                else
                {
                    _nodeBorder.style.borderBottomColor = _nodeDoneColor;
                    _nodeBorder.style.borderLeftColor = _nodeDoneColor;
                    _nodeBorder.style.borderRightColor = _nodeDoneColor;
                    _nodeBorder.style.borderTopColor = _nodeDoneColor;
                    
                    if (toParentEdge != null)
                    {
                        toParentEdge.SendToBack();
                        toParentEdge.edgeControl.inputColor = _edgeDoneColor;
                        toParentEdge.edgeControl.outputColor = _edgeDoneColor;
                    }
                    
                    if (input != null)
                    {
                        _inputElement.style.backgroundColor = _edgeDoneColor;
                    }
                    
                    if (output != null)
                    {
                        _outputElement.style.backgroundColor = _edgeDoneColor;
                    }
                }
            }
        }
    }
}