using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SequencerNode : CompositeNode
{
    private int _current;

    public override string desciption
    {
        get { return "can comflex connection Node"; }
    }

    protected  override void OnEnter()
    {
        _current = 0;
    }

    protected override void OnExit()
    {
        
    }

    //한 프레임에 모든 자식을 순회하는 것이 아니라 한 프레임에 한 자식씩만 순회하는 구조. 이게 맞나?
    protected override eState OnUpdate()
    {
        StateNode child = children[_current];

        switch (child.Update())
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
