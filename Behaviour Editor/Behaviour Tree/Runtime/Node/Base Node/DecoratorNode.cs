using UnityEngine;
using System;

namespace BehaviourSystem.BT
{
    [Serializable]
    public abstract class DecoratorNode : NodeBase
    {
        [HideInInspector]
        public NodeBase child;

        public override ENodeType nodeType
        {
            get { return ENodeType.Decorator; }
        }

        public override sealed void FixedUpdateNode()
        {
            if (child is not null)
            {
                child.FixedUpdateNode();
            }
        }

        public override sealed void GizmosUpdateNode()
        {
            if (child is not null)
            {
                child.GizmosUpdateNode();
            }
        }
    }
}