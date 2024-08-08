using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventNode : ActionNode
{
    public string message = "정상 작동";
    public BehaviourTreeEvent behaviourTreeEvent;

    protected override void OnEnter(BehaviourActor behaviourTree, PreviusBehaviourInfo info)
    {
        Debug.Log(message);
    }

    protected override eState OnUpdate(BehaviourActor behaviourTree, PreviusBehaviourInfo info)
    {
        Debug.Log(message);
        
        return eState.Success;
    }

    protected override void OnExit(BehaviourActor behaviourTree, PreviusBehaviourInfo info)
    {
        Debug.Log(message);
    }
}
