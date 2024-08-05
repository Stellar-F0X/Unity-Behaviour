using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Object = UnityEngine.Object;

[CreateAssetMenu()]
public class BehaviourTree : ScriptableObject
{
    [HideInInspector]
    public StateNode rootNode;

    [HideInInspector]
    public StateNode.eState treeState = StateNode.eState.Running;

    [HideInInspector]
    public List<StateNode> nodeList = new List<StateNode>();


    public StateNode.eState Update()
    {
        if (rootNode.state == StateNode.eState.Running)
        {
            treeState = rootNode.Update();
        }

        return treeState;
    }


#if UNITY_EDITOR
    public StateNode CreateNode(Type nodeType)
    {
        StateNode node = ScriptableObject.CreateInstance(nodeType) as StateNode;
        node.name = nodeType.Name.Replace("Node", string.Empty);
        node.guid = GUID.Generate().ToString();
        nodeList.Add(node);

        if (!Application.isPlaying)
        {
            Undo.RecordObject(this, "Behaviour Tree (CreateNode)");
            Undo.RegisterCreatedObjectUndo(node, "Behaviour Tree (CreateNode)");

            AssetDatabase.AddObjectToAsset(node, this);
            AssetDatabase.SaveAssets();
        }

        return node;
    }


    public void DeleteNode(StateNode node)
    {
        Undo.RecordObject(this, "Behaviour Tree (DeleteNode)");
        nodeList.Remove(node);

        //AssetDatabase.RemoveObjectFromAsset(node);
        Undo.DestroyObjectImmediate(node);
        AssetDatabase.SaveAssets();
    }


    public void AddChild(StateNode parent, StateNode child)
    {
        Undo.RecordObject(parent, "Behaviour Tree (AddChild)");

        switch (parent.nodeType)
        {
            case StateNode.eNodeType.Root: (parent as RootNode).child = child; break;
            case StateNode.eNodeType.Decorator: (parent as DecoratorNode).child = child; break;
            case StateNode.eNodeType.Composite: (parent as CompositeNode).children.Add(child); break;

            default: throw new BTException("It is a childless type of node.");
        }

        EditorUtility.SetDirty(child);
    }


    public void RemoveChild(StateNode parent, StateNode child)
    {
        Undo.RecordObject(parent, "Behaviour Tree (RemoveChild)");

        switch (parent.nodeType)
        {
            case StateNode.eNodeType.Root: (parent as RootNode).child = null; break;
            case StateNode.eNodeType.Decorator: (parent as DecoratorNode).child = null; break;
            case StateNode.eNodeType.Composite: (parent as CompositeNode).children.Remove(child); break;

            default: throw new BTException("It is a childless type of node");
        }

        EditorUtility.SetDirty(child);
    }


    public List<StateNode> GetChildren(StateNode parent)
    {
        switch (parent.nodeType)
        {
            case StateNode.eNodeType.Root: return new List<StateNode>() { (parent as RootNode).child };
            case StateNode.eNodeType.Decorator: return new List<StateNode>() { (parent as DecoratorNode).child };
            case StateNode.eNodeType.Composite: return (parent as CompositeNode).children;

            default: return new List<StateNode>(0);
        }
    }


    public void Traverse(StateNode node, Action<StateNode> visiter)
    {
        if (node == null)
        {
            return;
        }

        visiter.Invoke(node);
        var children = this.GetChildren(node);
        children.ForEach(node => this.Traverse(node, visiter));
    }


    public virtual BehaviourTree Clone()
    {
        BehaviourTree tree = Object.Instantiate(this);
        tree.rootNode = tree.rootNode.Clone();
        tree.nodeList = new List<StateNode>();

        this.Traverse(tree.rootNode, n => tree.nodeList.Add(n));
        return tree;
    }
#endif
}
