using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("BehaviourSystemEditor-BT")]
namespace BehaviourSystem.BT
{
    [Serializable]
    public abstract class NodeBase : ScriptableObject, IEquatable<NodeBase>
    {
        public enum ENodeCallState
        {
            BeforeEnter,
            Updating,
            BeforeExit,
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

        [Tooltip("If debug mode is on, it will log a message.")]
        public bool debugMode = true;

        [ReadOnly]
        public string guid;

        public string tag;

        [HideInInspector]
        public NodeBase parent;

        [NonSerialized]
        public BehaviourActor actor;

        protected ENodeCallState _callState;
        
        public int depth
        {
            get;
            internal set;
        }

        public ulong callCount
        {
            get;
            private set;
        }

        public EBehaviourResult behaviourResult
        {
            get;
            private set;
        }

        public abstract ENodeType nodeType
        {
            get;
        }


        public EBehaviourResult UpdateNode()
        {
            this.callCount++;
            
            if (_callState == ENodeCallState.BeforeEnter)
            {
                this.EnterNode();
            }

            if (this._callState == ENodeCallState.Updating)
            {
                this.behaviourResult = this.OnUpdate();

                if (this.behaviourResult != EBehaviourResult.Running)
                {
                    if (this.actor.currentNode != this)
                    {
                        this.actor.AbortSubtreeFrom(this);
                    }
                    
                    this._callState = ENodeCallState.BeforeExit;
                }
            }

            if (this._callState == ENodeCallState.BeforeExit)
            {
                this.ExitNode();
            }

            return this.behaviourResult;
        }


        public void EnterNode()
        {
            this.actor.PushInCallStack(this);
            this.OnEnter();
            this._callState = ENodeCallState.Updating;
        }
        
        
        public void ExitNode()
        {
            this.OnExit();
            this.actor.PopInCallStack();
            this._callState = ENodeCallState.BeforeEnter; 
            
            // If a parent node fails during execution, this node's result is set to Failure.
            if (this.behaviourResult == EBehaviourResult.Running)
            {
                this.behaviourResult = EBehaviourResult.Failure;
            }
        }
        
        
        public bool Equals(NodeBase other)
        {
            if (other is null)
            {
                return false;
            }
            
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            
            return string.CompareOrdinal(this.guid, other.guid) == 0;
        }

        

        public virtual void OnInitialize() { }

        public virtual void FixedUpdateNode() { }

        public virtual void GizmosUpdateNode() { }
        

        protected virtual void OnEnter() { }

        protected virtual void OnExit() { }
        

        protected abstract EBehaviourResult OnUpdate();
    }
}