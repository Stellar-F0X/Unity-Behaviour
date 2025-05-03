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

        public EBlackboardElement elementType
        {
            get;
        }

        public void InitializeBeforePlaymode();
    }
}