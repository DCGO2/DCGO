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
            string name = "";

            foreach (CEntity_Base card in Entities)
            {
                if (card.CardID.Contains("P-"))
                    continue;

                if (card.CardIndex > cardIndex)
                {
                    cardIndex = card.CardIndex;
                    name = card.CardSpriteName;
                }
            }

            Debug.Log($"Highest Card Index: {cardIndex} - {name}");
        }
    }

    public class GetHighestPromoCardIndex : MonoBehaviour
    {
        [MenuItem("Window/DCGO/Repair/Get Highest Promo Index")]
        static void FixEntityClassNames()
        {
            List<CEntity_Base> Entities = GetAsset.LoadAll<CEntity_Base>("Assets/CardBaseEntity/");
            int cardIndex = 0;
            string name = "";

            foreach (CEntity_Base card in Entities)
            {
                if (!card.CardID.Contains("P-"))
                    continue;

                if (card.CardIndex > cardIndex)
                {
                    cardIndex = card.CardIndex;
                    name = card.CardSpriteName;
                }
            }

            Debug.Log($"Highest Card Index: {cardIndex} - {name}");
        }
    }
}