using System;
using UnityEngine;
using BehaviourSystem.BT;

[System.Serializable]
public class FallNode : ActionNode
{
    protected override void OnEnter() 
    {
    
    }

    protected override NodeBase.EBehaviourResult OnUpdate()
    {
        return NodeBase.EBehaviourResult.Failure;
    }

    protected override void OnExit()
    {
    
    }
}