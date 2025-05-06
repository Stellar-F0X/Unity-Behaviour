
namespace BehaviourSystem.BT
{
    public class RepeatNode : DecoratorNode
    {
        protected override EBehaviourResult OnUpdate()
        {
            child.UpdateNode();
            return EBehaviourResult.Running;
        }
    }
}