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
    [SerializeReference]
    public UnityEvent onEnterEvent = new UnityEvent();
    
    [SerializeReference]
    public UnityEvent onUpdateEvent = new UnityEvent();
    
    [SerializeReference]
    public UnityEvent onExitEvent = new UnityEvent();
}