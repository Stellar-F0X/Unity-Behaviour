using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RootNode : Node
{
    [HideInInspector]
    public Node child;

    public override eNodeType baseType
    {
        get { return eNodeType.Root; }
    }

    protected override void OnEnter(BehaviourActor behaviourTree, PreviusBehaviourInfo info)
    {
        
    }

    protected override eState OnUpdate(BehaviourActor behaviourTree, PreviusBehaviourInfo info)
    {
        return child.UpdateNode(behaviourTree, new PreviusBehaviourInfo(tag, this.GetType(), baseType));
    }
    
    protected override void OnExit(BehaviourActor behaviourTree, PreviusBehaviourInfo info)
    {
        
    }

    public override Node Clone()
    {
        RootNode node = base.Clone() as RootNode;
        node.child = this.child.Clone();
        return node;
    }
}
