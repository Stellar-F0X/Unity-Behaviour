using System;
using UnityEngine;

namespace BehaviourSystem.BT
{
    public sealed class SequencerNode : CompositeNode
    {
        protected override void OnEnter()
        {
            _currentChildIndex = 0;
        }


        protected override EBehaviourResult OnUpdate()
        {
            if (children is null || children.Count == 0)
            {
                return EBehaviourResult.Failure;
            }

            switch (children[_currentChildIndex].UpdateNode())
            {
                case EBehaviourResult.Running: return EBehaviourResult.Running;

                case EBehaviourResult.Failure: return EBehaviourResult.Failure;

                case EBehaviourResult.Success: _currentChildIndex++; break;
            }

            if (_currentChildIndex == children.Count)
            {
                return EBehaviourResult.Success;
            }

            return EBehaviourResult.Running;
        }
    }
}