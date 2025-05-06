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

        public EBlackboardPropertyType propertyType
        {
            get;
        }

        public IBlackboardProperty Clone();
    }
}