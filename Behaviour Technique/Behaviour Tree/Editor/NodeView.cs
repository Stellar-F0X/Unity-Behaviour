using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor;
using System.Linq;

public class NodeView : Node
{
    public Action<NodeView> OnNodeSelected;

    public StateNode node;
    public Port input;
    public Port output;

    private static string uxmlPath
    {
        get { return BehaviourTreeEditor.FindEditorGraphicAssetFolder("Behaviour Technique t:Folder", "/Behaviour Tree/Layout") + "/NodeView.uxml"; }
    }


    public NodeView(StateNode node) : base(uxmlPath)
    {
        this.node = node;
        this.title = node.name;
        this.viewDataKey = node.guid;

        this.style.left = node.position.x;
        this.style.top = node.position.y;

        this.SetupEachNodes();
        this.CreateInputPorts();
        this.CreateOutputPorts();
    }

    private void CreateInputPorts()
    {
        switch (node.nodeType)
        {
            case StateNode.eNodeType.Root:
                break;

            case StateNode.eNodeType.Action:
                input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                break;
            case StateNode.eNodeType.Composite:
                input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                break;
            case StateNode.eNodeType.Decorator:
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
        switch (node.nodeType)
        {
            case StateNode.eNodeType.Root:
                output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
                break;

            case StateNode.eNodeType.Action:
                break;

            case StateNode.eNodeType.Composite:
                output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
                break;

            case StateNode.eNodeType.Decorator:
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

    public void SetupEachNodes()
    {
        string nodeType = node.nodeType.ToString();
        nodeType = nodeType.ToLower();
        base.AddToClassList(nodeType);

        base.tooltip = node.desciption;
    }

    public override void SetPosition(Rect newPos)
    {
        base.SetPosition(newPos);

        Undo.RecordObject(node, "Behaviour Tree (Set Position)");

        node.position.x = newPos.xMin;
        node.position.y = newPos.yMin;

        EditorUtility.SetDirty(node);
    }

    public override void OnSelected()
    {
        base.OnSelected();
        OnNodeSelected?.Invoke(this);
    }

    public void UpdateState()
    {
        style.borderBottomColor = new Color(0, 0, 0, 0);

        if (Application.isPlaying)
        {
            if (node.state == StateNode.eState.Running && node.started)
            {
                style.borderBottomColor = new Color(255, 255, 255, 1);
            }
            else
            {
                style.borderBottomColor = new Color(255, 255, 255, 1);
            }
        }
    }

    public void SortChildren()
    {
        if (this.node.nodeType != StateNode.eNodeType.Composite)
        {
            return;
        }

        if (node is CompositeNode compositeNode)
        {
            compositeNode.children.Sort((l, r) => {
                return l.position.x < r.position.x ? -1 : 1;
            });
        }
    }
}
