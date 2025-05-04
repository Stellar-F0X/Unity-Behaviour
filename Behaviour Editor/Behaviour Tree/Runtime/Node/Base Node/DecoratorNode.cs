using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace BehaviourSystem.BT
{
    [Serializable]
    public abstract class DecoratorNode : NodeBase
    {
        [HideInInspector]
        public NodeBase child;

        public override ENodeType baseType
        {
            get { return ENodeType.Decorator; }
        }

        public override NodeBase Clone()
        {
            DecoratorNode node = base.Clone() as DecoratorNode;
            node.child = this.child.Clone();
            return node;
        }
    }
}