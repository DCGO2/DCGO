using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using DCGO.CardEntities;

namespace DCGO.Tools.Repair{
    public class ChangeAssetsToEnglish : MonoBehaviour
    {
        [MenuItem("Window/DCGO/Repair/Change Entity Names To English")]
        static void FixEntityNames()
        {
            List<CEntity_Base> List = GetAsset.LoadAll<CEntity_Base>("Assets/CardBaseEntity/");

            foreach (CEntity_Base card in List)
            {
                string assetName = card.name;
                string correctName = $"{FixCharactersInName($"{card.CardName_ENG.Replace(" ", "")}-{card.CardSpriteName}")}";

                if (assetName.Equals(correctName))
                    continue;

                Debug.Log(assetName + " - " + correctName);
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(card), correctName);
                EditorUtility.SetDirty(card);
            }
                

            Debug.Log("All names changed in CardBaseEntity");
            return;

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
    }
}

