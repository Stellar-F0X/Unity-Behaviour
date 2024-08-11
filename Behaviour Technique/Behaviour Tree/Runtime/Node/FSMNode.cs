public class FSMNode : SubsetNode
{
    protected override void OnEnter(BehaviourActor behaviourTree, PreviusBehaviourInfo info)
    {
        
    }

    protected override eState OnUpdate(BehaviourActor behaviourTree, PreviusBehaviourInfo info)
    {
        return eState.Running;
    }

    protected override void OnExit(BehaviourActor behaviourTree, PreviusBehaviourInfo info)
    {
        
    }
}
