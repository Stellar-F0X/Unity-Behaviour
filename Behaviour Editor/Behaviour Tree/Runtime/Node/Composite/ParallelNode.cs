using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace BehaviourSystem.BT
{
    [Serializable]
    public class ParallelNode : CompositeNode
    {
        public enum EParallelPolicy
        {
            RequireAllSuccess,
            RequireOneSuccess,
            RequireAllFailure,
            RequireOneFailure
        };

        [Space]
        public EParallelPolicy parallelPolicy;

        private int _successfulChildCount = 0;
        private int _failedChildCount = 0;

        private List<bool> _isChildStopped;

        
        public override void PostTreeCreation()
        {
            _isChildStopped = new List<bool>(children.Count); // childCount -> children.Count

            for (int i = 0; i < children.Count; ++i)
            {
                _isChildStopped.Add(false);
            }
        }

        
        public void Stop()
        {
            for (int i = 0; i < children.Count; ++i)
            {
                _isChildStopped[i] = false;
            }
        }

        
        protected override void OnEnter()
        {
            if (children is null || children.Count == 0)
            {
                return;
            }

            _failedChildCount = 0;
            _successfulChildCount = 0;
            
            for (int i = 0; i < children.Count; ++i)
            {
                _isChildStopped[i] = false;
            }
        }

        
        protected override EBehaviourResult OnUpdate()
        {
            for (int i = 0; i < children.Count; ++i)
            {
                if (_isChildStopped[i] == false)
                {
                    switch (children[i].UpdateNode())
                    {
                        case EBehaviourResult.Success:
                        {
                            _successfulChildCount++;
                            _isChildStopped[i] = true;
                            break;
                        }

                        case EBehaviourResult.Failure:
                        {
                            _failedChildCount++;
                            _isChildStopped[i] = true;
                            break;
                        }
                    }
                }
            }

            return this.EvaluatePolicy();
        }

        
        private EBehaviourResult EvaluatePolicy()
        {
            switch (parallelPolicy)
            {
                case EParallelPolicy.RequireAllSuccess:
                {
                    if (_successfulChildCount == children.Count)
                    {
                        return EBehaviourResult.Success;
                    }

                    if (_failedChildCount > 0)
                    {
                        return EBehaviourResult.Failure;
                    }

                    return EBehaviourResult.Running;
                }

                case EParallelPolicy.RequireAllFailure:
                {
                    if (_failedChildCount == children.Count)
                    {
                        return EBehaviourResult.Success;
                    }

                    if (_successfulChildCount > 0)
                    {
                        return EBehaviourResult.Failure;
                    }

                    return EBehaviourResult.Running;
                }

                case EParallelPolicy.RequireOneSuccess:
                {
                    if (_successfulChildCount > 0)
                    {
                        return EBehaviourResult.Success;
                    }

                    if (_successfulChildCount + _failedChildCount == children.Count)
                    {
                        return EBehaviourResult.Failure;
                    }

                    return EBehaviourResult.Running;
                }

                case EParallelPolicy.RequireOneFailure:
                {
                    if (_failedChildCount > 0)
                    {
                        return EBehaviourResult.Success;
                    }

                    if (_successfulChildCount + _failedChildCount == children.Count)
                    {
                        return EBehaviourResult.Failure;
                    }

                    return EBehaviourResult.Running;
                }
            }

            return EBehaviourResult.Running;
        }

        
        protected override void OnExit()
        {
            for (int i = 0; i < children.Count; ++i)
            {
                if (_isChildStopped[i] == false)
                {
                    int stackID = children[i].callStackID;
                    
                    treeRunner.handler.AbortSubtree(stackID);
                    _isChildStopped[i] = true;
                }
            }
        }
    }
}