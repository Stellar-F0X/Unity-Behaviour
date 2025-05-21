using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("BehaviourSystemEditor-BT")]

namespace BehaviourSystem.BT
{
    [DefaultExecutionOrder(-1)]
    public class BehaviourTreeRunner : MonoBehaviour
    {
        private struct TraversalInfo
        {
            public TraversalInfo(NodeBase node, int stackID)
            {
                this.node = node;
                this.stackID = stackID;
            }

            public NodeBase node;
            public int stackID;
        }


        private readonly Dictionary<string, IBlackboardProperty> _properties = new Dictionary<string, IBlackboardProperty>();

        private readonly List<Stack<NodeBase>> _runtimeCallStack = new List<Stack<NodeBase>>();

        public event Action<NodeBase.EBehaviourResult> onResolved;

        public bool useUpdateRate = false;
        public bool useFixedUpdate = true;
        public bool useGizmos = true;

        [SerializeField]
        private uint _updateRate = 60;

        private float _frameInterval;
        private float _timeSinceLastUpdate;

        [SerializeField]
        private BehaviourTree _runtimeTree;

        private NodeBase _rootNode;


        internal BehaviourTree runtimeTree
        {
            get { return _runtimeTree; }
        }

        public NodeBase.EBehaviourResult lastExecutingResult
        {
            get;
            private set;
        }

        public bool pause
        {
            get;
            set;
        }

        public int updateRate
        {
            get { return this.useUpdateRate ? (int)this._updateRate : -1; }

            set
            {
                if (this.useUpdateRate)
                {
                    this._updateRate = (uint)Mathf.Max(Application.targetFrameRate, 0);
                    this._updateRate = _updateRate == 0 ? (uint)value : _updateRate;
                    this._frameInterval = 1f / _updateRate;
                }
                else
                {
                    Debug.LogWarning("Cannot set the update rate because useUpdateRate is disabled.");
                }
            }
        }


        private void Awake()
        {
            if (_runtimeTree is null)
            {
                Debug.LogError("BehaviourTree is not assigned.");
                this.enabled = false;
            }
            else
            {
                if (this.useUpdateRate)
                {
                    this.updateRate = (int)_updateRate;
                }

                this._runtimeTree = BehaviourTree.MakeRuntimeTree(this, _runtimeTree);
                this._rootNode = _runtimeTree.nodeSet.rootNode;
                this.CreateCallStack(_rootNode);
            }
        }


        private void Update()
        {
            if (_runtimeTree is null || pause)
            {
                return;
            }

            if (useUpdateRate == false || _frameInterval + _timeSinceLastUpdate < Time.time)
            {
                this._timeSinceLastUpdate = Time.time;
                this.lastExecutingResult = _rootNode.UpdateNode();

                if (this.lastExecutingResult != NodeBase.EBehaviourResult.Running)
                {
                    onResolved?.Invoke(this.lastExecutingResult);
                }
            }
        }


        private void FixedUpdate()
        {
            if (useFixedUpdate == false || _runtimeTree is null || pause)
            {
                return;
            }

            _rootNode.FixedUpdateNode();
        }


        private void OnDrawGizmos()
        {
            if (useGizmos == false || _runtimeTree is null || pause || Application.isPlaying == false)
            {
                return;
            }

            _rootNode.GizmosUpdateNode();
        }


        internal NodeBase GetCurrentNode(in int callStackID)
        {
            if (_runtimeCallStack.Count <= callStackID || _runtimeCallStack[callStackID].Count == 0)
            {
                return null;
            }

            return _runtimeCallStack[callStackID].Peek();
        }


        internal void PushInCallStack(in int callStackID, NodeBase node)
        {
            _runtimeCallStack[callStackID].Push(node);
        }


        internal void PopInCallStack(in int callStackID)
        {
            if (_runtimeCallStack.Count == 0)
            {
                Debug.LogWarning("");
                return;
            }

            _runtimeCallStack[callStackID].Pop();
        }


        internal void AbortSubtreeFrom(in int callStackID, NodeBase node)
        {
            Queue<TraversalInfo> abortQueue = new Queue<TraversalInfo>();
            abortQueue.Enqueue(new TraversalInfo(node, callStackID));

            while (abortQueue.Count > 0)
            {
                TraversalInfo current = abortQueue.Dequeue();
                int currentID = current.stackID;
                NodeBase currentNode = current.node;

                if (_runtimeCallStack.Count <= currentID || _runtimeCallStack[currentID].Count == 0)
                {
                    continue;
                }

                NodeBase stackNode = _runtimeCallStack[currentID].Peek();

                while (stackNode.Equals(currentNode) == false && stackNode.depth > currentNode.depth)
                {
                    if (stackNode is ParallelNode parallelNode)
                    {
                        parallelNode.Stop();

                        foreach (var child in parallelNode.GetChildren())
                        {
                            abortQueue.Enqueue(new TraversalInfo(stackNode, child.callStackID));
                        }
                    }

                    stackNode.ExitNode();

                    if (_runtimeCallStack[currentID].Count == 0)
                    {
                        break;
                    }

                    stackNode = _runtimeCallStack[currentID].Peek();
                }
            }
        }



