using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WaitNode : ActionNode
{
    public float duration = 1f;
    private float _startTime;

    public override string desciption
    {
        get { return "delay Node"; }
    }

    protected override void OnEnter()
    {
        _startTime = Time.time;
    }

    protected override void OnExit()
    {
        
    }

    protected override eState OnUpdate()
    {
        if (Time.time > _startTime + duration)
        {
            return eState.Success;
        }

        return eState.Running;
    }
}
