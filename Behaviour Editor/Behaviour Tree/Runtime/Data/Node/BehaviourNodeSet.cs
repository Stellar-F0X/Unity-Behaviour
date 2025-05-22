using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace BehaviourSystem.BT
{
    public partial class BehaviourNodeSet : ScriptableObject
    {
        [HideInInspector]
        public NodeBase rootNode;

        [HideInInspector]
        public List<NodeBase> nodeList = new List<NodeBase>();

        
        internal BehaviourNodeSet Clone(BehaviourTreeRunner treeRunner)
        {
            BehaviourNodeSet clonedSet = CreateInstance<BehaviourNodeSet>();
            clonedSet.rootNode = Instantiate(this.rootNode) as RootNode;
            treeRunner.handler.CloneNodeSet(this.rootNode, clonedSet.rootNode, treeRunner, this, clonedSet);
            return clonedSet;
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