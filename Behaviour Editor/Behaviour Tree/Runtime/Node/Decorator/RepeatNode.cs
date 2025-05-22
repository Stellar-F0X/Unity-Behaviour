using UnityEngine;

namespace BehaviourSystem.BT
{
    public class RepeatNode : DecoratorNode
    {
        [Space, Tooltip("The number of times the child node is updated per frame.")]
        public uint repeatCountPerFrame;

        protected override EBehaviourResult OnUpdate()
        {
            for (int i = 0; i < repeatCountPerFrame; i++)
            {
                EBehaviourResult result = child.UpdateNode();

                if (result != EBehaviourResult.Running)
                {
                    return result;
                }
            }

            return EBehaviourResult.Failure;
        }
    }
}