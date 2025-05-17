using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BehaviourSystem.BT
{
    [Serializable]
    public class GroupData : ScriptableObject, ISerializationCallbackReceiver
    {
        public string title;
        public Vector2 position;

        [SerializeField]
        private List<string> _nodeGuidList = new List<string>();
        private HashSet<string> _nodeGuidSet = new HashSet<string>(StringComparer.Ordinal);


        public int containedNodeCount
        {
            get { return _nodeGuidList.Count; }
        }


        public void OnBeforeSerialize()
        {
            if (_nodeGuidSet is not null)
            {
                _nodeGuidList.Clear();
                _nodeGuidList.AddRange(_nodeGuidSet);
            }
        }


        public void OnAfterDeserialize()
        {
            if (_nodeGuidSet is null)
            {
                _nodeGuidSet = new HashSet<string>(_nodeGuidList, StringComparer.Ordinal);
            }
            else
            {
                _nodeGuidSet.Clear();
                _nodeGuidList.ForEach(e => _nodeGuidSet.Add(e));
            }
        }


        public bool Contains(string nodeGuid)
        {
            return _nodeGuidSet.Contains(nodeGuid);
        }

        
        public void AddNodeGuid(List<NodeBase> nodeGuid)
        {
#if UNITY_EDITOR
            Undo.RecordObject(this, "Behaviour Tree (AddNodeGuidToGroup)");
            
            nodeGuid.ForEach(node => _nodeGuidSet.Add(node.guid));
            
            EditorUtility.SetDirty(this);
#endif
        }


        public void RemoveNodeGuid(List<NodeBase> nodeGuid)
        {
#if UNITY_EDITOR
            Undo.RecordObject(this, "Behaviour Tree (RemoveNodeGuidToGroup)");
            
            nodeGuid.ForEach(node => _nodeGuidSet.Remove(node.guid));
            
            EditorUtility.SetDirty(this);
#endif
        }
    }
}