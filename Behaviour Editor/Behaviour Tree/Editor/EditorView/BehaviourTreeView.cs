using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using BehaviourSystem;
using BehaviourSystem.BT;
using UnityEditor.UIElements;

namespace BehaviourSystemEditor.BT
{
    [UxmlElement]
    public partial class BehaviourTreeView : GraphView
    {
        public BehaviourTreeView()
        {
            base.Insert(0, new GridBackground());

            ContentZoomer zoomer = new ContentZoomer()
            {
                maxScale = BehaviourTreeEditorWindow.Settings.enlargementScale,
                minScale = BehaviourTreeEditorWindow.Settings.reductionScale
            };

            this.AddManipulator(zoomer);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            styleSheets.Add(BehaviourTreeEditorWindow.Settings.behaviourTreeStyle);

            _nodeEdgeHandler = new NodeEdgeHandler();
            _nodeSearchHelper = new NodeSearchHelper();

            Undo.undoRedoPerformed = () =>
            {
                this.OnGraphEditorView(_tree);
                AssetDatabase.SaveAssets();
            };
        }

        public Action onGraphViewChange;
        public Action<NodeView> onNodeSelected;
        public ToolbarPopupSearchField popupSearchField;

        private BehaviourTree _tree;
        private NodeSearchHelper _nodeSearchHelper;
        private NodeEdgeHandler _nodeEdgeHandler;
        private CreationWindow _creationWindow;


        public void ClearEditorView()
        {
            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements);

            nodes.ForEach(n => n.RemoveFromHierarchy());
            edges.ForEach(e => e.RemoveFromHierarchy());
        }


        public void OnGraphEditorView(BehaviourTree tree)
        {
            if (tree is not null)
            {
                this._tree = tree;
                this.Initialize(tree);
                this.IntegrityCheckAndCreateElements(tree);
            }
        }


        public NodeView FindNodeView(NodeBase node)
        {
            if (node is null || node.guid is null)
            {
                return null;
            }

            return this.GetNodeByGuid(node.guid) as NodeView;
        }


