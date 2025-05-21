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

        protected override EBehaviourResult OnUpdate()
        {
            if (child == null)
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