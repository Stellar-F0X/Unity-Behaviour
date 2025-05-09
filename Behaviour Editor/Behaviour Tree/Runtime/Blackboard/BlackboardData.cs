using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace BehaviourSystem.BT
{
    [Serializable]
    public class BlackboardData : ScriptableObject
    {
        [SerializeReference]
        private List<IBlackboardProperty> _properties = new List<IBlackboardProperty>();
        
        
        public int Count
        {
            get { return _properties.Count; }
        }
        

        public static BlackboardData Clone(BlackboardData origin)
        {
            BlackboardData newData = CreateInstance<BlackboardData>();
            newData._properties = new List<IBlackboardProperty>(origin.Count);
            
            for (int i = 0; i < origin.Count; ++i)
            {
                IBlackboardProperty prop = origin._properties[i];
                newData._properties.Add(prop.Clone(prop));
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
        
        
        public int IndexOf(IBlackboardProperty property)
        {
            if (_properties is not null)
            {
                return _properties.IndexOf(property);
            }

            return -1;
        }


        public void RemoveAt(int index)
        {
            _properties.RemoveAt(index);
        }

        
        public IBlackboardProperty GetProperty(in int index)
        {
            return _properties[index];
        }

        
        public IBlackboardProperty GetProperty(in string name)
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