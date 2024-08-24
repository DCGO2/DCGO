using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DCGO.Tools.Repair
{
    public class AdjustSetSpecificCardIndex : MonoBehaviour
    {
        [MenuItem("Window/DCGO/Repair/Fix Entity Card Index")]
        static void FixEntityCardIndex()
        {
            int startingIndex = 3541;
            string path = "Assets/CardBaseEntity/";

            if (Selection.assetGUIDs.Length != 0)
                path = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);

            Debug.Log($"ASSET PATH: {path}");

            List<CEntity_Base> List = GetAsset.LoadAll<CEntity_Base>(path)
                .OrderBy(x => x.name.Substring(x.name.LastIndexOf("-")+1,3)).ToList();

            foreach (CEntity_Base card in List)
            {
                Debug.Log($"{card.name}: {card.CardIndex} - {startingIndex}");
            
                card.CardIndex = startingIndex;
                EditorUtility.SetDirty(card);
                startingIndex++;
            }


            Debug.Log("Fixed all card index in CardBaseEntity");
            return;
        }
    }
}