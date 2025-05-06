using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourSystem.BT
{
    [Serializable]
    public class BlackboardBasedConditionNode : DecoratorNode
    {
        [Space(10), SerializeField]
        protected List<BlackboardBasedCondition> _conditions;


        protected override EBehaviourResult OnUpdate()
        {
            if (_conditions is null || _conditions.Count == 0)
            {
                return EBehaviourResult.Failure;
            }

            for (int i = 0; i < _conditions.Count; ++i)
            {
                if (_conditions[i].Execute() == false)
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