using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Analytics;
using WebSocketSharp;

namespace DCGO.Tools.Repair
{
    public class MatchClassNames : MonoBehaviour
    {
        [MenuItem("Window/DCGO/Repair/MatchClassNames")]
        static void FixEntityCardIndex()
        {
            Dictionary<string,string> classNameDictionary = new Dictionary<string,string>();
            string path = "Assets/CardBaseEntity/";

            if (Selection.assetGUIDs.Length != 0)
                path = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);

            Debug.Log($"ASSET PATH: {path}");

            List<CEntity_Base> List = GetAsset.LoadAll<CEntity_Base>(path).ToList();

            string assetName = "";

            //Locate All mismatched classNames
            foreach (CEntity_Base card in List)
            {
                assetName = card.CardSpriteName.Replace("-", "_");

                if (assetName == card.name)
                {
                    Debug.Log($"Matches already: {card.name}");
                    continue;
                }

                Debug.Log($"Name Change: {card.name} - {assetName}");

                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(card), assetName);
                EditorUtility.SetDirty(card);
            }

            Debug.Log($"DONE");
            return;
        }
    }
}