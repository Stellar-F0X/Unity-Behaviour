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

        public EParallelPolicy parallelPolicy;

        private int _successfulChildCount = 0;
        private int _failedChildCount = 0;

        private List<Coroutine> _executingCoroutines = new List<Coroutine>();


        protected override void OnEnter()
        {
            if (children is null || children.Count == 0)
            {
                return;
            }

            _failedChildCount = 0;
            _successfulChildCount = 0;

            foreach (var child in children)
            {
                IEnumerator enumerator = this.Run(child);

                _executingCoroutines.Add(treeRunner.StartCoroutine(enumerator));
            }
        }


        protected override EBehaviourResult OnUpdate()
        {
            if (_successfulChildCount + _failedChildCount > 0)
            {
                switch (parallelPolicy)
                {
                    case EParallelPolicy.RequireAllSuccess:
                    {
                        if (_successfulChildCount + _failedChildCount == children.Count)
                        {
                            return _successfulChildCount == children.Count ? EBehaviourResult.Success : EBehaviourResult.Failure;
                        }

                        break;
                    }

                    case EParallelPolicy.RequireAllFailure:
                    {
                        if (_successfulChildCount + _failedChildCount == children.Count)
                        {
                            return _failedChildCount == children.Count ? EBehaviourResult.Success : EBehaviourResult.Failure;
                        }

                        break;
                    }

                    case EParallelPolicy.RequireOneSuccess:
                    {
                        this.Stop();

                        foreach (var child in children)
                        {
                            if (child.behaviourResult == EBehaviourResult.Running)
                            {
                                treeRunner.AbortSubtree(child.callStackID);
                            }
                        }

                        return _successfulChildCount > 0 ? EBehaviourResult.Success : EBehaviourResult.Failure;
                    }

                    case EParallelPolicy.RequireOneFailure:
                    {
                        this.Stop();

                        foreach (var child in children)
                        {
                            if (child.behaviourResult == EBehaviourResult.Running)
                            {
                                treeRunner.AbortSubtree(child.callStackID);
                            }
                        }

                        return _failedChildCount > 0 ? EBehaviourResult.Success : EBehaviourResult.Failure;
                    }
                }
            }

            return EBehaviourResult.Running;
        }


        protected override void OnExit()
        {
            this.Stop();
        }


        private IEnumerator Run(NodeBase child)
        {
            while (true)
            {
                switch (child.UpdateNode())
                {
                    case EBehaviourResult.Failure:
                        _failedChildCount++;
                        yield break;

                    case EBehaviourResult.Success:
                        _successfulChildCount++;
                        yield break;
                }

                yield return null;
            }
        }


        public void Stop()
        {
            if (_executingCoroutines is null || _executingCoroutines.Count == 0)
            {
                return;
            }

            for (int i = 0; i < _executingCoroutines.Count; ++i)
            {
                if (_executingCoroutines[i] is not null)
                {
                    treeRunner.StopCoroutine(_executingCoroutines[i]);
                }
            }

            _executingCoroutines.Clear();
        }
    }
}