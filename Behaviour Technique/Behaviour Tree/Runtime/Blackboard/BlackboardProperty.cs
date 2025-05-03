using System;
using UnityEngine;

namespace BehaviourTechnique
{
    [Serializable]
    public class BlackboardProperty<T> : IBlackboardProperty where T : struct
    {
        public BlackboardProperty(string key, T value, EBlackboardElement type)
        {
            _key         = key;
            _value       = value;
            _elementType = type;
        }
        
        [SerializeField]
        private string _key;
        
        [SerializeField]
        private T _value;

        [SerializeField]
        private EBlackboardElement _elementType;

        
        public string key
        {
            get { return _key; }
            set { _key = value; }
        }
        
        public T value 
        {
            get { return _value; }
        }
        
        public EBlackboardElement elementType
        {
            get { return _elementType; }
        }

        
        public void InitializeBeforePlaymode()
        {
            _value = default(T);
        }
    }
}