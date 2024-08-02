using System.Collections;
using System.Collections.Generic;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;

public class DebugNode : ActionNode
{
    public string message;

    public UnityEvent onEnterEvent;
    public UnityEvent onUpdateEvent;
    public UnityEvent onExitEvent;

    public override string desciption
    {
        get { return "Logger Node"; }
    }

    protected override void OnEnter()
    {
        onEnterEvent?.Invoke();
    }

    protected override eState OnUpdate()
    {
        onUpdateEvent?.Invoke();
        return eState.Success;
    }

    protected override void OnExit()
    {
        onExitEvent?.Invoke();
    }
}
