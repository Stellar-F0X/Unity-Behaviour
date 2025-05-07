using System;
using UnityEngine;

namespace BehaviourSystem.BT
{
    [Serializable]
    public class IntProperty : BlackboardProperty<int>
    {
        public override EConditionType comparableConditions
        {
            get { return EConditionType.All; }
        }

        public override int CompareTo(IBlackboardProperty other)
        {
            return this.value.CompareTo(other);
        }
    }
    
    [Serializable]
    public class FloatProperty : BlackboardProperty<float>
    {
        public override EConditionType comparableConditions
        {
            get { return EConditionType.All; }
        }

        public override int CompareTo(IBlackboardProperty other)
        {
            var property = other as BlackboardProperty<float>;

            if (Mathf.Approximately(this.value, property.value))
            {
                return 0;
            }

            return this.value > property.value ? 1 : -1;
        }
    }

    [Serializable]
    public class BoolProperty : BlackboardProperty<bool>
    {
        public override EConditionType comparableConditions
        {
            get { return EConditionType.Equal | EConditionType.NotEqual; }
        }

        public override int CompareTo(IBlackboardProperty other)
        {
            return this.value == (other as BlackboardProperty<bool>).value ? 0 : -1;
        }
    }
    
    [Serializable]
    public class Vector3Property : BlackboardProperty<Vector3>
    {
        public override EConditionType comparableConditions
        {
            get { return EConditionType.All; }
        }

        public override int CompareTo(IBlackboardProperty other)
        {
            BlackboardProperty<Vector3> otherValue = other as BlackboardProperty<Vector3>;
            
            if (Mathf.Approximately(this.value.sqrMagnitude, otherValue.value.sqrMagnitude))
            {
                return 0;
            }

            return this.value.sqrMagnitude > otherValue.value.sqrMagnitude ? 1 : -1;
        }
    }
}