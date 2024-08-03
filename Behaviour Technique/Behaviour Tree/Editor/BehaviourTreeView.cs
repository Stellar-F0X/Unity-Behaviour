using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

public class BehaviourTreeView : GraphView
{
    public new class UxmlFactory : UxmlFactory<BehaviourTreeView, UxmlTraits> { }

    public Action<NodeView> OnNodeSelected;

    private BehaviourTree _cachedTree;


    public BehaviourTreeView()
    {
        string basePath = BehaviourTreeEditor.FindEditorGraphicAssetFolder("Behaviour Technique t:Folder", "/Behaviour Tree/Layout");
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(basePath + "/BehaviourTreeEditorStyle.uss");

        styleSheets.Add(styleSheet);

        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        base.Insert(0, new GridBackground());

        Undo.undoRedoPerformed += OnUndoRedo;
    }


    private void OnUndoRedo()
    {
        this.PopulateView(_cachedTree);
        AssetDatabase.SaveAssets();
    }


    public void PopulateView(BehaviourTree tree)
    {
        this._cachedTree = tree;
        graphViewChanged -= OnGraphViewChanged;
        base.DeleteElements(graphElements);
        graphViewChanged += OnGraphViewChanged;

        if (_cachedTree.rootNode == null)
        {
            tree.rootNode = tree.CreateNode(typeof(RootNode)) as RootNode;
            EditorUtility.SetDirty(tree);
            AssetDatabase.SaveAssets();
        }

        //다시 불러올때 만들어뒀던 Node를 생성.
        tree.nodeList.ForEach(n => this.CreateNodeView(n));

        //만들어뒀던 Node끼리 Edge를 연결.
        foreach (var node in tree.nodeList)
        {
            foreach(var child in tree.GetChildren(node))
            {
                NodeView parentView = this.FindNodeView(node);
                NodeView childView = this.FindNodeView(child);

                if (parentView == null || childView == null)
                {
                    continue;
                }

                base.AddElement(parentView.output.ConnectTo(childView.input));
            }
        }
    }


    private NodeView FindNodeView(StateNode node)
    {
        return base.GetNodeByGuid(node?.guid) as NodeView;
    }


    private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
    {
        if (graphViewChange.elementsToRemove != null)
        {
            graphViewChange.elementsToRemove.ForEach(e =>
            {
                if (e is NodeView && _cachedTree != null)
                {
                    NodeView nodeView = e as NodeView;
                    _cachedTree.DeleteNode(nodeView.node);
                }

                if (e is Edge)
                {
                    var edge = e as Edge;
                    NodeView parentView = edge.output.node as NodeView;
                    NodeView ChildView = edge.input.node as NodeView;
                    _cachedTree.RemoveChild(parentView.node, ChildView.node);
                }
            });
        }

        if (graphViewChange.edgesToCreate != null)
        {
            graphViewChange.edgesToCreate.ForEach(edge =>
            {
                NodeView parentView = edge.output.node as NodeView;
                NodeView childView = edge.input.node as NodeView;
                _cachedTree.AddChild(parentView.node, childView.node);
            });
        }

        if (graphViewChange.movedElements != null)
        {
            this.nodes.ForEach(n => (n as NodeView).SortChildren());
        }

        return graphViewChange;
    }


    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        return base.ports.ToList().Where(endPort =>
            //direction은 input과 output이므로, 다른 노드라도 같은 포트에 못 꽂게 방지
            endPort.direction != startPort.direction &&
            endPort.node != startPort.node
        ).ToList();
    }


    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        if (!BehaviourTreeEditor.Activated)
        {
            return;
        }

        Vector3 mousePos = evt.localMousePosition;

        InsertActions(TypeCache.GetTypesDerivedFrom<ActionNode>(), "Action");
        InsertActions(TypeCache.GetTypesDerivedFrom<CompositeNode>(), "Composite");
        InsertActions(TypeCache.GetTypesDerivedFrom<DecoratorNode>(), "Decorator");

        base.BuildContextualMenu(evt);

        void InsertActions(TypeCache.TypeCollection types, string prefix)
        {
            foreach (var type in types)
            {
                string sentence = $"[{prefix}] {type.Name.Replace("Node", string.Empty)}";
                evt.menu.AppendAction(sentence, a => CreateNode(type, mousePos));
            }
        }
    }


    private void CreateNode(Type type, Vector2 mousePosition)
    {
        StateNode node = _cachedTree.CreateNode(type);
        node.position = mousePosition;
        this.CreateNodeView(node);
    }


    private void CreateNodeView(StateNode node)
    {
        NodeView nodeView = new NodeView(node);
        nodeView.OnNodeSelected = this.OnNodeSelected;
        AddElement(nodeView);
    }


    public void UpdateNodeState()
    {
        nodes.ForEach(n => (n as NodeView).UpdateState());
    }
}
