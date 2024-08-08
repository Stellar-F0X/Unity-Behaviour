using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[Serializable]
public abstract class ActionNode : Node
{
    public override eNodeType baseType
    {
        get { return eNodeType.Action; }
    }
}
