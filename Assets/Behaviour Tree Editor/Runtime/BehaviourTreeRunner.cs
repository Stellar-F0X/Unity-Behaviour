using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourTreeRunner : MonoBehaviour
{
    [SerializeField]
    public BehaviourTree behaviourTree; 

    void Start()
    {
        behaviourTree = behaviourTree.Clone();
        //_behaviourTree.rootNode
    }

    // Update is called once per frame
    void Update()
    {
        behaviourTree.Update();
    }
}
