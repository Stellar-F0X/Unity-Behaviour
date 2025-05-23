using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace BehaviourSystemEditor.BT
{
    public static class EditorHelper
    {
        public static T FindAssetByName<T>(string searchFilter) where T : Object
        {
            string[] guids = AssetDatabase.FindAssets(searchFilter);

            if (guids is null || guids.Length == 0)
            {
                return null;
            }

            foreach (var guid in guids)
            {
                string parentPath = AssetDatabase.GUIDToAssetPath(guid);

                if (File.Exists(parentPath))
                {
                    return AssetDatabase.LoadAssetAtPath<T>(parentPath);
                }
            }

            throw new FileNotFoundException($"Asset not found at filter: {searchFilter}");
        }
        
        
        public static string FindAssetPath(string searchFilter)
        {
            string[] guids = AssetDatabase.FindAssets(searchFilter);
            
            if (guids != null && guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);

                if (File.Exists(path))
                {
                    return path;
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
        
        
        public static List<TOutput> ConvertAll<TInput, TOutput>(this IEnumerable<TInput> array, Func<TInput, TOutput> converter)
        {
            List<TOutput> outputList = new List<TOutput>(array.Count());

            foreach (var element in array)
            {
                outputList.Add(converter.Invoke(element));
            }
            
            return outputList;
        }
    }
}