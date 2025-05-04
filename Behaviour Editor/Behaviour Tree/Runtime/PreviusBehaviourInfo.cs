using System;

namespace BehaviourSystem.BT
{
    [Serializable]
    public struct PreviusBehaviourInfo
    {
        public PreviusBehaviourInfo(string tag, Type nodeType, NodeBase.ENodeType basedType)
        {
            this.tag       = tag;
            this.nodeType  = nodeType;
            this.basedType = basedType;
        }

        public string tag;
        public Type nodeType;
        public NodeBase.ENodeType basedType;
    }
}