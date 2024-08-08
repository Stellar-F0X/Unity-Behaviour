using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.Serialization;


[Serializable]
public class BehaviourTreeEvent
{
    public BehaviourTreeEvent(string key, UnityEvent value)
    {
        this.value = value;
        this.key = key;
    }
    
    public string key;
    public UnityEvent value;


    public void Invoke()
    {
        value?.Invoke();
    }

    public bool HasInvocation()
    {
        return value != null;
    }
}