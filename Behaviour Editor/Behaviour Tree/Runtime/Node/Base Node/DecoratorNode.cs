using UnityEngine;
using System;
using System.Collections.Generic;

namespace BehaviourSystem.BT
{
    [Serializable]
    public abstract class DecoratorNode : NodeBase, IBehaviourIterable
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

        public IEnumerable<NodeBase> GetChildren()
        {
            return new NodeBase[] { child };
        }
    }
}