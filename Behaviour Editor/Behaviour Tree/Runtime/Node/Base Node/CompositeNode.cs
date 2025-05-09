using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourSystem.BT
{
    [Serializable]
    public abstract class CompositeNode : NodeBase
    {
        [HideInInspector]
        public List<NodeBase> children = new List<NodeBase>();

        protected int _currentChildIndex;


        public override ENodeType nodeType
        {
            get { return ENodeType.Composite; }
        }

        public int currentChildIndex
        {
            get { return _currentChildIndex; }
        }

        public override sealed void FixedUpdateNode()
        {
            if (children is not null && currentChildIndex < children.Count)
            {
                children[_currentChildIndex].FixedUpdateNode();
            }
        }

        public override sealed void GizmosUpdateNode()
        {
            if (children is not null && currentChildIndex < children.Count)
            {
                children[_currentChildIndex].GizmosUpdateNode();
            }
        }
    }
}