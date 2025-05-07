using System;
using UnityEngine;

namespace BehaviourSystem.BT
{
    [Serializable]
    public abstract class BlackboardProperty<T> : IBlackboardProperty, ISerializationCallbackReceiver, IComparable<IBlackboardProperty>
    {
        public BlackboardProperty()
        {
            
        }
        
        [SerializeField]
        private string _key;

        [SerializeField]
        private T _value;

        [SerializeField]
        private string _propertyTypeName;
        private Type _propertyType;

        public string key
        {
            get { return _key; }
            set { _key = value; }
        }

        public T value
        {
            get { return _value; }
            set { _value = value; }
        }

        public Type propertyType
        {
            get { return _propertyType; }
            set { _propertyType = value; }
        }

        public abstract EConditionType comparableConditions
        {
            get;
        }

        public void OnBeforeSerialize()
        {
            _propertyTypeName = _propertyType.AssemblyQualifiedName;
        }

        public void OnAfterDeserialize()
        {
            _propertyType = Type.GetType(_propertyTypeName);
        }

        public IBlackboardProperty Clone(Type type)
        {
            BlackboardProperty<T> newProperty = IBlackboardProperty.Create(type) as BlackboardProperty<T>;
            newProperty._key              = string.Copy(_key);
            newProperty._value            = default;
            newProperty._propertyTypeName = type.AssemblyQualifiedName;
            newProperty._propertyType     = _propertyType;
            return newProperty;
        }
        
        public abstract int CompareTo(IBlackboardProperty other);
    }
}