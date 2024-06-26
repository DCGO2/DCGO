using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using DCGO.CardEntities;

namespace DCGO.Tools.Repair 
{
    public class GetHighestCardIndex : MonoBehaviour
    {
        [MenuItem("Window/DCGO/Repair/Get Highest Card Index")]
        static void FixEntityClassNames()
        {
            List<CEntity_Base> Entities = GetAsset.LoadAll<CEntity_Base>("Assets/CardBaseEntity/");
            int cardIndex = 0;

            foreach (CEntity_Base card in Entities)
            {
                if (card.CardID.Contains("P-"))
                    continue;

                if (card.CardIndex > cardIndex)
                    cardIndex = card.CardIndex;
            }

            Debug.Log($"Highest Card Index: {cardIndex}");
        }
    }
}