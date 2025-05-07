using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BehaviourSystem.BT
{
    [Serializable]
    public class BlackboardProperty<T> : IBlackboardProperty
    {
        public BlackboardProperty(string key, T value, EBlackboardPropertyType type)
        {
            _key          = key;
            _value        = value;
            _propertyType = type;
        }

        [SerializeField]
        private string _key;

        [SerializeField]
        private T _value;

        [SerializeField]
        private EBlackboardPropertyType _propertyType;

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

        public EBlackboardPropertyType propertyType
        {
            get { return _propertyType; }
        }

        public Type propertyType2;

        
        public IBlackboardProperty Clone()
        {
            //TODO: return Activator.CreateInstance(propertyType2) as IBlackboardProperty;
            return new BlackboardProperty<T>(string.Copy(key), default, _propertyType);
        }
    }
}