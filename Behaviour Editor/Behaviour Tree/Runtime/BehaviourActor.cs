using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using Object = UnityEngine.Object;

namespace BehaviourSystem.BT
{
    public class BehaviourActor : MonoBehaviour
    {
        public enum BehaviourUpdateMode
        {
            Update,
            FixedUpdate,
            LateUpdate
        };
        
        public event Action OnDone;

        public BehaviourUpdateMode updateMode;

        [SerializeField]
        private BehaviourTree _runtimeTree;
        private Type _updateType;

        private Dictionary<string, IBlackboardProperty> _properties = new Dictionary<string, IBlackboardProperty>();

        private PlayerLoopSystem _playerLoop;
        private PlayerLoopSystem.UpdateFunction _btUpdater;

        public BehaviourTree runtimeTree
        {
            get { return _runtimeTree; }
        }


        private void Awake()
        {
            this._runtimeTree = BehaviourTree.MakeRuntimeTree(this, _runtimeTree);
        }


        private void OnEnable()
        {
            switch (updateMode)
            {
                case BehaviourUpdateMode.Update: _updateType = typeof(Update); break;
                
                case BehaviourUpdateMode.FixedUpdate: _updateType = typeof(FixedUpdate); break;
                
                case BehaviourUpdateMode.LateUpdate: _updateType = typeof(PostLateUpdate); break;
            }
            
            this._playerLoop =  PlayerLoop.GetCurrentPlayerLoop();
            this._btUpdater  -= this.UpdateTree;
            this._btUpdater  += this.UpdateTree;

            PlayerLoopSystem loopSystem = _playerLoop.subSystemList.First(s => s.type == _updateType);
            int index = Array.IndexOf(_playerLoop.subSystemList, loopSystem);

            if (index != -1)
            {
                PlayerLoopSystem newPlayerLoop = new PlayerLoopSystem();
                newPlayerLoop.updateDelegate = _btUpdater;
                newPlayerLoop.type           = _updateType;

                var newSystems = loopSystem.subSystemList.Append(newPlayerLoop);

                loopSystem.subSystemList         = newSystems.ToArray();
                _playerLoop.subSystemList[index] = loopSystem;
                PlayerLoop.SetPlayerLoop(_playerLoop);
            }
        }


        private void OnDisable()
        {
            if (_btUpdater is not null && _btUpdater.GetInvocationList().Length > 0)
            {
                PlayerLoopSystem loopSystem = _playerLoop.subSystemList.First(s => s.type == _updateType);
                int index = Array.IndexOf(_playerLoop.subSystemList, loopSystem);

                if (index != -1)
                {
                    loopSystem.subSystemList = loopSystem.subSystemList
                                                         .Where(s => s.updateDelegate != _btUpdater)
                                                         .ToArray();

                    _playerLoop.subSystemList[index] = loopSystem;
                    PlayerLoop.SetPlayerLoop(_playerLoop);
                }

                this._btUpdater -= this.UpdateTree;
            }
        }


        public void AbortTree(bool callOnExit = true) { }



        public void SetBlackboardProperty<TValue>(in string key, TValue property)
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
                IBlackboardProperty newProperty = _runtimeTree.blackboardData.GetProperty(key);

                if (newProperty is BlackboardProperty<TValue> prop)
                {
                    prop.value = property;
                    _properties.Add(key, prop);
                    return;
                }
            }

            throw new Exception($"Blackboard property with key '{key}' was not found.");
        }


        public TProperty GetBlackboardProperty<TProperty>(in string key) where TProperty : class
        {
            if (_properties.TryGetValue(key, out var existingProperty))
            {
                if (existingProperty is BlackboardProperty<TProperty> castedProperty)
                {
                    return castedProperty.value;
                }
            }
            else
            {
                IBlackboardProperty newProperty = _runtimeTree.blackboardData.GetProperty(key);

                if (newProperty is BlackboardProperty<TProperty> castedProperty)
                {
                    _properties.Add(key, newProperty);
                    return castedProperty.value;
                }
            }

            throw new Exception($"Blackboard property with key '{key}' was not found.");
        }



        private void UpdateTree()
        {
            if (_runtimeTree.UpdateTree() == NodeBase.EBehaviourResult.Running)
            {
                OnDone?.Invoke();
            }
        }
    }
}