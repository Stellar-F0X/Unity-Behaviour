using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;


[Serializable]
public abstract class StateNode : ScriptableObject
{
    public enum eState
    {
        Running,
        Failure,
        Success
    };

    public enum eNodeType
    {
        Root,
        Action,
        Composite,
        Decorator
    };

    [HideInInspector] 
    public eState state = eState.Running;

    [HideInInspector] 
    public bool started = false;

    [HideInInspector] 
    public Vector2 position;

    [HideInInspector] 
    public string guid;

    public abstract string desciption
    {
        get;
    }

    public abstract eNodeType nodeType 
    {
        get; 
    }

    protected abstract void OnEnter();

    protected abstract void OnExit();

    protected abstract eState OnUpdate();


    public eState Update()
    {
        if (!started)
        {
            this.OnEnter();
            started = true;
        }

        state = this.OnUpdate();

        if (state != eState.Running) 
        {
            this.OnExit();
            started = false;
        }

        return state;
    }


    public virtual StateNode Clone()
    {
        return Object.Instantiate(this);
    }
}
