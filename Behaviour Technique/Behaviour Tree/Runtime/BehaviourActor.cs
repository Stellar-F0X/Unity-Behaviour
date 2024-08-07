using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
using FixedUpdate = UnityEngine.PlayerLoop.FixedUpdate;

public class BehaviourActor : MonoBehaviour
{
    public enum eUpdateMode
    {
        Update,
        FixedUpdate,
        LateUpdate
    };

    public enum eStartMode
    {
        Awake,
        Enable,
        Start
    }
    
    public BehaviourTree runtimeTree;
    public eUpdateMode updateMode;
    public eStartMode startMode;

    public BehaviourTreeEvent events;
    
    private PlayerLoopSystem _playerLoop;
    private PlayerLoopSystem.UpdateFunction _behaviourTreeUpdate;


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

        startMode = eStartMode.Enable;
    }

    private void OnEnable()
    {
        if (startMode == eStartMode.Enable)
        {
            RegisterUpdateCallback(eStartMode.Enable);
        }
    }

    private void OnDisable()
    {
        RemoveUpdateCallback();
    }



    private void RemoveUpdateCallback()
    {
        if (this._behaviourTreeUpdate == null)
        {
            return;
        }

        Type updateType = GetUpdateType();
        var system = _playerLoop.subSystemList.FirstOrDefault(s => s.type == updateType);
        int index = Array.IndexOf(_playerLoop.subSystemList, system);

        if (index != -1)
        {
            var newSystems = system.subSystemList.Append(new PlayerLoopSystem {
                updateDelegate = _behaviourTreeUpdate,
                type = updateType,
            });

            system.subSystemList = newSystems.ToArray();
            _playerLoop.subSystemList[index] = system;
            PlayerLoop.SetPlayerLoop(_playerLoop);
        }

        _behaviourTreeUpdate = null;
    }


    private void RegisterUpdateCallback(eStartMode mode)
    {
        this._playerLoop = PlayerLoop.GetCurrentPlayerLoop();
        this._behaviourTreeUpdate = BehaviourNodeUpdate;
        Type updateType = this.GetUpdateType();

        var system = _playerLoop.subSystemList.FirstOrDefault(s => s.type == updateType);
        int index = Array.IndexOf(_playerLoop.subSystemList, system);

        if (index != -1)
        {
            system.subSystemList = system.subSystemList
                .Where(system => system.updateDelegate != _behaviourTreeUpdate)
                .ToArray();

            _playerLoop.subSystemList[index] = system;
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

    private void BehaviourNodeUpdate()
    {
        runtimeTree.Update();
    }
}
