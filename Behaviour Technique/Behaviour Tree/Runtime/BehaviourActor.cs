using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using FixedUpdate = UnityEngine.PlayerLoop.FixedUpdate;

public class BehaviourActor : MonoBehaviour
{
    public enum eUpdateMode
    {
        None,
        Update,
        FixedUpdate,
        LateUpdate
    };

    public enum eStartMode
    {
        None,
        Awake,
        Enable,
        Start
    }

    public BehaviourTree behaviourTree;
    public eUpdateMode updateMode;
    public eStartMode startMode;

    private PlayerLoopSystem _playerLoop;
    private PlayerLoopSystem.UpdateFunction _behaviourTreeUpdate;


    private void OnEnable() => RegistryCallback(eStartMode.Enable);

    private void Start() => RegistryCallback(eStartMode.Start);

    private void Awake()
    {
        this.behaviourTree = behaviourTree.Clone();
        RegistryCallback(eStartMode.Awake);
    }


    private void OnDisable()
    {
        if (this._behaviourTreeUpdate == null)
        {
            return;
        }

        switch (updateMode)
        {
            case eUpdateMode.Update:
                RemoveUpdateCallback(typeof(Update));
                break;

            case eUpdateMode.FixedUpdate:
                RemoveUpdateCallback(typeof(FixedUpdate));
                break;

            case eUpdateMode.LateUpdate:
                RemoveUpdateCallback(typeof(PostLateUpdate));
                break;
        }

        _behaviourTreeUpdate = null;
    }


    private void RegistryCallback(eStartMode mode)
    {
        if (mode != startMode)
        {
            return;
        }

        this._playerLoop = PlayerLoop.GetCurrentPlayerLoop();
        this._behaviourTreeUpdate = BehaviourUpdate;

        switch (updateMode)
        {
            case eUpdateMode.Update:
                RegistryUpdateCallback(typeof(Update));
                break;
            
            case eUpdateMode.FixedUpdate:
                RegistryUpdateCallback(typeof(FixedUpdate));
                break;
            
            case eUpdateMode.LateUpdate:
                RegistryUpdateCallback(typeof(PostLateUpdate));
                break;
        }
    }


    private void RegistryUpdateCallback(Type updateType)
    {
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
    }


    private void RemoveUpdateCallback(Type updateType)
    {
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


    private void BehaviourUpdate()
    {
        behaviourTree.Update();
    }
}
