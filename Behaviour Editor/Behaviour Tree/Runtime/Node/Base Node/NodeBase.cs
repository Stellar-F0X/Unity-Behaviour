using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BehaviourSystem.BT
{
    [Serializable]
    public abstract class NodeBase : ScriptableObject
    {
        public enum ENodeCallState
        {
            Idle,
            Updating,
            Exited,
        };

        public enum EBehaviourResult
        {
            Running,
            Failure,
            Success
        };

        public enum ENodeType
        {
            Root,
            Action,
            Composite,
            Decorator,
            Subset
        };

#if UNITY_EDITOR
        [HideInInspector]
        public Vector2 position;
#endif

        [ReadOnly]
        public string guid;

        public string tag;

        [HideInInspector]
        public NodeBase parent;

        [NonSerialized]
        public BehaviourTree tree;

        [NonSerialized]
        public BehaviourActor actor;


        public Stack<NodeBase> callStack
        {
            protected get;
            set;
        }

        public EBehaviourResult behaviourResult
        {
            get;
            private set;
        }

        public ENodeCallState callState
        {
            get;
            private set;
        }

        public abstract ENodeType nodeType
        {
            get;
        }

        
        public void AbortNode()
        {
            this.callState = ENodeCallState.Exited;
            this.OnExit();
            this.callState = ENodeCallState.Idle;
        } 


        public EBehaviourResult UpdateNode()
        {
            switch (callState)
            {
                case ENodeCallState.Idle:
                {
                    callStack.Push(this);
                    this.OnEnter();
                    callState = ENodeCallState.Updating;
                    return EBehaviourResult.Running;
                }

                case ENodeCallState.Updating:
                {
                    behaviourResult = this.OnUpdate();
                    
                    if (behaviourResult != EBehaviourResult.Running)
                    {
                        callState = ENodeCallState.Exited;
                    }
                    
                    return behaviourResult;
                }

                case ENodeCallState.Exited:
                {
                    this.OnExit();
                    callStack.Pop();
                    callState = ENodeCallState.Idle;
                    return behaviourResult;
                }
                
                default: return EBehaviourResult.Failure;
            }
        }
        
        public virtual void OnInitialize() { }

        public virtual void FixedUpdateNode() { }

        public virtual void GizmosUpdateNode() { }
        
        protected virtual void OnEnter() { }

        protected virtual void OnExit() { }
        
        protected abstract EBehaviourResult OnUpdate();
    }
}