using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace BehaviourSystem.BT
{
    [CreateAssetMenu]
    public sealed class BehaviourTree : ScriptableObject, IEqualityComparer<BehaviourTree>
    {
        [HideInInspector]
        public NodeBase rootNode;

        [HideInInspector]
        public List<NodeBase> nodeList = new List<NodeBase>();

        [HideInInspector]
        public BlackboardData blackboardData = new BlackboardData();

        [SerializeField, ReadOnly, Tooltip("개별 트리를 구분하기 위한 고유 ID")]
        private string _specificGuid;

        [SerializeField, ReadOnly, Tooltip("복제 트리를 구분하기 위한 GUID")]
        private string _cloneGroupID;


        private string specificGuid
        {
            get { return _specificGuid; }
        }

        public string cloneGroupID
        {
            get { return _cloneGroupID; }
        }
        
        
        public static BehaviourTree MakeRuntimeTree(BehaviourActor actor, BehaviourTree targetTree)
        {
            Stack<(NodeBase parent, NodeBase child)> stack = new Stack<(NodeBase parent, NodeBase child)>();
            Stack<NodeBase> callStack = new Stack<NodeBase>(targetTree.nodeList.Count);
            BehaviourTree runtimeTree = Instantiate(targetTree);
            runtimeTree._specificGuid  = GUID.Generate().ToString();
            runtimeTree._cloneGroupID  = targetTree._cloneGroupID;
            runtimeTree.nodeList       = new List<NodeBase>(targetTree.nodeList.Count);
            runtimeTree.blackboardData = BlackboardData.Clone(targetTree.blackboardData);

            stack.Push((Instantiate(targetTree.rootNode), ((RootNode)targetTree.rootNode).child));

            while (stack.Count > 0)
            {
                (NodeBase parent, NodeBase child) traversal = stack.Pop();
                NodeBase clonedNode = Instantiate(traversal.child);

                clonedNode.tree       = runtimeTree;
                clonedNode.actor      = actor;
                clonedNode.parent     = traversal.parent;
                clonedNode.callStack = callStack;

                runtimeTree.nodeList.Add(clonedNode);
                runtimeTree.AddChild(traversal.parent, clonedNode);

                List<NodeBase> children = targetTree.GetChildren(traversal.child);

                if (children is null || children.Count == 0)
                {
                    continue;
                }

                for (int i = children.Count - 1; i >= 0; --i)
                {
                    stack.Push((clonedNode, children[i]));
                }
            }

            return runtimeTree;
        }


        public void OnEnable()
        {
            _cloneGroupID = GUID.Generate().ToString();
            _specificGuid = GUID.Generate().ToString();
        }


        public NodeBase.EBehaviourResult UpdateTree()
        {
            if (this.rootNode is null)
            {
                Debug.LogError("Root node is NullReference");
                return NodeBase.EBehaviourResult.Failure;
            }
            
            return this.rootNode.UpdateNode();
        }


        public List<NodeBase> GetChildren(NodeBase parent)
        {
            switch (parent.nodeType)
            {
                case NodeBase.ENodeType.Root: return new List<NodeBase> { ((RootNode)parent).child };

                case NodeBase.ENodeType.Decorator: return new List<NodeBase> { ((DecoratorNode)parent).child };

                case NodeBase.ENodeType.Composite: return ((CompositeNode)parent).children;

                default: return null;
            }
        }


        public bool Equals(BehaviourTree x, BehaviourTree y)
        {
            if (x is null || y is null || x.GetType() != y.GetType())
            {
                return false;
            }

            return x.rootNode.guid == y.rootNode.guid && x._specificGuid == y._specificGuid;
        }


        public int GetHashCode(BehaviourTree obj)
        {
            return HashCode.Combine(obj.rootNode, obj._specificGuid);
        }


        public void AddChild(NodeBase parent, NodeBase child)
        {
#if UNITY_EDITOR
            Undo.RecordObject(parent, "Behaviour Tree (AddChild)");
#endif

            switch (parent.nodeType)
            {
                case NodeBase.ENodeType.Root: ((RootNode)parent).child = child; break;

                case NodeBase.ENodeType.Decorator: ((DecoratorNode)parent).child = child; break;

                case NodeBase.ENodeType.Composite: ((CompositeNode)parent).children.Add(child); break;
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(child);
#endif
        }


        public void RemoveChild(NodeBase parent, NodeBase child)
        {
#if UNITY_EDITOR
            Undo.RecordObject(parent, "Behaviour Tree (RemoveChild)");
#endif
            switch (parent.nodeType)
            {
                case NodeBase.ENodeType.Root: ((RootNode)parent).child = null; break;

                case NodeBase.ENodeType.Decorator: ((DecoratorNode)parent).child = null; break;

                case NodeBase.ENodeType.Composite: ((CompositeNode)parent).children.Remove(child); break;
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(child);
#endif
        }


#if UNITY_EDITOR
        public NodeBase CreateNode(Type nodeType)
        {
            NodeBase node = CreateInstance(nodeType) as NodeBase;

            if (node is null)
            {
                throw new NullReferenceException("Failed to create a node. The node instance is null.");
            }

            //(?<!^)는 문자열의 시작이 아닌 위치에서만 매칭하며 ([A-Z])는 대문자를 찾는다. " $1"는 대문자 앞에 공백을 추가함.
            node.name = Regex.Replace(nodeType.Name.Replace("Node", ""), "(?<!^)([A-Z])", " $1");
            node.guid = GUID.Generate().ToString();
            nodeList.Add(node);

            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.RecordObject(this, "Behaviour Tree (CreateNode)");
                Undo.RegisterCreatedObjectUndo(node, "Behaviour Tree (CreateNode)");

                node.hideFlags = HideFlags.HideInHierarchy;
                AssetDatabase.AddObjectToAsset(node, this);
                AssetDatabase.SaveAssets();
            }

            return node;
        }


        public void DeleteNode(NodeBase node)
        {
            Undo.RecordObject(this, "Behaviour Tree (DeleteNode)");
            nodeList.Remove(node);

            Undo.DestroyObjectImmediate(node);
            AssetDatabase.SaveAssets();
        }
#endif
    }
}