        internal void AbortSubtree(in int callStackID)
        {
            Queue<int> abortQueue = new Queue<int>();
            abortQueue.Enqueue(callStackID);

            while (abortQueue.Count > 0)
            {
                int currentID = abortQueue.Dequeue();

                if (_runtimeCallStack.Count <= currentID || _runtimeCallStack[currentID].Count == 0)
                {
                    continue;
                }

                NodeBase stackNode = _runtimeCallStack[currentID].Peek();

                if (stackNode is ParallelNode parallelNode)
                {
                    parallelNode.Stop();

                    foreach (var child in parallelNode.GetChildren())
                    {
                        abortQueue.Enqueue(child.callStackID);
                    }
                }

                stackNode.ExitNode();

                if (_runtimeCallStack[currentID].Count == 0)
                {
                    break;
                }
                
                abortQueue.Enqueue(_runtimeCallStack[currentID].Peek().callStackID);
            }
        }



        public void SetProperty<TValue>(in string key, TValue property)
        {
            if (_properties.TryGetValue(key, out var existingProperty))
            {
                if (existingProperty is BlackboardProperty<TValue> prop)
                {
                    prop.value = property;
                    return;
                }
            }
            else
            {
                IBlackboardProperty newProperty = _runtimeTree.blackboard.FindProperty(key);

                if (newProperty is BlackboardProperty<TValue> prop)
                {
                    prop.value = property;
                    _properties.Add(key, prop);
                    return;
                }
            }

            Debug.LogWarning($"Blackboard property with key '{key}' was not found.");
        }


        public TValue GetProperty<TValue>(in string key)
        {
            if (_properties.TryGetValue(key, out var existingProperty))
            {
                if (existingProperty is BlackboardProperty<TValue> castedProperty)
                {
                    return castedProperty.value;
                }
            }
            else
            {
                IBlackboardProperty newProperty = _runtimeTree.blackboard.FindProperty(key);

                if (newProperty is BlackboardProperty<TValue> castedProperty)
                {
                    _properties.Add(key, newProperty);
                    return castedProperty.value;
                }
            }

            Debug.LogWarning($"Blackboard property with key '{key}' was not found.");
            return default;
        }


        public void RegisterOnNodeEnterCallback(string treePath, Action<NodeBase> callback)
        {
            if (this.TryGetNodeByPath(treePath, out NodeBase node))
            {
                node.onNodeEnter += callback;
            }
            else
            {
                Debug.LogWarning($"Node with path '{treePath}' was not found.");
            }
        }


        public void RegisterNodeExitCallback(string treePath, Action<NodeBase> callback)
        {
            if (this.TryGetNodeByPath(treePath, out NodeBase node))
            {
                node.onNodeExit += callback;
            }
            else
            {
                Debug.LogWarning($"Node with path '{treePath}' was not found.");
            }
        }


        public void UnregisterNodeEnterCallback(string treePath, Action<NodeBase> callback)
        {
            if (this.TryGetNodeByPath(treePath, out NodeBase node))
            {
                node.onNodeEnter -= callback;
            }
            else
            {
                Debug.LogWarning($"Node with path '{treePath}' was not found.");
            }
        }


        public void UnregisterNodeExitCallback(string treePath, Action<NodeBase> callback)
        {
            if (this.TryGetNodeByPath(treePath, out NodeBase node))
            {
                node.onNodeExit -= callback;
            }
            else
            {
                Debug.LogWarning($"Node with path '{treePath}' was not found.");
            }
        }


        private void CreateCallStack(NodeBase rootOfSubtree)
        {
            int callStackID = 0;
            Queue<TraversalInfo> workQueue = new Queue<TraversalInfo>();
            workQueue.Enqueue(new TraversalInfo(rootOfSubtree, 0));

            while (workQueue.Count > 0)
            {
                TraversalInfo currentTraversal = workQueue.Dequeue();
                Stack<NodeBase> traversalStack = new Stack<NodeBase>();
                currentTraversal.node.callStackID = currentTraversal.stackID;
                traversalStack.Push(currentTraversal.node);

                while (traversalStack.Count > 0)
                {
                    NodeBase currentNode = traversalStack.Pop();
                    currentNode.callStackID = currentTraversal.stackID;

                    while (_runtimeCallStack.Count <= currentNode.callStackID)
                    {
                        _runtimeCallStack.Add(new Stack<NodeBase>());
                    }

                    if (currentNode is IBehaviourIterable iterable && iterable.childCount > 0)
                    {
                        if (currentNode is ParallelNode)
                        {
                            _runtimeCallStack.Add(new Stack<NodeBase>());

                            foreach (NodeBase child in iterable.GetChildren())
                            {
                                workQueue.Enqueue(new TraversalInfo(child, ++callStackID));
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
        }


        private bool TryGetNodeByPath(string treePath, out NodeBase node)
        {
            if (string.IsNullOrEmpty(treePath) || string.IsNullOrWhiteSpace(treePath))
            {
                node = null;
                return false;
            }

            string[] paths = treePath.Split('/');

            if (paths.Length == 0 || string.CompareOrdinal(_runtimeTree.nodeSet.rootNode.name, paths[0]) != 0)
            {
                node = null;
                return false;
            }

            NodeBase nodeBase = _runtimeTree.nodeSet.rootNode;

            for (int i = 1; i < paths.Length; i++)
            {
                bool find = false;

                foreach (NodeBase child in _runtimeTree.nodeSet.GetChildren(nodeBase))
                {
                    if (string.CompareOrdinal(child.name, paths[i]) == 0)
                    {
                        nodeBase = child;
                        find = true;
                        break;
                    }
                }

                if (find == false)
                {
                    node = null;
                    return false;
                }
            }

            node = nodeBase;
            return true;
        }
    }
}