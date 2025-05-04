using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace BehaviourSystem.BT
{
    [CreateAssetMenu]
    public class BehaviourTree : ScriptableObject, IEqualityComparer<BehaviourTree>
    {
        [HideInInspector]
        public NodeBase rootNodeBase;

        [HideInInspector]
        public NodeBase.EState treeState = NodeBase.EState.Running;

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


        public void OnEnable()
        {
            _cloneGroupID = GUID.Generate().ToString();
            _specificGuid = GUID.Generate().ToString();
        }


        public NodeBase.EState UpdateTree(BehaviourActor behaviourActor)
        {
            if (rootNodeBase.state == NodeBase.EState.Running)
            {
                treeState = rootNodeBase.UpdateNode(behaviourActor, new PreviusBehaviourInfo(string.Empty, null, NodeBase.ENodeType.Root));
            }

            return treeState;
        }


        public bool Equals(BehaviourTree x, BehaviourTree y)
        {
            if (x is null || y is null || x.GetType() != y.GetType())
            {
                return false;
            }

            return x.rootNodeBase.guid == y.rootNodeBase.guid && x._specificGuid == y._specificGuid;
        }


        public int GetHashCode(BehaviourTree obj)
        {
            return HashCode.Combine(obj.rootNodeBase, obj._specificGuid);
        }


#if UNITY_EDITOR
        public NodeBase CreateNode(Type nodeType)
        {
            NodeBase node = ScriptableObject.CreateInstance(nodeType) as NodeBase;
            node.name = nodeType.Name.Replace("Node", string.Empty);
            node.guid = GUID.Generate().ToString();
            nodeList.Add(node);

            if (!Application.isPlaying && !Undo.isProcessing)
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


        public void AddChild(NodeBase parent, NodeBase child)
        {
            Undo.RecordObject(parent, "Behaviour Tree (AddChild)");

            switch (parent.baseType)
            {
                case NodeBase.ENodeType.Root: (parent as RootNode).child = child; break;

                case NodeBase.ENodeType.Decorator: (parent as DecoratorNode).child = child; break;

                case NodeBase.ENodeType.Composite: (parent as CompositeNode).children.Add(child); break;
            }

            EditorUtility.SetDirty(child);
        }


        public void RemoveChild(NodeBase parent, NodeBase child)
        {
            Undo.RecordObject(parent, "Behaviour Tree (RemoveChild)");

            switch (parent.baseType)
            {
                case NodeBase.ENodeType.Root: (parent as RootNode).child = null; break;

                case NodeBase.ENodeType.Decorator: (parent as DecoratorNode).child = null; break;

                case NodeBase.ENodeType.Composite: (parent as CompositeNode).children.Remove(child); break;
            }

            EditorUtility.SetDirty(child);
        }


        public List<NodeBase> GetChildren(NodeBase parent)
        {
            switch (parent.baseType)
            {
                case NodeBase.ENodeType.Root:
                    return new List<NodeBase>()
                    {
                        (parent as RootNode).child
                    };

                case NodeBase.ENodeType.Decorator:
                    return new List<NodeBase>()
                    {
                        (parent as DecoratorNode).child
                    };

                case NodeBase.ENodeType.Composite: return (parent as CompositeNode).children;

                default: return new List<NodeBase>(0);
            }
        }


        private void Traverse(NodeBase node, Action<NodeBase> visiter)
        {
            if (node == null)
            {
                return;
            }

            visiter.Invoke(node);
            var children = this.GetChildren(node);
            children.ForEach(node => this.Traverse(node, visiter));
        }


        public virtual BehaviourTree Clone()
        {
            BehaviourTree tree = Object.Instantiate(this);
            tree._specificGuid = GUID.Generate().ToString();
            tree._cloneGroupID = this._cloneGroupID;

            tree.rootNodeBase = tree.rootNodeBase.Clone();
            tree.nodeList = new List<NodeBase>();

            this.Traverse(tree.rootNodeBase, n => tree.nodeList.Add(n));
            return tree;
        }
#endif
    }
}