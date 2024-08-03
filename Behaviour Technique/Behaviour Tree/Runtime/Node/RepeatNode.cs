using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepeatNode : DecoratorNode
{
    public override string desciption
    {
        get { return "Iterate through the nodes connected lower layer"; }
    }

    protected override void OnEnter()
    {
        
    }

    protected override void OnExit()
    {
        
    }

    protected override eState OnUpdate()
    {
        child.Update();
        return eState.Running;
    }
}
