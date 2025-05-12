using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourSystem
{
    [Serializable]
    public class GroupViewData : ScriptableObject, ISerializationCallbackReceiver
    {
        public string title;
        public Vector2 position;

        [SerializeField]
        private List<string> _nodeGuidList = new List<string>();
        private HashSet<string> _nodeGuidSet = new HashSet<string>(StringComparer.Ordinal);
        
        
        public void Setup(string title, Vector2 position)
        {
            this.title = title;
            this.position = position;
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

        
        public void AddNodeGuid(string nodeGuid)
        {
            _nodeGuidSet.Add(nodeGuid);
        }


        public void RemoveNodeGuid(string nodeGuid)
        {
            _nodeGuidSet.Remove(nodeGuid);
        }
    }
}