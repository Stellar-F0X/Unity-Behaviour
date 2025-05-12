using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BehaviourSystem
{
    public class GroupViewDataCollection : ScriptableObject
    {
        [SerializeField]
        private List<GroupViewData> _groupViewDataList = new List<GroupViewData>();


        public int Count
        {
            get { return _groupViewDataList?.Count ?? 0; }
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
            AssetDatabase.SaveAssets();
#endif
        }


        public void RemoveGroup(GroupViewData data)
        {
            if (_groupViewDataList.Contains(data))
            {
                _groupViewDataList.Remove(data);
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
#endif
            }
        }
    }
}