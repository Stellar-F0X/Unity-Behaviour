using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourSystem.BT
{
    [Serializable]
    public abstract class CompositeNode : NodeBase
    {
        [HideInInspector]
        public List<NodeBase> children = new List<NodeBase>();

        public override ENodeType baseType
        {
            get { return ENodeType.Composite; }
        }

        public override NodeBase Clone()
        {
            CompositeNode node = base.Clone() as CompositeNode;
            node.children = children.ConvertAll(c => c.Clone());
            return node;
        }
    }
}