namespace BehaviourSystem.BT
{
    public sealed class SelectorNode : CompositeNode
    {
        private int _currentIndex;

        protected override void OnEnter()
        {
            _currentIndex = 0;
        }

        protected override EBehaviourResult OnUpdate()
        {
            if (children is not null && _currentIndex < children.Count)
            {
                switch (children[_currentIndex].UpdateNode())
                {
                    case EBehaviourResult.Success: return EBehaviourResult.Success;
                    
                    case EBehaviourResult.Running: return EBehaviourResult.Running;

                    case EBehaviourResult.Failure: _currentIndex++; break; 
                }
                
                return EBehaviourResult.Running;
            }

            return EBehaviourResult.Failure;
        }
    }
}