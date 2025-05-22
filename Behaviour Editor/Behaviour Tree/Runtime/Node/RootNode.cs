using System.Collections.Generic;
using UnityEngine;

namespace BehaviourSystem.BT
{
    public sealed class RootNode : NodeBase, IBehaviourIterable
    {
        [HideInInspector]
        public NodeBase child;

        public override ENodeType nodeType
        {
            get { return ENodeType.Root; }
        }
        
        public int childCount
        {
            get { return 1; }
        }

        
        protected override EBehaviourResult OnUpdate()
        {
            if (child is null)
            {
                return EBehaviourResult.Failure;
            }
            else
            {
                return child.UpdateNode();
            }
        }


        public IEnumerable<NodeBase> GetChildren()
        {
            return new NodeBase[] { child };
        }
    }
}