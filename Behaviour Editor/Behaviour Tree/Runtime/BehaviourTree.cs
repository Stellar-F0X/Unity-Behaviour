using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

                foreach (var nodeBase in runtimeTree.nodeSet.nodeList)
                {
                    Type nodeType = nodeBase.GetType();

                    if (ReflectionHelper.FieldCacher.TryGetValue(nodeType, out FieldInfo[] infos) == false)
                    {
                        infos = nodeType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                        .Where(f => typeof(IBlackboardProperty).IsAssignableFrom(f.FieldType))
                                        .ToArray();

                        ReflectionHelper.FieldCacher[nodeType] = infos;
                    }

                    foreach (var info in infos)
                    {
                        if (typeof(IBlackboardProperty).IsAssignableFrom(info.FieldType))
                        {
                            ReflectionHelper.FieldAccessor accessor = ReflectionHelper.GetAccessor(info);

                            if (accessor.getter(nodeBase) is IBlackboardProperty property)
                            {
                                IBlackboardProperty foundProperty = runtimeTree.blackboard.FindProperty(property.key);

                                if (foundProperty != null)
                                {
                                    accessor.setter(nodeBase, foundProperty);
                                }
                            }
                        }
                    }
                }

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