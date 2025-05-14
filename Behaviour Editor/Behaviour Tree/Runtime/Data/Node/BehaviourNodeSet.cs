using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace BehaviourSystem.BT
{
    public class BehaviourNodeSet : ScriptableObject
    {
        [HideInInspector]
        public NodeBase rootNode;

        [HideInInspector]
        public List<NodeBase> nodeList = new List<NodeBase>();



        public List<NodeBase> GetChildren(NodeBase parent)
        {
            switch (parent.nodeType)
            {
                case NodeBase.ENodeType.Root: return new List<NodeBase> { ((RootNode)parent).child };

                case NodeBase.ENodeType.Decorator: return new List<NodeBase> { ((DecoratorNode)parent).child };

                case NodeBase.ENodeType.Composite: return ((CompositeNode)parent).children;
            }

            return null;
        }


        public BehaviourNodeSet Clone(BehaviourActor actor, Stack<NodeBase> callStack)
        {
            Stack<(NodeBase clone, NodeBase origin)> recursionStack = new Stack<(NodeBase clone, NodeBase origin)>();
            BehaviourNodeSet clone = CreateInstance<BehaviourNodeSet>();
            
            clone.rootNode = Instantiate(this.rootNode) as RootNode;
            recursionStack.Push((clone.rootNode, this.rootNode));

            while (recursionStack.Count > 0)
            {
                var traversal = recursionStack.Pop();

                traversal.clone.name = traversal.clone.name.Remove(traversal.origin.name.Length, 7);
                traversal.clone.callStack = callStack;
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
                            recursionStack.Push((childInstance, origin.child));
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
                            recursionStack.Push((childInstance, origin.child));
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
                                recursionStack.Push((childInstance, origin.children[i]));
                            }
                        }

                        break;
                    }
                }

                traversal.clone.OnInitialize();
            }

            return clone;
        }


#if UNITY_EDITOR

        public void AddChild(NodeBase parent, NodeBase child)
        {
            Undo.RecordObject(parent, "Behaviour Tree (AddChild)");

            switch (parent.nodeType)
            {
                case NodeBase.ENodeType.Root:
                    ((RootNode)parent).child = child;
                    child.parent = parent;
                    break;

                case NodeBase.ENodeType.Decorator:
                    ((DecoratorNode)parent).child = child;
                    child.parent = parent;
                    break;

                case NodeBase.ENodeType.Composite:
                    ((CompositeNode)parent).children.Add(child);
                    child.parent = parent;
                    break;
            }

            EditorUtility.SetDirty(parent);
            EditorUtility.SetDirty(child);
        }


        public void RemoveChild(NodeBase parent, NodeBase child)
        {
            Undo.RecordObject(parent, "Behaviour Tree (RemoveChild)");

            switch (parent.nodeType)
            {
                case NodeBase.ENodeType.Root: ((RootNode)parent).child = null; break;

                case NodeBase.ENodeType.Decorator: ((DecoratorNode)parent).child = null; break;

                case NodeBase.ENodeType.Composite: ((CompositeNode)parent).children.Remove(child); break;
            }

            EditorUtility.SetDirty(child);
        }


        public NodeBase CreateNode(Type nodeType)
        {
            if (CreateInstance(nodeType) is NodeBase node)
            {
                node.name = Regex.Replace(nodeType.Name.Replace("Node", ""), "(?<!^)([A-Z])", " $1");
                node.guid = GUID.Generate().ToString();
                node.hideFlags = HideFlags.HideInHierarchy;

                if (Application.isPlaying == false && Undo.isProcessing == false)
                {
                    Undo.RecordObject(this, "Behaviour Tree (CreateNode)");
                }

                nodeList.Add(node);

                if (Application.isPlaying == false && Undo.isProcessing == false)
                {
                    Undo.RegisterCreatedObjectUndo(node, "Behaviour Tree (CreateNode)");
                    AssetDatabase.AddObjectToAsset(node, this);
                    EditorUtility.SetDirty(this);
                    AssetDatabase.SaveAssets();
                }

                return node;
            }

            throw new Exception("Node is null");
        }



        public void DeleteNode(NodeBase node)
        {
            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.RecordObject(this, "Behaviour Tree (DeleteNode)");
            }

            nodeList.Remove(node);

            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.DestroyObjectImmediate(node);
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }
        }
#endif
    }
}