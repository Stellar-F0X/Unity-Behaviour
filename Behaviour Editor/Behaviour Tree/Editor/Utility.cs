using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using Object = UnityEngine.Object;

namespace BehaviourSystemEditor.BT
{
    public static class Utility
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


        public static void ForEach<T>(this IEnumerable<T> array, Action<T> action)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }
            
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            
            foreach (var element in array)
            {
                action.Invoke(element);
            }
        }


        public static void ForEach(this SerializedProperty property, Action<SerializedProperty> action)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }
            
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            
            for (int i = 0; i < property.arraySize; ++i)
            {
                action.Invoke(property.GetArrayElementAtIndex(i));
            }
        }
    }
}