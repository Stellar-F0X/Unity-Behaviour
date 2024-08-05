using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DebugNode : ActionNode
{
    public string message;

    public override string desciption
    {
        get { return "Logger Node"; }
    }

    protected override void OnEnter()
    {

    }

    protected override eState OnUpdate()
    {
        return eState.Success;
    }

    protected override void OnExit()
    {

    }
}
