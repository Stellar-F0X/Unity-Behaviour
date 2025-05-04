using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourSystem.BT
{
    namespace BehaviourTechnique
    {
        [Serializable]
        public class ConditionNode : DecoratorNode
        {
            [Space(10)]
            public List<Condition> condition;

            protected override void OnEnter(BehaviourActor behaviourTree, PreviusBehaviourInfo info) { }

            protected override EState OnUpdate(BehaviourActor behaviourTree, PreviusBehaviourInfo info)
            {
                Debug.Log("구현 해야 됨.");
                return EState.Success;
            }

            protected override void OnExit(BehaviourActor behaviourTree, PreviusBehaviourInfo info) { }
        }
    }
}