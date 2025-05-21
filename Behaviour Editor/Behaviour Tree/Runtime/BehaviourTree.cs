﻿using System;
using UnityEngine;

namespace BehaviourSystem.BT
{
    [CreateAssetMenu(fileName = "Behaviour Tree", menuName = "Behaviour Tree/Tree Asset")]
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


        public static BehaviourTree MakeRuntimeTree(BehaviourTreeRunner treeRunner, BehaviourTree targetTree)
        {
            if (treeRunner == null)
            {
                Debug.LogError("BehaviourActor is null.");
                return null;
            }

            if (targetTree == null)
            {
                Debug.LogError("BehaviourTree is null.");
                return null;
            }

            BehaviourTree runtimeTree = Instantiate(targetTree);

            runtimeTree.blackboard = targetTree.blackboard.Clone();
            runtimeTree.groupDataSet = targetTree.groupDataSet.Clone();
            runtimeTree.nodeSet = targetTree.nodeSet.Clone(treeRunner);
            return runtimeTree;
        }


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