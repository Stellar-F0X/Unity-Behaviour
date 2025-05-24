using UnityEngine;

namespace BehaviourSystem.BT
{
    public class UntilForNode : DecoratorNode
    {
        public enum EUntilCondition
        {
            Failure = 1,
            Success = 2
        };
        
        public EUntilCondition targetResult = EUntilCondition.Success;


        public override string tooltip
        {
            get { return "Executes the child node repeatedly until it returns the specified result."; }
        }
        

        protected override EBehaviourResult OnUpdate()
        {
            if ((int)child.UpdateNode() == (int)targetResult)
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