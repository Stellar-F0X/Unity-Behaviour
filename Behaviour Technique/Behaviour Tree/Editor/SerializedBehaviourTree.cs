using System;
using UnityEditor;

namespace StateMachine.BT
{
    [Serializable]
    public class SerializedBehaviourTree
    {
        public SerializedBehaviourTree(BehaviourTree tree) 
        {
            this._serializedBehaviourTree = new SerializedObject(tree);
            this._behaviourTree = tree;
        }
        
        private BehaviourTree _behaviourTree;
        private SerializedObject _serializedBehaviourTree;
    }
}
