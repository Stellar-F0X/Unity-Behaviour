using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourSystem.BT
{
    [Serializable]
    public sealed class BlackboardBasedConditionNode : DecoratorNode
    {
        [Space(10)]
        public List<BlackboardBasedCondition> conditions;

        private readonly Queue<NodeBase> _pendingAbortNodes = new Queue<NodeBase>();
        

        protected override EBehaviourResult OnUpdate()
        {
            if (child is null)
            {
                if (debugMode)
                {
                    Debug.LogWarning($"[{typeof(BlackboardBasedCondition)}] {this.name} has no child node.");
                }
                
                return EBehaviourResult.Failure;
            }
            
            if (conditions == null || conditions.Count == 0)
            {
                return defaultResult;
            }

            if (this.HandlePendingAbortNodes())
            {
                return EBehaviourResult.Running;
            }

            if (this.EvaluateConditions() == false)
            {
                this.EnqueueAbortNodes();
                return EBehaviourResult.Failure;
            }

            return child.UpdateNode();
        }

        
        private bool HandlePendingAbortNodes()
        {
            if (_pendingAbortNodes.Count == 0)
            {
                return false;
            }

            if (_pendingAbortNodes.TryDequeue(out var node))
            {
                node.AbortNode();
            }

            return _pendingAbortNodes.Count > 0;
        }


        private bool EvaluateConditions()
        {
            for (int i = 0; i < conditions.Count; ++i)
            {
                if (conditions[i].Execute())
                {
                    return false;
                }
            }

            return true;
        }


        private void EnqueueAbortNodes()
        {
            while (callStack.Peek() != this)
            {
                _pendingAbortNodes.Enqueue(callStack.Pop());
            }
        }
    }
}