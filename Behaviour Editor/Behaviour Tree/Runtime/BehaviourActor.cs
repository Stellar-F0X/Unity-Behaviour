using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("BehaviourSystemEditor-BT")]
namespace BehaviourSystem.BT
{
    public class BehaviourActor : MonoBehaviour
    {
        private readonly Dictionary<string, IBlackboardProperty> _properties = new Dictionary<string, IBlackboardProperty>();

        [SerializeField]
        private BehaviourTree _runtimeTree;

        private Stack<NodeBase> _runtimeCallStack;


        internal BehaviourTree runtimeTree
        {
            get { return _runtimeTree; }
        }

        public NodeBase currentNode
        {
            get { return _runtimeCallStack.Peek(); }
        }

        public NodeBase.EBehaviourResult lastExecutingResult
        {
            get;
            private set;
        }

        public bool pause
        {
            get;
            set;
        }


#region Unity Event Methods

        private void Awake()
        {
            if (_runtimeTree is null)
            {
                Debug.LogError("BehaviourTree is not assigned.");

                this.enabled = false;
            }
            else
            {
                this._runtimeCallStack = new Stack<NodeBase>(_runtimeTree.nodeSet.nodeList.Count);

                this._runtimeTree = BehaviourTree.MakeRuntimeTree(this, _runtimeTree);
            }
        }


        private void Update()
        {
            if (_runtimeTree is null || pause)
            {
                return;
            }

            lastExecutingResult = _runtimeTree.nodeSet.rootNode.UpdateNode();
        }


        private void FixedUpdate()
        {
            if (_runtimeTree is null || pause)
            {
                return;
            }

            _runtimeTree.nodeSet.rootNode.FixedUpdateNode();
        }


        private void OnDrawGizmos()
        {
            if (_runtimeTree is null || pause || Application.isPlaying == false)
            {
                return;
            }

            _runtimeTree.nodeSet.rootNode.GizmosUpdateNode();
        }

#endregion


#region Internal Methods

        internal void PushInCallStack(NodeBase node)
        {
            _runtimeCallStack.Push(node);
        }

        internal void PopInCallStack()
        {
            if (_runtimeCallStack.Count > 0)
            {
                _runtimeCallStack.Pop();
            }
        }

        internal void AbortSubtreeFrom(NodeBase node)
        {
            if (this.currentNode is null)
            {
                return;
            }

            while (this.currentNode.Equals(node) == false && this.currentNode.depth > node.depth)
            {
                currentNode.ExitNode();
            }
        }

#endregion


        public void SetProperty<TValue>(in string key, TValue property)
        {
            if (_properties.TryGetValue(key, out var existingProperty))
            {
                if (existingProperty is BlackboardProperty<TValue> prop)
                {
                    prop.value = property;
                    return;
                }
            }
            else
            {
                IBlackboardProperty newProperty = _runtimeTree.blackboard.FindProperty(key);

                if (newProperty is BlackboardProperty<TValue> prop)
                {
                    prop.value = property;
                    _properties.Add(key, prop);
                    return;
                }
            }

            throw new Exception($"Blackboard property with key '{key}' was not found.");
        }


        public TValue GetProperty<TValue>(in string key)
        {
            if (_properties.TryGetValue(key, out var existingProperty))
            {
                if (existingProperty is BlackboardProperty<TValue> castedProperty)
                {
                    return castedProperty.value;
                }
            }
            else
            {
                IBlackboardProperty newProperty = _runtimeTree.blackboard.FindProperty(key);

                if (newProperty is BlackboardProperty<TValue> castedProperty)
                {
                    _properties.Add(key, newProperty);
                    return castedProperty.value;
                }
            }

            throw new Exception($"Blackboard property with key '{key}' was not found.");
        }
    }
}