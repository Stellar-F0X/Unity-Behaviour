using UnityEngine;
using System.IO;

namespace BehaviourSystemEditor.BT
{
    public static class AssetUtility
    {
        public static T FindAssetByName<T>(string searchFilter) where T : Object
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets(searchFilter);

            if (guids is null || guids.Length == 0)
            {
                return null;
            }

            foreach (var guid in guids)
            {
                string parentPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);

                if (File.Exists(parentPath))
                {
                    return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(parentPath);
                }
            }

            throw new FileNotFoundException($"Asset not found at filter: {searchFilter}");
        }
    }
}