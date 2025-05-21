using System;
using System.Collections.Generic;

namespace BehaviourSystem.BT
{
    public class BehaviourTracer
    {
        public BehaviourTracer(NodeBase rootNode)
        {
            _rootNode = rootNode;
            _rootNode.callStackID = 0;

            this.CreateCallStack(_rootNode);
        }

        private int _callStackID; 
        
        private readonly NodeBase _rootNode;

        private readonly List<Stack<NodeBase>> _runtimeCallStack = new List<Stack<NodeBase>>();



        private void CreateCallStack(NodeBase rootOfSubtree, int newCallStackID = 0)
        {
            _runtimeCallStack.Add(new Stack<NodeBase>());
            _runtimeCallStack[newCallStackID].Push(rootOfSubtree);
            
            Stack<NodeBase> traversalStack = new Stack<NodeBase>();
            traversalStack.Push(rootOfSubtree);

            while (traversalStack.Count > 0)
            {
                NodeBase currentNode = traversalStack.Pop();
                currentNode.callStackID = newCallStackID;

                if (currentNode is IBehaviourIterable iterable)
                {
                    if (currentNode is ParallelNode)
                    {
                        foreach (NodeBase child in iterable.GetChildren())
                        {
                            _callStackID++;
                            this.CreateCallStack(child, _callStackID);
                        }
                    }
                    else
                    {
                        foreach (NodeBase child in iterable.GetChildren())
                        {
                            traversalStack.Push(child);
                        }
                    }
                }
            }
        }
        
        
        public virtual void AbortSubtreeFrom(in int callStackID, NodeBase node)
        {
            if (_runtimeCallStack[callStackID].Count > 0)
            {
                NodeBase currentNode = _runtimeCallStack[callStackID].Peek();

                while (currentNode.Equals(node) == false && currentNode.depth > node.depth)
                {
                    if (currentNode is ParallelNode parallelNode)
                    {
                        parallelNode.Stop();
                        this.AbortSubtreeFrom(currentNode.callStackID, currentNode);
                    }

                    currentNode.ExitNode();
                    _runtimeCallStack[callStackID].Pop();

                    if (_runtimeCallStack[callStackID].Count > 0)
                    {
                        currentNode = _runtimeCallStack[callStackID].Peek();
                    }
                }
            }
        }


        public NodeBase GetCurrentNode(in int callStackID)
        {
            return _runtimeCallStack[callStackID].Peek();
        }


        public virtual NodeBase.EBehaviourResult UpdateTree()
        {
            return _rootNode.UpdateNode();
        }


        public virtual void FixedUpdateTree()
        {
            _rootNode.FixedUpdateNode();
        }


        public virtual void GizmoUpdateTree()
        {
            _rootNode.GizmosUpdateNode();
        }


        public void PushInCallStack(in int callStackID, NodeBase node)
        {
            _runtimeCallStack[callStackID].Push(node);
        }


        public void PopInCallStack(in int callStackID)
        {
            _runtimeCallStack[callStackID].Pop();
        }
    }
}