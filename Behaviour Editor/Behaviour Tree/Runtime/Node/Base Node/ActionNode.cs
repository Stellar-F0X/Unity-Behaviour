using System;

namespace BehaviourSystem.BT
{
    [Serializable]
    public abstract class ActionNode : NodeBase
    {
        public override ENodeType nodeType
        {
            get { return ENodeType.Action; }
        }
    }
}