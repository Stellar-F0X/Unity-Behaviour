using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WaitNode : ActionNode
{
    public float duration = 1f;
    private float _startTime;
    
    protected override void OnEnter(BehaviourActor behaviourTree, PreviusBehaviourInfo info)
    {
        _startTime = Time.time;
    }

    protected override void OnExit(BehaviourActor behaviourTree, PreviusBehaviourInfo info)
    {
        
    }

    protected override eState OnUpdate(BehaviourActor behaviourTree, PreviusBehaviourInfo info)
    {
        if (Time.time > _startTime + duration)
        {
            return eState.Success;
        }

        return eState.Running;
    }
}
