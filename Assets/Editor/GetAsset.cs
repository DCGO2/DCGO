using UnityEditor;
using UnityEngine;

using System.IO;
using System.Collections.Generic;

/// <summary>
/// Resourcesディレクトリ以外のオブジェクトにアクセスすることができる。実はResourcesディレクトリのオブジェクトにもアクセスできる
/// </summary>
public static class GetAsset
{

    //=================================================================================
    //単体ロード
    //=================================================================================

    /// <summary>
    /// ファイルのパス(Assetsから、拡張子も含める)と型を設定し、Objectを読み込む。存在しない場合はNullを返す
    /// </summary>
    public static T Load<T>(string path) where T : Object
    {
        return AssetDatabase.LoadAssetAtPath<T>(path);
    }

    /// <summary>
    /// ファイルのパス(Assetsから、拡張子も含める)を設定し、Objectを読み込む。存在しない場合はNullを返す
    /// </summary>
    public static Object Load(string path)
    {
        return Load<Object>(path);
    }

    //=================================================================================
    //複数ロード
    //=================================================================================

    /// <summary>
    /// ディレクトリのパス(Assetsから)と型を設定し、Objectを読み込む。存在しない場合は空のListを返す
    /// </summary>
    public static List<T> LoadAll<T>(string directoryPath) where T : Object
    {
        List<T> assetList = new List<T>();

        //指定したディレクトリに入っている全ファイルを取得(子ディレクトリも含む)
        string[] filePathArray = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);

        //取得したファイルの中からアセットだけリストに追加する
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
    /// ディレクトリのパス(Assetsから)を設定し、Objectを読み込む。存在しない場合は空のListを返す
    /// </summary>
    public static List<Object> LoadAll(string directoryPath)
    {
        return LoadAll<Object>(directoryPath);
    }

}