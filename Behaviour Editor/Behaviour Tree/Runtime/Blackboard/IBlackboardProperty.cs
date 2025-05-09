using System;

namespace BehaviourSystem.BT
{
    public interface IBlackboardProperty
    {
        public string key
        {
            get;
            set;
        }

        public Type type
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
            if (Activator.CreateInstance(propertyType) is IBlackboardProperty property)
            {
                property.type = propertyType;
                return property;
            }

            throw new Exception($"Failed to create property of type {propertyType}");
        }

        public IBlackboardProperty Clone(IBlackboardProperty origin);
    }
}