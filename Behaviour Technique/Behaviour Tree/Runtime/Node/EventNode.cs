using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventNode : ActionNode
{
    public override string desciption
    {
        get { return "Event Node"; }
    }

    public BehaviourTreeEvent btEvent;

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
