using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DCGO.Tools.Repair
{
    public class FindEntitiesFromCardIndex : MonoBehaviour
    {
        [MenuItem("Window/DCGO/Repair/Find Entity Card Index")]
        static void FixEntityCardIndex()
        {
            int startingIndex = 10336;
            string path = "Assets/CardBaseEntity/";

            List<CEntity_Base> List = GetAsset.LoadAll<CEntity_Base>(path)
                .Where(x => x.CardIndex >= startingIndex)
                .OrderBy(x => x.CardIndex).ToList();

            foreach (CEntity_Base card in List)
            {
                Debug.Log($"{card.CardName_ENG}: {card.CardIndex} - {startingIndex}");
            }

            Debug.Log("Found all card index in CardBaseEntity");
            return;
        }
    }
}