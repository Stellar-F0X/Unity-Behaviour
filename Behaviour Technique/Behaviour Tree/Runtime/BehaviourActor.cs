using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
using Update = UnityEngine.PlayerLoop.Update;
using FixedUpdate = UnityEngine.PlayerLoop.FixedUpdate;


public class BehaviourActor : MonoBehaviour
{
    public enum eUpdateMode
    {
        Update,
        FixedUpdate,
        LateUpdate
    }

    public enum eStartMode
    {
        Awake,
        Enable,
        Start
    }
    
    public BehaviourTree runtimeTree;
    public eUpdateMode updateMode;
    public eStartMode startMode;
    
    [SerializeField, HideInInspector]
    private List<BehaviourTreeEvent> _behaviourEvents = new List<BehaviourTreeEvent>();

    private bool _canRegisterWhenEnable;
    
    private PlayerLoopSystem _playerLoop;
    private PlayerLoopSystem.UpdateFunction _behaviourTreeUpdate;


    public void AddBehaviourEvent(BehaviourTreeEvent newEvent)
    {
        if (string.IsNullOrEmpty(newEvent.key) || _behaviourEvents.Contains(newEvent))
        {
            return;
        }
        
        _behaviourEvents.Add(newEvent);
    }


    public BehaviourTreeEvent GetLastEvent()
    {
        return _behaviourEvents?.Last();
    }
    
    
    public void RemoveBehaviourEvent(string eventID)
    {
        BehaviourTreeEvent behaviourEvent = this.GetBehaviourEvent(eventID);

        if (behaviourEvent != null)
        {
            _behaviourEvents.Remove(behaviourEvent);
        }
    }


    public BehaviourTreeEvent GetBehaviourEvent(string eventID)
    {
        return _behaviourEvents.First(e => string.Compare(e.key, eventID) == 0);
    }
    

    #region Activator Functions
    
    private void Awake()
    {
        this.runtimeTree = runtimeTree.Clone();

        if (startMode == eStartMode.Awake)
        {
            RegisterUpdateCallback(eStartMode.Awake);
        }
    }

    private void Start()
    {
        if (startMode == eStartMode.Start)
        {
            RegisterUpdateCallback(eStartMode.Start);
        }

        _canRegisterWhenEnable = true;
    }

    private void OnEnable()
    {
        if (startMode == eStartMode.Enable || _canRegisterWhenEnable)
        {
            this.RegisterUpdateCallback(eStartMode.Enable);
        }
    }

    private void OnDisable()
    {
        if (_behaviourTreeUpdate?.GetInvocationList().Length > 0)
        {
            this.RemoveUpdateCallback();
        }
    }

    #endregion


    private void RemoveUpdateCallback()
    {
        Type updateType = GetUpdateType();
        var loopSystem = _playerLoop.subSystemList.FirstOrDefault(s => s.type == updateType);
        int index = Array.IndexOf(_playerLoop.subSystemList, loopSystem);

        if (index != -1)
        {
            loopSystem.subSystemList = loopSystem.subSystemList
                .Where(system => system.updateDelegate != _behaviourTreeUpdate)
                .ToArray();

            _playerLoop.subSystemList[index] = loopSystem;
            PlayerLoop.SetPlayerLoop(_playerLoop);
        }

        this._behaviourTreeUpdate -= BehaviourTreeUpdate;
    }


    private void RegisterUpdateCallback(eStartMode mode)
    {
        this._playerLoop = PlayerLoop.GetCurrentPlayerLoop();
        this._behaviourTreeUpdate -= BehaviourTreeUpdate;
        this._behaviourTreeUpdate += BehaviourTreeUpdate;
        
        Type updateType = this.GetUpdateType();
        var loopSystem = _playerLoop.subSystemList.FirstOrDefault(s => s.type == updateType);
        int index = Array.IndexOf(_playerLoop.subSystemList, loopSystem);
        
        if (index != -1)
        {
            var newSystems = loopSystem.subSystemList.Append(new PlayerLoopSystem {
                updateDelegate = _behaviourTreeUpdate,
                type = updateType,
            });

            loopSystem.subSystemList = newSystems.ToArray();
            _playerLoop.subSystemList[index] = loopSystem;
            PlayerLoop.SetPlayerLoop(_playerLoop);
        }
    }


    private Type GetUpdateType()
    {
        switch (updateMode)
        {
            case eUpdateMode.Update: return typeof(Update);

            case eUpdateMode.FixedUpdate: return typeof(FixedUpdate);

            case eUpdateMode.LateUpdate: return typeof(PostLateUpdate);

            default: return null;
        }
    }

    
    private void BehaviourTreeUpdate()
    {
        runtimeTree.UpdateTree(this);
    }
}
