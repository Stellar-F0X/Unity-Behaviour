using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[Serializable]
public abstract class DecoratorNode : StateNode
{
    [HideInInspector]
    public StateNode child;

    public override eNodeType nodeType
    {
        get { return eNodeType.Decorator; }
    }

    public override StateNode Clone()
    {
        DecoratorNode node = base.Clone() as DecoratorNode;
        node.child = this.child.Clone();
        return node;
    }
}
