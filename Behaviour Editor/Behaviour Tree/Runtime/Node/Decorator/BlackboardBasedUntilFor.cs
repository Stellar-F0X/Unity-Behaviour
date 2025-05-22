using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BehaviourSystem.BT
{
    public class BlackboardBasedUntilFor : DecoratorNode
    {
        [Space(10)]
        public List<BlackboardBasedCondition> conditions;


        public override string tooltip
        {
            get { return "Keeps executing the child node until all blackboard conditions are satisfied."; }
        }


        protected override EBehaviourResult OnUpdate()
        {
            if (conditions != null && conditions.All(c => c.Execute()))
            {
                return EBehaviourResult.Success;
            }
            else
            {
                child.UpdateNode();
                return EBehaviourResult.Running;
            }
        }
    }
}