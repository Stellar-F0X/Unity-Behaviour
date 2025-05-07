using System;
using UnityEngine;

namespace BehaviourSystem.BT
{
    public interface IBlackboardProperty
    {
        public string key
        {
            get;
            set;
        }

        public Type propertyType
        {
            get;
            set;
        }

        public EConditionType comparableConditions
        {
            get;
        }

        public static IBlackboardProperty Create(Type propertyType)
        {
            var prop = Activator.CreateInstance(propertyType) as IBlackboardProperty;
            prop.propertyType = propertyType;
            return prop;
        }

        public IBlackboardProperty Clone(Type type);
    }
}