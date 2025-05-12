using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BehaviourSystem
{
    public class GroupViewDataCollection : ScriptableObject
    {
        [SerializeField]
        private List<GroupViewData> _groupViewDataList = new List<GroupViewData>();

        
        public int count
        {
            get { return _groupViewDataList.Count; }
        }


        public GroupViewData ElementAt(int index)
        {
            if (index >= 0 && index < _groupViewDataList.Count)
            {
                return _groupViewDataList[index];
            }
            
            return null;
        }
        
        
        public void AddGroup(GroupViewData newData)
        {
            if (_groupViewDataList.Contains(newData))
            {
                return;
            }
            
            _groupViewDataList.Add(newData);
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }


        public void RemoveGroup(GroupViewData data)
        {
            if (_groupViewDataList.Contains(data) == false)
            {
                return;
            }

            _groupViewDataList.Remove(data);
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
    }
}