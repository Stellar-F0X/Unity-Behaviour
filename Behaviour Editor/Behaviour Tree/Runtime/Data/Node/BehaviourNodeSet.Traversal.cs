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


        public BehaviourNodeSet Clone(BehaviourTreeRunner treeRunner)
        {
            Stack<TraversalInfo> recursionStack = new Stack<TraversalInfo>();
            Stack<NodeBase> postIninitNodeStack = new Stack<NodeBase>();
            BehaviourNodeSet clonedSet = CreateInstance<BehaviourNodeSet>();

            clonedSet.rootNode = Instantiate(this.rootNode) as RootNode;
            recursionStack.Push(new TraversalInfo(clonedSet.rootNode, this.rootNode, 0));

            while (recursionStack.Count > 0)
            {
                TraversalInfo traversal = recursionStack.Pop();
                postIninitNodeStack.Push(traversal.clone);

                traversal.clone.name = traversal.clone.name.Remove(traversal.origin.name.Length, 7);
                traversal.clone.depth = traversal.depth;
                traversal.clone.treeRunner = treeRunner;

                clonedSet.nodeList.Add(traversal.clone);

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

                foreach (var info in ReflectionHelper.GetCachedFieldInfo(traversal.clone?.GetType()))
                {
                    if (typeof(IBlackboardProperty).IsAssignableFrom(info.FieldType))
                    {
                        ReflectionHelper.FieldAccessor accessor = ReflectionHelper.GetAccessor(info);

                        if (accessor.getter(traversal.clone) is IBlackboardProperty property)
                        {
                            IBlackboardProperty foundProperty = treeRunner.runtimeTree.blackboard.FindProperty(property.key);

                            if (foundProperty != null)
                            {
                                accessor.setter(traversal.clone, foundProperty);
                            }
                        }
                    }
                }

                traversal.clone.OnInitialize();
            }

            while (postIninitNodeStack.Count > 0)
            {
                NodeBase currentNode = postIninitNodeStack.Pop();
                currentNode.OnPostInitialize();
            }

            return clonedSet;
        }
    }
}