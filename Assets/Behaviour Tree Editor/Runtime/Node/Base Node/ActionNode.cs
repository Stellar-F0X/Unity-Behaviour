using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActionNode : StateNode
{
    public override eNodeType nodeType
    {
        get { return eNodeType.Action; }
    }
}
