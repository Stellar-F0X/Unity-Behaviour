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
        public GroupViewData CreateGroupData(string title, Vector2 position)
        {
            GroupViewData newGroupData = CreateInstance<GroupViewData>();
            newGroupData.hideFlags = HideFlags.HideInHierarchy;
            newGroupData.Setup(title, position);

            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.RecordObject(this, "Behaviour Tree (CreateGroup)");
            }

            _groupViewDataList.Add(newGroupData);

            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.RegisterCreatedObjectUndo(newGroupData, "Behaviour Tree (CreateGroup)");
                AssetDatabase.AddObjectToAsset(newGroupData, this);
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }

            return newGroupData;
        }



        public void DeleteGroupData(GroupViewData data)
        {
            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.RecordObject(this, "Behaviour Tree (RemoveGroup)");
            }

            _groupViewDataList.Remove(data);

            if (Application.isPlaying == false && Undo.isProcessing == false)
            {
                Undo.DestroyObjectImmediate(data);
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
            }
        }

#endif
    }
}