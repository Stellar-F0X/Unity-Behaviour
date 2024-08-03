using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CompositeNode : StateNode
{
    [HideInInspector]
    public List<StateNode> children = new List<StateNode>();

    public override eNodeType nodeType
    {
        get { return eNodeType.Composite; }
    }

    public override StateNode Clone()
    {
        CompositeNode node = base.Clone() as CompositeNode;
        node.children = children.ConvertAll(c => c.Clone());
        return node;
    }
}
