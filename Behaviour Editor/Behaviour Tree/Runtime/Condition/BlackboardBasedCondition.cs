using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace BehaviourSystem.BT
{
    [Serializable]
    public sealed class BlackboardBasedCondition
    {
        [SerializeReference]
        public IBlackboardProperty property;

        [SerializeReference]
        public IBlackboardProperty comparableValue;

        public EConditionType conditionType;


        private const int _EQUAL = 0;

        private const int _GREATER = 1;
        
        private const int _LESS = -1;
        
        

        public bool Execute()
        {
            switch (property.propertyType)
            {
                case EBlackboardPropertyType.Int: return this.Compare((BlackboardProperty<int>)property, (BlackboardProperty<int>)comparableValue);

                case EBlackboardPropertyType.Float: return this.Compare((BlackboardProperty<float>)property, (BlackboardProperty<float>)comparableValue);

                case EBlackboardPropertyType.Bool: return this.Compare((BlackboardProperty<bool>)property, (BlackboardProperty<bool>)comparableValue);
                
                default: return false;
            }
        }
        

        private bool Compare<T>(BlackboardProperty<T> a, BlackboardProperty<T> b) where T : struct, IComparable<T>
        {
            switch (conditionType)
            {
                case EConditionType.Equal: return a.value.CompareTo(b.value) == _EQUAL;

                case EConditionType.NotEqual: return a.value.CompareTo(b.value) != _EQUAL;

                case EConditionType.GreaterThan: return a.value.CompareTo(b.value) == _GREATER;

                case EConditionType.GreaterThanOrEqual: return a.value.CompareTo(b.value) is _GREATER or _EQUAL;

                case EConditionType.LessThan: return a.value.CompareTo(b.value) == _LESS;

                case EConditionType.LessThanOrEqual: return a.value.CompareTo(b.value) is _LESS or _EQUAL;
                
                default: throw new NotImplementedException();
            }
        }
    }
}