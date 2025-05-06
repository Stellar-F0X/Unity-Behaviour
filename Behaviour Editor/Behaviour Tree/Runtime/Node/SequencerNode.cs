using System;

namespace BehaviourSystem.BT
{
    public class SequencerNode : CompositeNode
    {
        protected override void OnEnter()
        {
            _currentChildIndex = 0;
        }


        protected override EBehaviourResult OnUpdate()
        {
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
            else
            {
                return EBehaviourResult.Running;
            }
        }
    }
}