using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BehaviourSystem
{
    public class GroupViewDataCollection : ScriptableObject
    {
        [SerializeField]
        private List<GroupViewData> _groupViewDataList = new List<GroupViewData>();

        
        public IReadOnlyList<GroupViewData> dataList
        {
            get { return _groupViewDataList; }
        }
        

#if UNITY_EDITOR

        public void AddGroup(GroupViewData newData)
        {
            if (_groupViewDataList.Contains(newData))
            {
                return;
            }

            Undo.RecordObject(this, "Behaviour Tree (AddGroup)");
            
            _groupViewDataList.Add(newData);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }


        public void RemoveGroup(GroupViewData data)
        {
            if (_groupViewDataList.Contains(data))
            {
                Undo.RecordObject(this, "Behaviour Tree (RemoveGroup)");
                
                _groupViewDataList.Remove(data);
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }
        }
#endif
    }
}