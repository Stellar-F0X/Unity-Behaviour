using System.Collections.Generic;

namespace BehaviourSystem.BT
{
    public partial class BehaviourNodeSet
    {
        public struct TraversalInfo
        {
            public TraversalInfo(NodeBase clone, NodeBase origin, int depth)
            {
                this.clone = clone;
                this.origin = origin;
                this.depth = depth;
            }

            public NodeBase clone;
            public NodeBase origin;
            public int depth;
        }


        public BehaviourNodeSet Clone(BehaviourActor actor)
        {
            Stack<TraversalInfo> recursionStack = new Stack<TraversalInfo>();
            BehaviourNodeSet clone = CreateInstance<BehaviourNodeSet>();

            clone.rootNode = Instantiate(this.rootNode) as RootNode;
            recursionStack.Push(new TraversalInfo(clone.rootNode, this.rootNode, 0));

            while (recursionStack.Count > 0)
            {
                TraversalInfo traversal = recursionStack.Pop();

                traversal.clone.name = traversal.clone.name.Remove(traversal.origin.name.Length, 7);
                traversal.clone.depth = traversal.depth;
                traversal.clone.actor = actor;

                clone.nodeList.Add(traversal.clone);

                switch (traversal.origin.nodeType)
                {
                    case NodeBase.ENodeType.Root:
                    {
                        RootNode instance = (RootNode)traversal.clone;
                        RootNode origin = (RootNode)traversal.origin;

                        if (origin.child != null)
                        {
                            NodeBase childInstance = Instantiate(origin.child);
                            childInstance.parent = instance;
                            instance.child = childInstance;
                            recursionStack.Push(new TraversalInfo(childInstance, origin.child, traversal.depth + 1));
                        }

                        break;
                    }

                    case NodeBase.ENodeType.Decorator:
                    {
                        DecoratorNode instance = (DecoratorNode)traversal.clone;
                        DecoratorNode origin = (DecoratorNode)traversal.origin;

                        if (origin.child != null)
                        {
                            NodeBase childInstance = Instantiate(origin.child);
                            childInstance.parent = instance;
                            instance.child = childInstance;
                            recursionStack.Push(new TraversalInfo(childInstance, origin.child, traversal.depth + 1));
                        }

                        break;
                    }

                    case NodeBase.ENodeType.Composite:
                    {
                        CompositeNode instance = (CompositeNode)traversal.clone;
                        CompositeNode origin = (CompositeNode)traversal.origin;

                        if (origin.children != null && origin.children.Count > 0)
                        {
                            for (int i = 0; i < origin.children.Count; ++i)
                            {
                                NodeBase childInstance = Instantiate(origin.children[i]);
                                childInstance.parent = instance;
                                instance.children[i] = childInstance;
                                recursionStack.Push(new TraversalInfo(childInstance, origin.children[i], traversal.depth + 1));
                            }
                        }

                        break;
                    }
                }

                traversal.clone.OnInitialize();
            }

            return clone;
        }
    }
}