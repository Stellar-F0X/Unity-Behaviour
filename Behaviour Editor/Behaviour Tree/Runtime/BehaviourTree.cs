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


        [field: SerializeField, ReadOnly]
        public string treeGuid
        {
            get;
            internal set;
        }


#region Make Runtime Tree

        public static BehaviourTree MakeRuntimeTree(BehaviourActor actor, BehaviourTree targetTree)
        {
            if (targetTree != null)
            {
                BehaviourTree runtimeTree = Instantiate(targetTree);

                runtimeTree.nodeSet = targetTree.nodeSet.Clone(actor);
                runtimeTree.blackboard = targetTree.blackboard.Clone();
                runtimeTree.groupDataSet = targetTree.groupDataSet.Clone();
                return runtimeTree;
            }

            return null;
        }

#endregion

        public bool Equals(BehaviourTree other)
        {
            if (other is null || this.GetType() != other.GetType())
            {
                return false;
            }

            if (string.CompareOrdinal(this.treeGuid, other.treeGuid) != 0)
            {
                return false;
            }

            if (string.CompareOrdinal(this.nodeSet.rootNode.guid, other.nodeSet.rootNode.guid) != 0)
            {
                return false;
            }

            return true;
        }
    }
}