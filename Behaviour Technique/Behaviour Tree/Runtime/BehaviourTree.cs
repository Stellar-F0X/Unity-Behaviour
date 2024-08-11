using System;
using System.Collections.Generic;
using BehaviourTechnique;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

[CreateAssetMenu]
public class BehaviourTree : ScriptableObject
{
    [HideInInspector]
    public Node rootNode;

    [HideInInspector]
    public Node.eState treeState = Node.eState.Running;

    [HideInInspector]
    public List<Node> nodeList = new List<Node>();

    
    [field: SerializeField, ReadOnly, Tooltip("개별 트리를 구분하기 위한 고유 ID")]
    public string specificGuid
    {
        get;
        private set;
    }
    
    [field: SerializeField, ReadOnly, Tooltip("복제 트리를 구분하기 위한 GUID")]
    public string cloneGroupID
    {
        get;
        private set;
    }

    

    public void OnEnable()
    {
        cloneGroupID = GUID.Generate().ToString();
        specificGuid = GUID.Generate().ToString();
    }

    
    public Node.eState UpdateTree(BehaviourActor behaviourActor)
    {
        if (rootNode.state == Node.eState.Running)
        {
            treeState = rootNode.UpdateNode(behaviourActor, new PreviusBehaviourInfo(string.Empty, null, Node.eNodeType.Root));
        }

        return treeState;
    }


#if UNITY_EDITOR
    public Node CreateNode(Type nodeType)
    {
        Node node = ScriptableObject.CreateInstance(nodeType) as Node;
        node.name = nodeType.Name.Replace("Node", string.Empty);
        node.guid = GUID.Generate().ToString();
        nodeList.Add(node);

        if (!Application.isPlaying && !Undo.isProcessing)
        {
            Undo.RecordObject(this, "Behaviour Tree (CreateNode)");
            Undo.RegisterCreatedObjectUndo(node, "Behaviour Tree (CreateNode)");

            AssetDatabase.AddObjectToAsset(node, this);
            AssetDatabase.SaveAssets();
        }

        return node;
    }


    public void DeleteNode(Node node)
    {
        Undo.RecordObject(this, "Behaviour Tree (DeleteNode)");
        nodeList.Remove(node);
        
        Undo.DestroyObjectImmediate(node);
        AssetDatabase.SaveAssets();
    }


    public void AddChild(Node parent, Node child)
    {
        Undo.RecordObject(parent, "Behaviour Tree (AddChild)");

        switch (parent.baseType)
        {
            case Node.eNodeType.Root: (parent as RootNode).child = child; break;
            case Node.eNodeType.Decorator: (parent as DecoratorNode).child = child; break;
            case Node.eNodeType.Composite: (parent as CompositeNode).children.Add(child); break;

            default: throw new BehaviourTreeException("It is a childless type of node.");
        }

        EditorUtility.SetDirty(child);
    }


    public void RemoveChild(Node parent, Node child)
    {
        Undo.RecordObject(parent, "Behaviour Tree (RemoveChild)");

        switch (parent.baseType)
        {
            case Node.eNodeType.Root: (parent as RootNode).child = null; break;
            case Node.eNodeType.Decorator: (parent as DecoratorNode).child = null; break;
            case Node.eNodeType.Composite: (parent as CompositeNode).children.Remove(child); break;

            default: throw new BehaviourTreeException("It is a childless type of node");
        }

        EditorUtility.SetDirty(child);
    }


    public List<Node> GetChildren(Node parent)
    {
        switch (parent.baseType)
        {
            case Node.eNodeType.Root: return new List<Node>() { (parent as RootNode).child };
            case Node.eNodeType.Decorator: return new List<Node>() { (parent as DecoratorNode).child };
            case Node.eNodeType.Composite: return (parent as CompositeNode).children;

            default: return new List<Node>(0);
        }
    }


    public void Traverse(Node node, Action<Node> visiter)
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
        tree.specificGuid = GUID.Generate().ToString();
        tree.cloneGroupID = this.cloneGroupID;
        
        tree.rootNode = tree.rootNode.Clone();
        tree.nodeList = new List<Node>();

        this.Traverse(tree.rootNode, n => tree.nodeList.Add(n));
        return tree;
    }
#endif
}
