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

        private Queue<NodeBase> _pendingAbortNodes = new Queue<NodeBase>();


        protected override EBehaviourResult OnUpdate()
        {
            if (conditions is null || conditions.Count == 0)
            {
                return EBehaviourResult.Failure;
            }

            if (_pendingAbortNodes.Count > 0)
            {
                if (_pendingAbortNodes.TryDequeue(out var node))
                {
                    node.AbortNode();
                }

                return _pendingAbortNodes.Count == 0 ? EBehaviourResult.Failure : EBehaviourResult.Running;
            }

            for (int i = 0; i < conditions.Count; ++i)
            {
                if (conditions[i].Execute() == false)
                {
                    while (callStack.Peek() != this)
                    {
                        _pendingAbortNodes.Enqueue(callStack.Pop());
                    }

                    return EBehaviourResult.Failure;
                }
            }

            return child.UpdateNode();
        }
    }
}