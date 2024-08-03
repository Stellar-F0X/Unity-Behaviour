using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootNode : StateNode
{
    [HideInInspector]
    public StateNode child;

    public override eNodeType nodeType
    {
        get { return eNodeType.Root; }
    }

    public override string desciption
    {
        get { return "root node"; }
    }

    protected override void OnEnter()
    {
        
    }

    protected override void OnExit()
    {
        
    }

    protected override eState OnUpdate()
    {
        return child.Update();
    }

    public override StateNode Clone()
    {
        RootNode node = base.Clone() as RootNode;
        node.child = this.child.Clone();
        return node;
    }
}
