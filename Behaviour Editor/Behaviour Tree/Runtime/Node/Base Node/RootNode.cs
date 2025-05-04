using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourSystem.BT
{
    public class RootNode : NodeBase
    {
        [HideInInspector]
        public NodeBase child;

        public override ENodeType baseType
        {
            get { return ENodeType.Root; }
        }

        protected override void OnEnter(BehaviourActor behaviourTree, PreviusBehaviourInfo info) { }

        protected override EState OnUpdate(BehaviourActor behaviourTree, PreviusBehaviourInfo info)
        {
            return child.UpdateNode(behaviourTree, new PreviusBehaviourInfo(tag, this.GetType(), baseType));
        }

        protected override void OnExit(BehaviourActor behaviourTree, PreviusBehaviourInfo info) { }

        public override NodeBase Clone()
        {
            RootNode node = base.Clone() as RootNode;

            if (this.child != null)
            {
                node.child = this.child.Clone();
            }

            return node;
        }
    }
}