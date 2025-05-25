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

            List<CEntity_Base> List = GetAsset.LoadAll<CEntity_Base>(path).ToList();
            List<CEntity_Base> normalArts = List.Filter(x => !x.name.Contains("_P"));
            List<CEntity_Base> alternateArts = List.Filter(x => x.name.Contains("_P"));

            string assetName = "";
            int mismatchCount = 0;

            //Locate All mismatched classNames
            foreach (CEntity_Base card in normalArts)
            {
                assetName = card.CardEffectClassName;

                List<CEntity_Base> matchingCards = alternateArts.Filter(x => x.CardID == card.CardID);

                foreach(CEntity_Base matchingCard in matchingCards)
                {
                    if(matchingCard.CardEffectClassName != assetName)
                    {
                        Debug.Log($"Mismatch Found: {matchingCard.name}");
                        mismatchCount++;
                        matchingCard.CardEffectClassName = assetName;
                        EditorUtility.SetDirty(matchingCard);
                    }
                }
            }

            Debug.Log($"DONE: mismatches found: {mismatchCount}");
            return;
        }
    }
}