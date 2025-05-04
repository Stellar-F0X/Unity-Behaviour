using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;


public static class BTEditorUtility
{
    public static string GetAssetFolderPath(string searchFilter, string folderPath = "")
    {
        foreach (var guid in AssetDatabase.FindAssets(searchFilter) ?? Enumerable.Empty<string>())
        {
            string parentPath = AssetDatabase.GUIDToAssetPath(guid);
            string resultPath = $"{parentPath}{folderPath}";

            if (Directory.Exists(resultPath))
            {
                return resultPath;
            }
        }

        return string.Empty;
    }


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
}
