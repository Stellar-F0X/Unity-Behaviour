using UnityEngine;

namespace BehaviourSystem.BT.Demo
{
    public class MoveNode : ActionNode
    {
        [SerializeReference]
        public BlackboardProperty<float> _moveSpeed;


        protected override void OnEnter()
        {
            Debug.Log($"speed: {_moveSpeed.value}");
        }

        protected override EBehaviourResult OnUpdate()
        {
            return EBehaviourResult.Success;
        }
    }
}