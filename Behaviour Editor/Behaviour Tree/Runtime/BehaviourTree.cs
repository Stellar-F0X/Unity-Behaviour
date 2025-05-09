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
        public BlackboardData blackboardData;

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
            Stack<(NodeBase instance, NodeBase origin)> stack = new Stack<(NodeBase instance, NodeBase origin)>();
            Stack<NodeBase> callStack = new Stack<NodeBase>(targetTree.nodeList.Count);
            BehaviourTree runtimeTree = Instantiate(targetTree);

            runtimeTree._specificGuid  = GUID.Generate().ToString();
            runtimeTree._cloneGroupID  = targetTree._cloneGroupID;
            runtimeTree.nodeList       = new List<NodeBase>(targetTree.nodeList.Count);
            runtimeTree.blackboardData = BlackboardData.Clone(targetTree.blackboardData);

            if (Instantiate(targetTree.rootNode) is RootNode newRootNode)
            {
                runtimeTree.rootNode = newRootNode;
                stack.Push((newRootNode, targetTree.rootNode));

                while (stack.Count > 0)
                {
                    (NodeBase instance, NodeBase origin) traversal = stack.Pop();

                    traversal.instance.name      = traversal.instance.name.Remove(traversal.origin.name.Length, 7);
                    traversal.instance.tree      = runtimeTree;
                    traversal.instance.actor     = actor;
                    traversal.instance.callStack = callStack;

                    runtimeTree.nodeList.Add(traversal.instance);

                    switch (traversal.origin.nodeType)
                    {
                        case NodeBase.ENodeType.Root:
                        {
                            RootNode instance = (RootNode)traversal.instance;
                            RootNode origin = (RootNode)traversal.origin;
                            NodeBase childInstance = Instantiate(origin.child);
                            childInstance.parent = instance;
                            instance.child       = childInstance;
                            stack.Push((childInstance, origin.child));
                            break;
                        }

                        case NodeBase.ENodeType.Decorator:
                        {
                            DecoratorNode instance = (DecoratorNode)traversal.instance;
                            DecoratorNode origin = (DecoratorNode)traversal.origin;
                            NodeBase childInstance = Instantiate(origin.child);
                            childInstance.parent = instance;
                            instance.child       = childInstance;
                            stack.Push((childInstance, origin.child));
                            break;
                        }

                        case NodeBase.ENodeType.Composite:
                        {
                            CompositeNode instance = (CompositeNode)traversal.instance;
                            CompositeNode origin = (CompositeNode)traversal.origin;

                            for (int i = 0; i < origin.children.Count; ++i)
                            {
                                NodeBase childInstance = Instantiate(origin.children[i]);
                                childInstance.parent = instance;
                                instance.children[i] = childInstance;
                                stack.Push((childInstance, origin.children[i]));
                            }
                            break;
                        }
                    }

                    traversal.instance.OnInitialize();
                }
            }

            return runtimeTree;
        }


        public void Awake()
        {
            if (string.IsNullOrEmpty(_cloneGroupID))
            {
                _cloneGroupID = GUID.Generate().ToString();
            }

            if (string.IsNullOrEmpty(_specificGuid))
            {
                _specificGuid = GUID.Generate().ToString();
            }

            if (blackboardData is null)
            {
                this.blackboardData           = CreateInstance<BlackboardData>();
                this.blackboardData.hideFlags = HideFlags.HideInHierarchy;

#if UNITY_EDITOR
                EditorUtility.SetDirty(this.blackboardData);
                AssetDatabase.AddObjectToAsset(this.blackboardData, this);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
#endif
            }
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


        public void FixedUpdateTree()
        {
            if (this.rootNode is null)
            {
                Debug.LogError("Root node is NullReference");
                return;
            }

            this.rootNode.FixedUpdateNode();
        }


        public void GizmosUpdateTree()
        {
            if (this.rootNode is null)
            {
                Debug.LogError("Root node is NullReference");
                return;
            }

            this.rootNode.GizmosUpdateNode();
        }


        public List<NodeBase> GetChildren(NodeBase parent)
        {
            switch (parent.nodeType)
            {
                case NodeBase.ENodeType.Root: return new List<NodeBase> { ((RootNode)parent).child };

                case NodeBase.ENodeType.Decorator: return new List<NodeBase> { ((DecoratorNode)parent).child };

                case NodeBase.ENodeType.Composite: return ((CompositeNode)parent).children;
            }

            return null;
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


#if UNITY_EDITOR
        public void AddChild(NodeBase parent, NodeBase child)
        {
            Undo.RecordObject(parent, "Behaviour Tree (AddChild)");

            switch (parent.nodeType)
            {
                case NodeBase.ENodeType.Root:
                    ((RootNode)parent).child = child;
                    child.parent             = parent;
                    break;

                case NodeBase.ENodeType.Decorator:
                    ((DecoratorNode)parent).child = child;
                    child.parent                  = parent;
                    break;

                case NodeBase.ENodeType.Composite:
                    ((CompositeNode)parent).children.Add(child);
                    child.parent = parent;
                    break;
            }

            EditorUtility.SetDirty(child);
        }


        public void RemoveChild(NodeBase parent, NodeBase child)
        {
            Undo.RecordObject(parent, "Behaviour Tree (RemoveChild)");

            switch (parent.nodeType)
            {
                case NodeBase.ENodeType.Root: ((RootNode)parent).child = null; break;

                case NodeBase.ENodeType.Decorator: ((DecoratorNode)parent).child = null; break;

                case NodeBase.ENodeType.Composite: ((CompositeNode)parent).children.Remove(child); break;
            }

            EditorUtility.SetDirty(child);
        }


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