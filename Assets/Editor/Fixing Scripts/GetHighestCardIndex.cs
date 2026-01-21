using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using DCGO.CardEntities;

namespace DCGO.Tools 
{
    public class GetHighestCardIndex : MonoBehaviour
    {
        [MenuItem("Window/DCGO/Get Index/Get Highest Card Index")]
        static void GetIndex()
        {
            List<CEntity_Base> Entities = GetAsset.LoadAll<CEntity_Base>("Assets/CardBaseEntity/");
            int cardIndex = 0;
            string name = "";

            foreach (CEntity_Base card in Entities)
            {
                if (card.CardID.Contains("P-"))
                    continue;

                if (cardIndex == card.CardIndex)
                    Debug.LogWarning($"DUPLICATE INDEX FOUND: {card.CardSpriteName} - {cardIndex}");

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
        [MenuItem("Window/DCGO/Get Index/Get Highest Promo Index")]
        static void GetIndex()
        {
            List<CEntity_Base> Entities = GetAsset.LoadAll<CEntity_Base>("Assets/CardBaseEntity/");
            int cardIndex = 0;
            string name = "";

            foreach (CEntity_Base card in Entities)
            {
                if (!card.CardID.Contains("P-"))
                    continue;

                if (cardIndex == card.CardIndex)
                    Debug.LogWarning($"DUPLICATE INDEX FOUND: {card.CardSpriteName} - {cardIndex}");

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