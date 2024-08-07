using System;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine.Events;


[Serializable]
public class BehaviourTreeEventKeyValuePair : IEqualityComparer<BehaviourTreeEventKeyValuePair>
{
    public string key;
    public UnityEvent value;

    
    public bool Equals(BehaviourTreeEventKeyValuePair x, BehaviourTreeEventKeyValuePair y)
    {
        return string.Compare(x.key, y.key) == 0 && x.value == y.value;
    }

    public int GetHashCode(BehaviourTreeEventKeyValuePair obj)
    {
        return obj.key.GetHashCode();
    }
}
