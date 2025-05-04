
namespace BehaviourSystem.BT
{
    public class RepeatNode : DecoratorNode
    {
        protected override EState OnUpdate(BehaviourActor behaviourTree, PreviusBehaviourInfo info)
        {
            child.UpdateNode(behaviourTree, new PreviusBehaviourInfo(tag, GetType(), baseType));
            return EState.Running;
        }
    }
}