using System.Collections.Generic;

namespace BehaviourSystem.BT
{
    public interface IBehaviourIterable
    {
        public IEnumerable<NodeBase> GetChildren();
    }
}