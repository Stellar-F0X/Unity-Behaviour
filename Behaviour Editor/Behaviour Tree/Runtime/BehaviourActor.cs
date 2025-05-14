using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourSystem.BT
{
    public class BehaviourActor : MonoBehaviour
    {
        private readonly Dictionary<string, IBlackboardProperty> _properties = new Dictionary<string, IBlackboardProperty>();
        
        private readonly Stack<NodeBase> _runtimeCallStack = new Stack<NodeBase>();


        [SerializeField]
        private BehaviourTree _runtimeTree;


        public BehaviourTree runtimeTree
        {
            get { return _runtimeTree; }
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

        public bool aborted
        {
            get;
            private set;
        }


        private void Awake()
        {
            this._runtimeTree = BehaviourTree.MakeRuntimeTree(this, _runtimeTree, _runtimeCallStack);
        }


        private void Update()
        {
            if (_runtimeTree is null || pause)
            {
                return;
            }

            if (aborted)
            {
                if (_runtimeCallStack.Count > 0)
                {
                    _runtimeCallStack.Pop().AbortNode();
                }
            }
            else
            {
                lastExecutingResult = _runtimeTree.nodeSet.rootNode.UpdateNode();
            }
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


        public void AbortTree()
        {
            if (_runtimeTree is null)
            {
                return;
            }

            this.aborted = true;
        }
        
        
        
        public void RestartTree()
        {
            if (_runtimeTree is null)
            {
                return;
            }
            
            this.aborted = false;
            this.pause = false;
            
            while (_runtimeCallStack.Count > 0)
            {
                _runtimeCallStack.Pop().AbortNode(false);
            }
        }



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