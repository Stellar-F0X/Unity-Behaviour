using System;
using UnityEngine;
using System.Collections.Generic;

namespace BehaviourSystem.BT
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
        

        public static BlackboardData Clone(BlackboardData origin)
        {
            BlackboardData newData = new BlackboardData();
            newData._properties = new List<IBlackboardProperty>(origin.Count);
            
            for (int i = 0; i < origin.Count; ++i)
            {
                newData._properties[i] = origin._properties[i].Clone();
            }
            
            return newData;
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

        
        public IBlackboardProperty GetProperty(string name)
        {
            for (int i = 0; i < _properties.Count; i++)
            {
                if (string.Compare(_properties[i].key, name, StringComparison.OrdinalIgnoreCase) == 0) 
                {
                    return _properties[i];
                }
            }

            return null;
        }
    }
}