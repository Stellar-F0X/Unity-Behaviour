using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.Serialization;

namespace BehaviourSystem.BT
{
    [CreateAssetMenu]
    public sealed class BehaviourTree : ScriptableObject, IEquatable<BehaviourTree>
    {
        [HideInInspector]
        public BehaviourNodeSet nodeSet;

        [HideInInspector]
        public Blackboard blackboard;

        [HideInInspector]
        public GroupDataSet groupDataSet;

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

        public static BehaviourTree MakeRuntimeTree(BehaviourActor actor, BehaviourTree targetTree, Stack<NodeBase> callStack)
        {
            if (targetTree != null)
            {
                BehaviourTree runtimeTree = Instantiate(targetTree);

                runtimeTree._specificGuid = Guid.NewGuid().ToString();
                runtimeTree._cloneGroupID = targetTree._cloneGroupID;

                runtimeTree.nodeSet = targetTree.nodeSet.Clone(actor, callStack);
                runtimeTree.blackboard = targetTree.blackboard.Clone();
                runtimeTree.groupDataSet = targetTree.groupDataSet.Clone();
                return runtimeTree;
            }

            return null;
        }

        #endregion


        public void Awake()
        {
            _cloneGroupID ??= Guid.NewGuid().ToString();
            _specificGuid ??= Guid.NewGuid().ToString();

#if UNITY_EDITOR
            if (nodeSet is null)
            {
                this.nodeSet = CreateInstance<BehaviourNodeSet>();
                this.nodeSet.hideFlags = HideFlags.HideInHierarchy;
                AssetDatabase.AddObjectToAsset(this.nodeSet, this);

                if (this.nodeSet.rootNode is null)
                {
                    this.nodeSet.rootNode = this.nodeSet.CreateNode(typeof(RootNode));
                    EditorUtility.SetDirty(this);
                }

                AssetDatabase.SaveAssets();
            }

            if (blackboard is null)
            {
                this.blackboard = CreateInstance<Blackboard>();
                this.blackboard.hideFlags = HideFlags.HideInHierarchy;
                AssetDatabase.AddObjectToAsset(this.blackboard, this);
                AssetDatabase.SaveAssets();
            }

            if (groupDataSet is null)
            {
                groupDataSet = CreateInstance<GroupDataSet>();
                groupDataSet.hideFlags = HideFlags.HideInHierarchy;
                AssetDatabase.AddObjectToAsset(groupDataSet, this);
                AssetDatabase.SaveAssets();
            }
#endif
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

            if (this.nodeSet.rootNode.guid == other.nodeSet.rootNode.guid && _specificGuid == other._specificGuid)
            {
                return true;
            }

            return false;
        }
    }
}