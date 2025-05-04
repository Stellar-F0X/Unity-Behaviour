namespace BehaviourSystem.BT
{
    public abstract class SubsetNode : NodeBase
    {
        public override ENodeType baseType
        {
            get { return ENodeType.Subset; }
        }
    }
}