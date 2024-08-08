using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SequencerNode : CompositeNode
{
    private int _current;

    protected  override void OnEnter(BehaviourActor behaviourTree, PreviusBehaviourInfo info)
    {
        _current = 0;
    }

    protected override void OnExit(BehaviourActor behaviourTree, PreviusBehaviourInfo info)
    {
        
    }
    
    
    protected override eState OnUpdate(BehaviourActor behaviourTree, PreviusBehaviourInfo info)
    {
        switch (children[_current].UpdateNode(behaviourTree, new PreviusBehaviourInfo(tag, GetType(), baseType)))
        {
            case eState.Running: return eState.Running;
            case eState.Failure: return eState.Failure;
            case eState.Success: _current++; break;
        }

        if (_current == children.Count)
        {
            return eState.Success;
        }
        else
        {
            return eState.Running;
        }
    }
}
