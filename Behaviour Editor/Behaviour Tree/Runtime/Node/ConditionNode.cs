using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace BehaviourSystem.BT
{
    namespace BehaviourTechnique
    {
        [Serializable]
        public class ConditionNode : DecoratorNode
        {
            [Space(10)]
            public List<Condition> conditions;
            

            protected override EState OnUpdate(BehaviourActor behaviourTree, PreviusBehaviourInfo info)
            {
                for (int i = 0; i < conditions.Count; ++i)
                {
                    if (conditions[i].Execute() == false)
                    {
                        return EState.Failure;
                    }
                }
                
                return child.UpdateNode(behaviourTree, info);
            }
        }
    }
}