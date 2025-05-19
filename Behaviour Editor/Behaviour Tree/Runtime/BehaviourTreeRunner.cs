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
        private readonly Dictionary<string, IBlackboardProperty> _properties = new Dictionary<string, IBlackboardProperty>();

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

        private Stack<NodeBase> _runtimeCallStack;


        internal BehaviourTree runtimeTree
        {
            get { return _runtimeTree; }
        }

        internal NodeBase currentNode
        {
            get { return _runtimeCallStack.Peek(); }
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
            get
            {
                return this.useUpdateRate ? (int)this._updateRate : -1;
            }

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


#region Unity Event Methods

        private void Awake()
        {
            if (_runtimeTree is null)
            {
                Debug.LogError("BehaviourTree is not assigned.");
                this.enabled = false;
            }
            else
            {
                if (useUpdateRate)
                {
                    this.updateRate = (int)_updateRate;
                }
                
                this._runtimeCallStack = new Stack<NodeBase>(_runtimeTree.nodeSet.nodeList.Count);
                this._runtimeTree = BehaviourTree.MakeRuntimeTree(this, _runtimeTree);
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
                this.lastExecutingResult = _runtimeTree.nodeSet.rootNode.UpdateNode();

                if (this.lastExecutingResult != NodeBase.EBehaviourResult.Running)
                {
                    onResolved?.Invoke(this.lastExecutingResult);
                }
            }
        }


        private void FixedUpdate()
        {
            if (useFixedUpdate == false)
            {
                return;
            }

            if (_runtimeTree is null || pause)
            {
                return;
            }

            _runtimeTree.nodeSet.rootNode.FixedUpdateNode();
        }


        private void OnDrawGizmos()
        {
            if (useGizmos == false)
            {
                return;
            }

            if (_runtimeTree is null || pause || Application.isPlaying == false)
            {
                return;
            }

            _runtimeTree.nodeSet.rootNode.GizmosUpdateNode();
        }

#endregion


#region Internal Methods

        internal void PushInCallStack(NodeBase node)
        {
            _runtimeCallStack.Push(node);
        }

        internal void PopInCallStack()
        {
            if (_runtimeCallStack.Count > 0)
            {
                _runtimeCallStack.Pop();
            }
        }

        internal void AbortSubtreeFrom(NodeBase node)
        {
            if (this.currentNode is null)
            {
                return;
            }

            while (this.currentNode.Equals(node) == false && this.currentNode.depth > node.depth)
            {
                currentNode.ExitNode();
            }
        }

#endregion


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