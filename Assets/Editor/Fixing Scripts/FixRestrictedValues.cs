using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCGO.Tools.Repair
{
    public class FixRestrictedValues : MonoBehaviour
    {
        [MenuItem("Window/DCGO/Repair/Fix Max Count In Deck")]
        static void FixRestrictedValuesData()
        {
            List<CEntity_Base> List = GetAsset.LoadAll<CEntity_Base>("Assets/CardBaseEntity/");
            List = List.Filter(x => x.MaxCountInDeck < 4);

            foreach (CEntity_Base card in List)
            {
                card.MaxCountInDeck = 4;
                EditorUtility.SetDirty(card);
            }

            Debug.Log("Fixed all Max Count in Deck values in CardBaseEntity");
            return;
        }
    }
}