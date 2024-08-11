using System;
using UnityEngine.Events;
using Object = UnityEngine.Object;


[Serializable]
public class BehaviourTreeEvent
{
    public string key;
    public UnityEvent value;
    
    public BehaviourTreeEvent(string key, UnityEvent value)
    {
        this.value = value;
        this.key = key;
    }
    


    public void Invoke()
    {
        value?.Invoke();
    }

    public bool HasInvocation()
    {
        return value != null;
    }
}