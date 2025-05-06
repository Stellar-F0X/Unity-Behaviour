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
    }
}