using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace BehaviourSystem
{
    [Serializable]
    public class GroupViewData : ISerializationCallbackReceiver
    {
        public GroupViewData(string title, Vector2 position)
        {
            this.title = title;
            this.position = position;
            this._nodeGuidList = new List<string>();
            this._nodeGuidsSet = new HashSet<string>(StringComparer.Ordinal);
        }

        public string title;
        public Vector2 position;

        [SerializeField]
        private List<string> _nodeGuidList;
        private HashSet<string> _nodeGuidsSet;


        public void OnBeforeSerialize()
        {
            if (_nodeGuidsSet is not null)
            {
                _nodeGuidList.Clear();
                _nodeGuidList.AddRange(_nodeGuidsSet);
            }
        }


        public void OnAfterDeserialize()
        {
            if (_nodeGuidsSet is null)
            {
                _nodeGuidsSet = new HashSet<string>(_nodeGuidList, StringComparer.Ordinal);
            }
            else
            {
                _nodeGuidList.ForEach(e => _nodeGuidsSet.Add(e));
            }
        }


        public bool Contains(string nodeGuid)
        {
            return _nodeGuidsSet.Contains(nodeGuid);
        }


#if UNITY_EDITOR
        public void AddNodeGuid(string nodeGuid)
        {
            _nodeGuidsSet.Add(nodeGuid);
        }


        public void RemoveNodeGuid(string nodeGuid)
        {
            _nodeGuidsSet.Remove(nodeGuid);
        }
#endif
    }
}