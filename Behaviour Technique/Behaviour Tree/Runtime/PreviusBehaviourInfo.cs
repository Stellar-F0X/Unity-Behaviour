using UnityEngine;
using System;

[Serializable]
public struct PreviusBehaviourInfo
{
    public PreviusBehaviourInfo(string tag, Type nodeType, Node.eNodeType basedType)
    {
        this.tag = tag;
        this.nodeType = nodeType;
        this.basedType = basedType;
    }

    public string tag;
    public Type nodeType;
    public Node.eNodeType basedType;
}
