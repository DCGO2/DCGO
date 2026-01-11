using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DCGO.Tools.Repair{
    public class FixDigiEggCost : MonoBehaviour
    {
        [MenuItem("Window/DCGO/Repair/Fix Digi-Egg Play Cost")]
        static void FixDigiEggPlayCost()
        {
            List<CEntity_Base> List = GetAsset.LoadAll<CEntity_Base>("Assets/CardBaseEntity/");
            List = List.Filter(x => x.cardKind == CardKind.DigiEgg).Filter(x => x.PlayCost == 0);

            foreach (CEntity_Base card in List)
            {
                card.PlayCost = -1;
                EditorUtility.SetDirty(card);
            }

            Debug.Log("Fixed all Digi-Egg play cost in CardBaseEntity");
            return;
        }
    }
}

