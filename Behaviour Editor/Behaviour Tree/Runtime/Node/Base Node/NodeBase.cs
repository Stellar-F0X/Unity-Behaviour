using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("BehaviourSystemEditor-BT")]

namespace BehaviourSystem.BT
{
    [Serializable]
    public abstract class NodeBase : ScriptableObject, IEquatable<NodeBase>
    {
#region Node Enums

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

#endregion


        public event Action<NodeBase> onNodeEnter;

        public event Action<NodeBase> onNodeExit;


        [HideInInspector]
        public string guid;

        public string tag;

#if UNITY_EDITOR
        [HideInInspector]
        public Vector2 position;
#endif

        [HideInInspector]
        public NodeBase parent;

        [NonSerialized]
        public BehaviourTreeRunner treeRunner;

        public int callStackID;
        
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
                this.onNodeEnter?.Invoke(this);
            }

            if (this._callState == ENodeCallState.Updating)
            {
                this.behaviourResult = this.OnUpdate();

                if (this.behaviourResult != EBehaviourResult.Running)
                {
                    if (this.treeRunner.GetCurrentNode(callStackID) != this)
                    {
                        this.treeRunner.AbortSubtreeFrom(callStackID, this);
                    }

                    this._callState = ENodeCallState.BeforeExit;
                }
            }

            if (this._callState == ENodeCallState.BeforeExit)
            {
                this.onNodeExit?.Invoke(this);
                this.ExitNode();
            }

            return this.behaviourResult;
        }


        public void EnterNode()
        {
            this.treeRunner.PushInCallStack(callStackID, this);
            this.OnEnter();
            this._callState = ENodeCallState.Updating;
        }


        public void ExitNode()
        {
            this.OnExit();
            this.treeRunner.PopInCallStack(callStackID);
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



        /// 현재 노드가 생성 직후 호출되는 함수.
        public virtual void OnInitialize() { }
        
        
        /// 모든 노드가 생성된뒤 호출되는 함수.
        public virtual void OnPostInitialize() { }
        

        public virtual void FixedUpdateNode() { }

        
        public virtual void GizmosUpdateNode() { }


        protected virtual void OnEnter() { }

        
        protected virtual void OnExit() { }


        protected abstract EBehaviourResult OnUpdate();

    }
}