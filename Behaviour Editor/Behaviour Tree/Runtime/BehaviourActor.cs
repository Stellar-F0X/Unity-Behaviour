using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.LowLevel;

namespace BehaviourSystem.BT
{
    public class BehaviourActor : MonoBehaviour
    {
        public event Action OnDone;

        [SerializeField]
        private BehaviourTree _runtimeTree;

        private Type _updateType;

        private readonly Dictionary<string, IBlackboardProperty> _properties = new Dictionary<string, IBlackboardProperty>();

        private PlayerLoopSystem _playerLoop;
        private PlayerLoopSystem.UpdateFunction _btUpdater;
        private NodeBase.EBehaviourResult _lastExecutingResult;

        public BehaviourTree runtimeTree
        {
            get { return _runtimeTree; }
        }

        public NodeBase.EBehaviourResult lastExecutingResult
        {
            get { return _lastExecutingResult; }
        }


        private void Awake()
        {
            this._runtimeTree = BehaviourTree.MakeRuntimeTree(this, _runtimeTree);
        }


        private void Update()
        {
            if (_runtimeTree is null)
            {
                Debug.LogError("Behaviour Tree is null");
                return;
            }

            _lastExecutingResult = _runtimeTree.UpdateTree();

            if (_lastExecutingResult != NodeBase.EBehaviourResult.Running)
            {
                OnDone?.Invoke();
            }
        }


        private void FixedUpdate()
        {
            if (_runtimeTree is null)
            {
                Debug.LogError("Behaviour Tree is null");
                return;
            }

            _runtimeTree.FixedUpdateTree();
        }


        private void OnDrawGizmos()
        {
            if (Application.isPlaying == false)
            {
                return;
            }
            
            if (_runtimeTree is null)
            {
                Debug.LogError("Behaviour Tree is null");
                return;
            }

            _runtimeTree.GizmosUpdateTree();
        }


        public void PauseTree() { }


        public void AbortTree(bool callOnExit = true) { }




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
                IBlackboardProperty newProperty = _runtimeTree.blackboardData.FindProperty(key);

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
                IBlackboardProperty newProperty = _runtimeTree.blackboardData.FindProperty(key);

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