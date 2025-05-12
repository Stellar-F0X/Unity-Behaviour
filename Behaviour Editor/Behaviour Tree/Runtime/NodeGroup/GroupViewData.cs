using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourSystem
{
    [Serializable]
    public class GroupViewData : ISerializationCallbackReceiver
    {
        public GroupViewData(string title, Vector2 position)
        {
            this.groupTitle = title;
            this.position = position;
            this.groupGuid = Guid.NewGuid().ToString();
            this._nodeGuids = new List<string>();
            this._nodeGuidsSet = new HashSet<string>(StringComparer.Ordinal);
        }

        public string groupGuid;
        public string groupTitle;
        public Vector2 position;

        [SerializeField]
        private List<string> _nodeGuids;
        private HashSet<string> _nodeGuidsSet;

        
        public void OnBeforeSerialize()
        {
            _nodeGuids.Clear();

            if (_nodeGuidsSet is not null)
            {
                _nodeGuids.AddRange(_nodeGuidsSet);
            }
        }

        public void OnAfterDeserialize()
        {
            _nodeGuidsSet = new HashSet<string>(_nodeGuids, StringComparer.Ordinal);
        }


        public bool Contains(string nodeGuid)
        {
            return _nodeGuidsSet.Contains(nodeGuid);
        }
        

        public void AddNodeGUID(string nodeGuid)
        {
            _nodeGuidsSet.Add(nodeGuid);
        }

        
        public void RemoveNodeGUID(string nodeGuid)
        {
            _nodeGuidsSet.Remove(nodeGuid);
        }
    }
}