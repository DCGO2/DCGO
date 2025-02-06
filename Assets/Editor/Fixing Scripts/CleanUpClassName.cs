using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCGO.Tools.Repair{
    public class CleanUpClassName : MonoBehaviour
    {
        [MenuItem("Window/DCGO/Repair/Fix Entity Class Names")]
        static void FixEntityClassNames()
        {
            List<CEntity_Base> List = GetAsset.LoadAll<CEntity_Base>("Assets/CardBaseEntity/");

            foreach (CEntity_Base card in List)
            {
                card.CardEffectClassName = FixCharactersInClassName(card.CardEffectClassName);
                EditorUtility.SetDirty(card);
            }
                

            Debug.Log("Fixed all class names in CardBaseEntity");
            return;

            //Parse ScriptableObject Class Name
            string FixCharactersInClassName(string str)
            {
                string name = str;

                name = FixCharactersInName(name);

                name = name
                    .Replace("-", "_")
                    .Replace(".", "")
                    .Replace("'", "")
                    .Replace("&", "And")
                    .Replace("(", "")
                    .Replace(")", "");

                return name;
            }

            //Parse ScriptableObject Name
            string FixCharactersInName(string str)
            {
                string name = str;

                name = name
                    .Replace(" ", "_")
                    .Replace(":", "")
                    .Replace("?", "")
                    .Replace("!", "")
                    .Replace("<", "")
                    .Replace(">", "");

                return name;
            }
        }

        [MenuItem("Window/DCGO/Repair/Convert Entity Class Names")]
        static void ConvertEntityClassNames()
        {
            string path = "Assets/CardBaseEntity/";

            if (Selection.assetGUIDs.Length != 0)
                path = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);

            List<CEntity_Base> List = GetAsset.LoadAll<CEntity_Base>(path);

            string cardList = "";
            int count = 0;

            foreach (CEntity_Base card in List)
            {
                string cardID = card.CardID.Replace("-", "_");

                if(!String.IsNullOrEmpty(card.CardEffectClassName))
                    card.CardEffectClassName = card.CardEffectClassName.Substring(card.CardEffectClassName.IndexOf("_") + 1);
                EditorUtility.SetDirty(card);
                Debug.Log($"{card.CardSpriteName.Replace("-", "_")} - {cardID} - {card.CardEffectClassName}");
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(card), card.CardSpriteName.Replace("-", "_"));
                count++;
                EditorUtility.SetDirty(card);
            }

            Debug.Log("Cards using another class:\n" + cardList);
            Debug.Log($"Fixed all class names in CardBaseEntity: {count}");
            return;
        }
    }
}

