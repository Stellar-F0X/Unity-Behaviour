using System;
using UnityEngine;
using BehaviourTechnique;
using Object = UnityEngine.Object;


[Serializable]
public abstract class Node : ScriptableObject
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
        Decorator,
        Subset
    };

    [HideInInspector]
    public eState state = eState.Running;

    [HideInInspector]
    public bool started = false;

    [HideInInspector]
    public Vector2 position; //나중에 nodeView로 옮김
    
    [ReadOnly]
    public string guid;
    
    public string tag;

    public abstract eNodeType baseType
    {
        get;
    }
    

    protected abstract void OnEnter(BehaviourActor behaviourTree, PreviusBehaviourInfo info);

    protected abstract eState OnUpdate(BehaviourActor behaviourTree, PreviusBehaviourInfo info);

    protected abstract void OnExit(BehaviourActor behaviourTree, PreviusBehaviourInfo info);
    

    public virtual eState UpdateNode(BehaviourActor behaviourTree, PreviusBehaviourInfo info)
    {
        if (!started)
        {
            this.OnEnter(behaviourTree, info);
            started = true;
        }

        state = this.OnUpdate(behaviourTree, info);

        if (state != eState.Running)
        {
            this.OnExit(behaviourTree, info);
            started = false;
        }

        return state;
    }


    public virtual Node Clone()
    {
        return Object.Instantiate(this);
    }
}
