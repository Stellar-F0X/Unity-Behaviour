using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace BehaviourTechnique
{
    [Serializable]
    public class BlackboardData
    {
        [SerializeReference]
        private List<IBlackboardProperty> _properties = new List<IBlackboardProperty>();
        
        
        public int Count
        {
            get { return _properties.Count; }
        }


        public void Initialize()
        {
            _properties?.ForEach(p => p.InitializeBeforePlaymode());
        }
        

        public void AddProperty(IBlackboardProperty property)
        {
            _properties?.Add(property);
        }
        
        
        public void RemoveProperty(IBlackboardProperty property)
        {
            _properties?.Remove(property);
        }
        
        
        public void RemoveAt(int index)
        {
            _properties?.RemoveAt(index);
        }


        public void Remove(IBlackboardProperty property)
        {
            _properties?.Remove(property);
        }

        
        public IBlackboardProperty GetProperty(int index)
        {
            return _properties[index];
        }
    }
}