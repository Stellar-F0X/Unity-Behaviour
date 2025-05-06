using UnityEngine;

namespace BehaviourSystem.BT
{
    public class RootNode : NodeBase
    {
        [HideInInspector]
        public NodeBase child;

        public override ENodeType nodeType
        {
            get { return ENodeType.Root; }
        }

        protected override EBehaviourResult OnUpdate()
        {
            return child.UpdateNode();
        }
    }
}