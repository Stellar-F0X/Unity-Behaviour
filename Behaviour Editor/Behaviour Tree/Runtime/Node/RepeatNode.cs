namespace BehaviourSystem.BT
{
    public class RepeatNode : DecoratorNode
    {
        public int maxRepeatCount;

        protected override EBehaviourResult OnUpdate()
        {
            for (int i = 0; i < maxRepeatCount; i++)
            {
                EBehaviourResult result = child.UpdateNode();

                if (result != EBehaviourResult.Running)
                {
                    return result;
                }
            }

            return EBehaviourResult.Running;
        }
    }
}