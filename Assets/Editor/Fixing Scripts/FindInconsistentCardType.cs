using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using DCGO.CardEntities;
using Unity.EditorCoroutines.Editor;
using System.Collections;
using UnityEngine.Networking;
using WebSocketSharp;
using static UnityEngine.ParticleSystem;

namespace DCGO.Tools.Repair{


    public class FindInconsistentCardType : MonoBehaviour
    {
        InconsistentName _stringValue;
        List<TrackedData> _entities;


        [MenuItem("Window/DCGO/Repair/Find Inconsistent Card Types")]
        static void FindInconsistentCardTypes()
        {
            List<CEntity_Base> Entities = GetAsset.LoadAll<CEntity_Base>("Assets/CardBaseEntity/");

            foreach (CEntity_Base card in Entities)
            {
                if(card.Level == 2)
                {
                    if(card.cardKind != CardKind.DigiEgg)
                        Debug.Log($"Mismatched Type (Egg): {card.CardSprite} - {card.cardColors[0]}");
                }
                else if (card.Level > 2)
                {
                    if (card.cardKind != CardKind.Digimon)
                        Debug.Log($"Mismatched Type (Digimon): {card.CardSprite} - {card.cardColors[0]}");
                }
            }

            Debug.Log($"FIND INCONSISTANCY COMPLETE");
        }
    }
}

