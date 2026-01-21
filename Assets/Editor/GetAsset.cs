using UnityEditor;
using UnityEngine;

using System.IO;
using System.Collections.Generic;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;
using UnityEngine.Windows;

/// <summary>
/// Resources Objects other than directories can be accessed. In fact, you can also access objects in the Resources directory.
/// </summary>
public static class GetAsset
{

    //=================================================================================
    //Single load
    //=================================================================================

    /// <summary>
    /// Set the file path (including the extension from Assets) and type, and load the Object. Returns Null if not present
    /// </summary>
    public static T Load<T>(string path) where T : Object
    {
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }

    /// <summary>
    /// Set the file path (from Assets, including the extension) and load the Object. Returns Null if not present
    /// </summary>
    public static Object Load(string path)
    {
        return Load<Object>(path);
    }

    //=================================================================================
    //multiple loads
    //=================================================================================

    /// <summary>
    /// Set the directory path(from Assets) and type, and load the Object.Returns an empty List if it does not exist
    /// </summary>
    public static List<T> LoadAll<T>(string directoryPath) where T : Object
    {
        List<T> assetList = new List<T>();

        //Get all files in the specified directory (including child directories)
        string[] filePathArray = System.IO.Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);

        //Add only assets from the acquired files to the list
        foreach (string filePath in filePathArray)
        {
            T asset = Load<T>(filePath);
            if (asset != null)
            {
                assetList.Add(asset);
            }
        }

        return assetList;
    }

    /// <summary>
    /// Set the directory path (from Assets) and read the Object. Returns an empty List if it does not exist
    /// </summary>
    public static List<Object> LoadAll(string directoryPath)
    {
        return LoadAll<Object>(directoryPath);
    }

}