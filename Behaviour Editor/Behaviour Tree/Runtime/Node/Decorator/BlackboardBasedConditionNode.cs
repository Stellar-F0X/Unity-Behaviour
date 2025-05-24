using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourSystem.BT
{
    [Serializable]
    public sealed class BlackboardBasedConditionNode : DecoratorNode
    {
        public List<BlackboardBasedCondition> conditions;


        public override string tooltip
        {
            get { return "Provides access to the blackboard variables and data"; }
        }


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
            int count = conditions.Count;
            
            for (int i = 0; i < count; ++i)
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