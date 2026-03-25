using DCGO.CardEntities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace DCGO.Tools.Repair
{
    public class FixForDualTypings : MonoBehaviour
    {
        
        static FixForDualTypings instance;
        string baseURL = "https://raw.githubusercontent.com/TakaOtaku/Digimon-Card-App/main/src/";
        public List<CardData> _cardData;

        [MenuItem("Window/DCGO/Repair/Fix Card Kind Data")]
        static void FixCardKind()
        {
            instance = new FixForDualTypings();
            EditorCoroutineUtility.StartCoroutine(instance.FixCardKinds(), instance);

            Debug.Log("Fixed all CardKind in CardBaseEntity");
            return;
        }

        IEnumerator FixCardKinds()
        {
            yield return EditorCoroutineUtility.StartCoroutine(instance.GetJsonData(), instance);

            List<CEntity_Base> Entities = GetAsset.LoadAll<CEntity_Base>("Assets/CardBaseEntity/");

            foreach (CEntity_Base card in Entities)
            {
                if (card.cardKind.Count > 0)
                    continue;

                CardData data = _cardData.Where(x => x.id == card.CardID).First();

                card.cardKind = DictionaryUtility.GetCardKind(data.cardType.Replace("-", ""), DataBase.CardKindENNameDictionary);
                EditorUtility.SetDirty(card);
            }

            Debug.Log($"COMPLETED");
        }

        IEnumerator GetJsonData()
        {
            string url = baseURL + "assets/cardlists/DigimonCards.json";
            UnityWebRequest jsonWebRequest = UnityWebRequest.Get(url);

            yield return jsonWebRequest.SendWebRequest();

            if (jsonWebRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(jsonWebRequest.error);
            }
            else
            {

                RootObject root = JsonUtility.FromJson<RootObject>("{\"cards\":" + jsonWebRequest.downloadHandler.text + "}");
                _cardData = root.cards;
            }

            yield return null;
        }
    }
}