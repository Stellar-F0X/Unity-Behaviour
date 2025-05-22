using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BehaviourSystem.BT
{
    [Serializable]
    public sealed class BlackboardBasedConditionNode : DecoratorNode
    {
        [Space(10)]
        public List<BlackboardBasedCondition> conditions;


        protected override EBehaviourResult OnUpdate()
        {
            if (conditions != null && conditions.All(c => c.Execute()))
            {
                return child.UpdateNode();
            }
            else
            {
                return EBehaviourResult.Failure;
            }
        }
    }
}