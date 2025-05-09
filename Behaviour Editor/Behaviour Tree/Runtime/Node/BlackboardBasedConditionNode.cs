using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace BehaviourSystem.BT
{
    [Serializable]
    public sealed class BlackboardBasedConditionNode : DecoratorNode
    {
        [Space(10)]
        public List<BlackboardBasedCondition> conditions;


        protected override EBehaviourResult OnUpdate()
        {
            if (conditions is null || conditions.Count == 0)
            {
                return EBehaviourResult.Failure;
            }

            for (int i = 0; i < conditions.Count; ++i)
            {
                if (conditions[i].Execute() == false)
                {
                    while (callStack.Peek() != this)
                    {
                        NodeBase node = callStack.Pop();
                        node.AbortNode();
                    }
                    
                    return EBehaviourResult.Failure;
                }
            }

            return child.UpdateNode();
        }
    }
}