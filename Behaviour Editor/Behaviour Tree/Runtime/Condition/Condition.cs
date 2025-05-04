using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace BehaviourTechnique
{
    [Serializable]
    public sealed class Condition
    {
        [SerializeReference]
        public IBlackboardProperty property;
        
        [SerializeReference]
        public IBlackboardProperty comparableValue;
        public EConditionType conditionType;
    }
}