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
            if (conditions != null && this.CheckCondition())
            {
                return child.UpdateNode();
            }
            else
            {
                return EBehaviourResult.Failure;
            }
        }


        private bool CheckCondition()
        {
            for (int i = 0; i < conditions.Count; ++i)
            {
                if (conditions[i].Execute() == false)
                {
                    return false;
                }
            }

            return true;
        }
    }
}