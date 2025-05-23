using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Serialization;

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
        
        public event Action onNodeEnter;

        public event Action onNodeExit;


        [HideInInspector]
        public string guid;

        public string tag;
        
        [NonSerialized]
        public int depth;
        
        [NonSerialized]
        public ulong callCount;
        
        [NonSerialized]
        public EBehaviourResult behaviourResult;
        
        [Multiline(3)]
        public string description;
        
        [NonSerialized]
        internal int callStackID;

        [HideInInspector]
        public NodeBase parent;

        [NonSerialized]
        public BehaviourTreeRunner treeRunner;
        
#if UNITY_EDITOR
        [HideInInspector]
        public Vector2 position;
#endif

        public ENodeCallState callState
        {
            get;
            private set;
        }

        public abstract ENodeType nodeType
        {
            get;
        }

        public virtual string tooltip
        {
            get;
        }


        public EBehaviourResult UpdateNode()
        {
            this.callCount++;

            if (callState == ENodeCallState.BeforeEnter)
            {
                this.EnterNode();
                this.onNodeEnter?.Invoke();
            }

            if (this.callState == ENodeCallState.Updating)
            {
                this.behaviourResult = this.OnUpdate();

                if (this.behaviourResult != EBehaviourResult.Running)
                {
                    if (this.treeRunner.handler.GetCurrentNode(callStackID) != this)
                    {
                        this.treeRunner.handler.AbortSubtreeFrom(callStackID, this);
                    }

                    this.callState = ENodeCallState.BeforeExit;
                }
            }

            if (this.callState == ENodeCallState.BeforeExit)
            {
                this.onNodeExit?.Invoke();
                this.ExitNode();
            }

            return this.behaviourResult;
        }


        public void EnterNode()
        {
            this.treeRunner.handler.PushInCallStack(callStackID, this);
            this.OnEnter();
            this.callState = ENodeCallState.Updating;
        }


        public void ExitNode()
        {
            this.treeRunner.handler.PopInCallStack(callStackID);
            this.OnExit();
            this.callState = ENodeCallState.BeforeEnter;

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
        
        /// 모든 노드가 생성된뒤 호출되는 함수.
        public virtual void PostTreeCreation() { }
        

        public virtual void FixedUpdateNode() { }

        
        public virtual void GizmosUpdateNode() { }


        protected virtual void OnEnter() { }

        
        protected virtual void OnExit() { }


        protected abstract EBehaviourResult OnUpdate();
    }
}