using System;
using UnityEngine;

namespace BehaviourTechnique
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

        public void InitializeBeforePlaymode();
    }
}