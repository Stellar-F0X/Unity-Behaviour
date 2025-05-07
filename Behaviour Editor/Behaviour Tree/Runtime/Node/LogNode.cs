using UnityEngine;

namespace BehaviourSystem.BT
{
    public class LogNode : ActionNode
    {
        [Space]
        public string onEnterMessage;
        public string onUpdateMessage;
        public string onExitMessage;
        
        protected override void OnEnter()
        {
            if (string.IsNullOrEmpty(onEnterMessage))
            {
                return;
            }
            
            Debug.Log(onEnterMessage);
        }

        protected override EBehaviourResult OnUpdate()
        {
            if (string.IsNullOrEmpty(onUpdateMessage) == false)
            {
                Debug.Log(onUpdateMessage);
            }
            
            return EBehaviourResult.Success;
        }

        protected override void OnExit()
        {
            if (string.IsNullOrEmpty(onExitMessage))
            {
                return;
            }
            
            Debug.Log(onExitMessage);
        }
    }
}