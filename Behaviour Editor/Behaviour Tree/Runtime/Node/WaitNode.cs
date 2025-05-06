using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BehaviourSystem.BT
{
    public class WaitNode : ActionNode
    {
        public float duration = 1f;
        private float _startTime;

        protected override void OnEnter()
        {
            _startTime = Time.time;
        }

        protected override EBehaviourResult OnUpdate()
        {
            if (Time.time > _startTime + duration)
            {
                return EBehaviourResult.Success;
            }

            return EBehaviourResult.Running;
        }
    }
}