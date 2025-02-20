using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using DCGO.CardEntities;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

namespace DCGO.Tools
{
    public class GetLowestCardIndex : MonoBehaviour
    {
        [MenuItem("Window/DCGO/Get Index/Get Lowest Card Index")]
        static void GetIndex()
        {

            string path = "";
            int minValue = 0;

            if (Selection.assetGUIDs.Length != 0)
                path = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);

            if (path != "")
            {
                List<CEntity_Base> List = GetAsset.LoadAll<CEntity_Base>(path)
                .OrderBy(x => x.CardIndex).ToList();

                minValue = List[0].CardIndex;
            }

            List<CEntity_Base> Entities = GetAsset.LoadAll<CEntity_Base>("Assets/CardBaseEntity/")
            .Filter(entity => !entity.CardID.Contains("P-"))
            .Filter(entity => entity.CardIndex >= minValue);
            int cardIndex = 0;
            string name = "";

            foreach (CEntity_Base card in Entities)
            {
                if (card.CardID.Contains("P-"))
                    continue;

                if (cardIndex == card.CardIndex)
                    Debug.LogWarning($"DUPLICATE INDEX FOUND: {card.CardSpriteName} - {cardIndex}");

                if (card.CardIndex < cardIndex || cardIndex == 0)
                {
                    cardIndex = card.CardIndex;
                    name = card.CardSpriteName;
                }
            }

            Debug.Log($"Lowest Card Index: {cardIndex} - {name}");
        }
    }

    public class GetLowestPromoCardIndex : MonoBehaviour
    {
        [MenuItem("Window/DCGO/Get Index/Get Lowest Promo Index")]
        static void GetIndex()
        {
            string path = "";
            int minValue = 0;

            if (Selection.assetGUIDs.Length != 0)
                path = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);

            if(path != "")
            {
                CEntity_Base entity = GetAsset.Load<CEntity_Base>(path);

                minValue = entity.CardIndex;
            }
            

            List<CEntity_Base> Entities = GetAsset.LoadAll<CEntity_Base>("Assets/CardBaseEntity/")
            .Filter(entity => entity.CardID.Contains("P-"))
            .Filter(entity => entity.CardIndex > minValue);

            int cardIndex = 0;
            string name = "";

            Debug.Log($"CARD COUNT: {Entities.Count}");
            foreach (CEntity_Base card in Entities)
            {
                if (cardIndex == card.CardIndex)
                    Debug.LogWarning($"DUPLICATE INDEX FOUND: {card.CardSpriteName} - {cardIndex}");

                if ((card.CardIndex < cardIndex || cardIndex == 0))
                {
                    cardIndex = card.CardIndex;
                    name = card.CardSpriteName;
                }
            }

            Debug.Log($"Lowest Card Index: {cardIndex} - {name}");
        }
    }
}