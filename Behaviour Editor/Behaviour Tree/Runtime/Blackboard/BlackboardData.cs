using System;
using UnityEngine;
using System.Collections.Generic;

namespace BehaviourSystem.BT
{
    [Serializable]
    public class BlackboardData : ScriptableObject
    {
        [SerializeReference]
        private List<IBlackboardProperty> _properties = new List<IBlackboardProperty>();
        
        public List<IBlackboardProperty> properties
        {
            get { return _properties; }
        }
        

        public static BlackboardData Clone(BlackboardData origin)
        {
            BlackboardData newData = CreateInstance<BlackboardData>();
            newData._properties = new List<IBlackboardProperty>(origin.properties.Count);
            
            for (int i = 0; i < origin.properties.Count; ++i)
            {
                IBlackboardProperty prop = origin._properties[i];
                newData._properties.Add(prop.Clone(prop));
            }
            
            return newData;
        }


        public IBlackboardProperty FindProperty(in string key)
        {
            foreach (IBlackboardProperty prop in properties)
            {
                if (string.CompareOrdinal(prop.key, key) == 0)
                {
                    return prop;
                }
            }

            return null;
        }
    }
}