        public override List<Port> GetCompatiblePorts(Port input, NodeAdapter nodeAdapter)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            //direction은 input과 output이므로, 다른 노드라도 같은 포트에 못 꽂게 방지
            return ports.Where(output => input.direction != output.direction && input.node != output.node).ToList();
        }


        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (BehaviourTreeEditorWindow.Instance.CanEditTree == false)
            {
                return;
            }

            if (_creationWindow is null)
            {
                _creationWindow = ScriptableObject.CreateInstance<CreationWindow>();
                _creationWindow.Initialize(this);
            }

            Vector2 screenPoint = GUIUtility.GUIToScreenPoint(evt.mousePosition);
            SearchWindowContext context = new SearchWindowContext(screenPoint, 200, 240);

            SearchWindow.Open(context, _creationWindow);
        }


        public void SelectNode(NodeView nodeView)
        {
            base.ClearSelection();

            if (nodeView != null)
            {
                base.AddToSelection(nodeView);
            }
        }


        public NodeView CreateNewNodeAndView(Type type, Vector2 mousePosition)
        {
            NodeBase node = _tree.CreateNode(type);
            node.position = mousePosition;
            return this.RecreateNodeViewOnLoad(node);
        }



        public NodeGroupView CreateNewNodeGroupView(string title, Vector2 position)
        {
            GroupViewData nodeGroupData = _tree.groupViewDataCollection.CreateGroupData(title, position);
            NodeGroupView groupView = new NodeGroupView(_tree.groupViewDataCollection, nodeGroupData);

            groupView.SetPosition(new Rect(position, Vector2.zero));
            groupView.style.backgroundColor = BehaviourTreeEditorWindow.Settings.nodeGroupColor;
            groupView.title = title;

            base.AddElement(groupView);
            return groupView;
        }



        public void UpdateNodeView()
        {
            int length = nodes.Count();

            for (int i = 0; i < length; i++)
            {
                if (nodes.AtIndex(i) is NodeView nodeView)
                {
                    nodeView.UpdateState();
                }
            }
        }


        private void Initialize(BehaviourTree tree)
        {
            onGraphViewChange?.Invoke();

            graphViewChanged -= this.OnGraphViewChanged;
            this.deleteSelection -= this.OnDeleteSelectionElements;
            base.DeleteElements(base.graphElements);
            graphViewChanged += this.OnGraphViewChanged;
            this.deleteSelection += this.OnDeleteSelectionElements;
        }


        private void IntegrityCheckAndCreateElements(BehaviourTree tree)
        {
            for (int i = 0; i < tree.nodeList.Count; ++i)
            {
                //Undo로 생성이 취소된 노드를 여기서 처리.
                if (tree.nodeList[i] is null)
                {
                    tree.nodeList.RemoveAt(i);
                }
            }

            //트리 구조라서 미리 모두 생성해둬야 자식과 부모를 연결 할 수 있음.
            tree.nodeList.ForEach(n => this.RecreateNodeViewOnLoad(n));
            tree.nodeList.ForEach(n => _nodeEdgeHandler.ConnectEdges(this, n, tree.GetChildren(n)));

            tree.groupViewDataCollection.dataList?.ForEach(this.RecreateNodeGroupViewOnLoad);
        }


        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove is not null)
            {
                foreach (var element in graphViewChange.elementsToRemove)
                {
                    switch (element)
                    {
                        case Edge edge: this._nodeEdgeHandler.DeleteEdges(_tree, edge); break; 

                        case NodeView nodeView: this._tree.DeleteNode(nodeView.node); break;
                        
                        case NodeGroupView groupView: this._tree.groupViewDataCollection.DeleteGroupData(groupView.viewData); break;
                    }
                }
            }

            if (graphViewChange.edgesToCreate is not null)
            {
                this._nodeEdgeHandler.ConnectEdges(_tree, graphViewChange.edgesToCreate);
            }

            if (graphViewChange.movedElements is not null)
            {
                base.nodes.ForEach(n => (n as NodeView)?.SortChildren());
            }

            return graphViewChange;
        }
        
        
        private void OnDeleteSelectionElements(string operationName, AskUser user)
        {
            for (int i = 0; i < selection.Count; ++i)
            {
                if (selection[i] is NodeView view && view.node.nodeType == NodeBase.ENodeType.Root)
                {
                    view.selected = false;
                    selection.RemoveAt(i);
                    break;
                }
            }

            this.DeleteElements(selection.ConvertAll(e => (GraphElement)e));
        }


        private NodeView RecreateNodeViewOnLoad(NodeBase node)
        {
            NodeView nodeView = new NodeView(node, BehaviourTreeEditorWindow.Settings.nodeViewXml);
            nodeView.OnNodeSelected += this.onNodeSelected;

            base.AddElement(nodeView); //nodes라는 GraphElement 컨테이너에 추가.
            return nodeView;
        }


        private void RecreateNodeGroupViewOnLoad(GroupViewData data)
        {
            NodeGroupView nodeGroupView = new NodeGroupView(_tree.groupViewDataCollection, data);
            nodeGroupView.SetPosition(new Rect(data.position, Vector2.zero));
            nodeGroupView.title = data.title;
            base.AddElement(nodeGroupView);

            nodes.Where(n => n is NodeView v && data.Contains(v.node.guid))
                 .ForEach(filteredNode => nodeGroupView.AddElement(filteredNode));
        }


        public void SearchNodeByNameOrTag(ChangeEvent<string> changeEvent)
        {
            if (_nodeSearchHelper.HasSyntaxes(changeEvent.newValue, out var syntaxes))
            {
                popupSearchField.menu.ClearItems();

                NodeView[] views = null;

                if (syntaxes.Length == 1)
                {
                    views = _nodeSearchHelper.GetNodeView(syntaxes[0], NodeSearchHelper.ESearchOptions.Both, nodes);
                }
                else if (string.Compare(syntaxes[0], "t:", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    views = _nodeSearchHelper.GetNodeView(syntaxes[1], NodeSearchHelper.ESearchOptions.Tag, nodes);
                }
                else if (string.Compare(syntaxes[0], "n:", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    views = _nodeSearchHelper.GetNodeView(syntaxes[1], NodeSearchHelper.ESearchOptions.Name, nodes);
                }

                if (views is null)
                {
                    return;
                }

                for (int i = 0; i < views.Length; ++i)
                {
                    NodeView view = views[i];

                    string menuName = $"[{i + 1}]   name: [{view.node.name}]   tag: [{view.node.tag}]";

                    popupSearchField.menu.AppendAction(menuName, delegate
                    {
                        this.SelectNode(view);
                        base.FrameSelection();
                    });
                }

                EditorApplication.delayCall -= popupSearchField.ShowMenu;
                EditorApplication.delayCall += popupSearchField.ShowMenu;
            }
        }
    }
}