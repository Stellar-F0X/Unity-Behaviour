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

        private readonly VisualElement _nodeBorder;

        private readonly Color _runningColor = new Color32(54, 154, 204, 255);
        private readonly Color _doneColor = new Color32(24, 93, 125, 255);



        public override void OnSelected() => OnNodeSelected?.Invoke(this);

        public override void OnUnselected() => OnNodeUnselected?.Invoke(this);


        private void CreatePorts()
        {
            switch (node.nodeType)
            {
                case NodeBase.ENodeType.Root: output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool)); break;

                case NodeBase.ENodeType.Action: input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool)); break;

                case NodeBase.ENodeType.Composite:
                    input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                    output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
                    break;

                case NodeBase.ENodeType.Decorator:
                    input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                    output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
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


        public void UpdateState()
        {
            if (Application.isPlaying)
            {
                if (node.callState == NodeBase.ENodeCallState.Updating)
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
    }
}