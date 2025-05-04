using System;

namespace BehaviourSystem.BT
{
    [Serializable]
    public abstract class ActionNode : NodeBase
    {
        public override ENodeType baseType
        {
            get { return ENodeType.Action; }
        }
    }
}