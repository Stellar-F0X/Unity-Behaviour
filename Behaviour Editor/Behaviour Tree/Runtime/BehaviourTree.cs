using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace BehaviourSystem.BT
{
    [CreateAssetMenu]
    public sealed class BehaviourTree : ScriptableObject, IEquatable<BehaviourTree>
    {
        [HideInInspector]
        public NodeBase rootNode;

        [HideInInspector]
        public List<NodeBase> nodeList = new List<NodeBase>();

        [HideInInspector]
        public BlackboardData blackboardData;

        [HideInInspector]
        public GroupViewDataCollection groupViewDataCollection;

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


        #region Make Runtime Tree

        public static BehaviourTree MakeRuntimeTree(BehaviourActor actor, BehaviourTree targetTree)
        {
            if (targetTree is null)
            {
                return null;
            }
            
            Stack<(NodeBase instance, NodeBase origin)> stack = new Stack<(NodeBase instance, NodeBase origin)>();
            Stack<NodeBase> callStack = new Stack<NodeBase>(targetTree.nodeList.Count);
            BehaviourTree runtimeTree = Instantiate(targetTree);

            runtimeTree._specificGuid = Guid.NewGuid().ToString();
            runtimeTree._cloneGroupID = targetTree._cloneGroupID;
            runtimeTree.nodeList = new List<NodeBase>(targetTree.nodeList.Count);
            runtimeTree.blackboardData = BlackboardData.Clone(targetTree.blackboardData);

            if (Instantiate(targetTree.rootNode) is RootNode newRootNode)
            {
                runtimeTree.rootNode = newRootNode;
                stack.Push((newRootNode, targetTree.rootNode));

                while (stack.Count > 0)
                {
                    (NodeBase instance, NodeBase origin) traversal = stack.Pop();

                    traversal.instance.name = traversal.instance.name.Remove(traversal.origin.name.Length, 7);
                    traversal.instance.tree = runtimeTree;
                    traversal.instance.actor = actor;
                    traversal.instance.callStack = callStack;

                    runtimeTree.nodeList.Add(traversal.instance);

                    switch (traversal.origin.nodeType)
                    {
                        case NodeBase.ENodeType.Root:
                        {
                            RootNode instance = (RootNode)traversal.instance;
                            RootNode origin = (RootNode)traversal.origin;

                            if (origin.child is null)
                            {
                                continue;
                            }
                            
                            NodeBase childInstance = Instantiate(origin.child);
                            childInstance.parent = instance;
                            instance.child = childInstance;
                            stack.Push((childInstance, origin.child));
                            break;
                        }

                        case NodeBase.ENodeType.Decorator:
                        {
                            DecoratorNode instance = (DecoratorNode)traversal.instance;
                            DecoratorNode origin = (DecoratorNode)traversal.origin;
                            
                            if (origin.child is null)
                            {
                                continue;
                            }
                            
                            NodeBase childInstance = Instantiate(origin.child);
                            childInstance.parent = instance;
                            instance.child = childInstance;
                            stack.Push((childInstance, origin.child));
                            break;
                        }

                        case NodeBase.ENodeType.Composite:
                        {
                            CompositeNode instance = (CompositeNode)traversal.instance;
                            CompositeNode origin = (CompositeNode)traversal.origin;
                            
                            if (origin.children is null)
                            {
                                continue;
                            }

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

        #endregion


        public void Awake()
        {
            _cloneGroupID ??= Guid.NewGuid().ToString();
            _specificGuid ??= Guid.NewGuid().ToString();

#if UNITY_EDITOR
            if (blackboardData is null)
            {
                this.blackboardData = CreateInstance<BlackboardData>();
                this.blackboardData.hideFlags = HideFlags.HideInHierarchy;
                AssetDatabase.AddObjectToAsset(this.blackboardData, this);
                AssetDatabase.SaveAssets();
            }

            if (groupViewDataCollection is null)
            {
                groupViewDataCollection = CreateInstance<GroupViewDataCollection>();
                groupViewDataCollection.hideFlags = HideFlags.HideInHierarchy;
                AssetDatabase.AddObjectToAsset(groupViewDataCollection, this);
                AssetDatabase.SaveAssets();
            }
#endif
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

        
        public bool Equals(BehaviourTree other)
        {
            if (other is null || this.GetType() != other.GetType())
            {
                return false;
            }

            if (rootNode.guid == other.rootNode.guid && _specificGuid == other._specificGuid)
            {
                return true;
            }

            return false;
        }


#if UNITY_EDITOR
        public void AddChild(NodeBase parent, NodeBase child)
        {
            Undo.RecordObject(parent, "Behaviour Tree (AddChild)");

            switch (parent.nodeType)
            {
                case NodeBase.ENodeType.Root:
                    ((RootNode)parent).child = child;
                    child.parent = parent;
                    break;

                case NodeBase.ENodeType.Decorator:
                    ((DecoratorNode)parent).child = child;
                    child.parent = parent;
                    break;

                case NodeBase.ENodeType.Composite:
                    ((CompositeNode)parent).children.Add(child);
                    child.parent = parent;
                    break;
            }

            EditorUtility.SetDirty(parent);
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
            node.hideFlags = HideFlags.HideInHierarchy;
            nodeList.Add(node);

            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.RecordObject(this, "Behaviour Tree (CreateNode)");
                Undo.RegisterCreatedObjectUndo(node, "Behaviour Tree (CreateNode)");

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