using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.Serialization;


[System.Serializable]
public class BehaviourTreeEvent
{
    public GameObject bindGameObject;
    public UnityEngine.Object aObject;
    public string message;
}