using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventNode : ActionNode
{
    public BehaviourTreeEvent behaviourTreeEvent;

    protected override void OnEnter(BehaviourActor behaviourTree, PreviusBehaviourInfo info)
    {
        behaviourTreeEvent?.Invoke();
    }

    protected override eState OnUpdate(BehaviourActor behaviourTree, PreviusBehaviourInfo info)
    {
        behaviourTreeEvent?.Invoke();
        return eState.Success;
    }

    protected override void OnExit(BehaviourActor behaviourTree, PreviusBehaviourInfo info)
    {
        behaviourTreeEvent?.Invoke();
    }
}
