using DCGO.CardEntities;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace DCGO.Tools
{
    public class FindMissingAAs : MonoBehaviour
    {
        static FindMissingAAs instance;
        string baseURL = "https://raw.githubusercontent.com/TakaOtaku/Digimon-Card-App/main/src/";
        public List<CardData> _cardData;

        string debugText = "";
        int matchedCount = 0;

        [MenuItem("Window/DCGO/Find Missing AAs")]
        static void ErrataImages()
        {
            instance = new FindMissingAAs();
            EditorCoroutineUtility.StartCoroutine(instance.FindAAs(), instance);
        }

        IEnumerator FindAAs()
        {
            yield return EditorCoroutineUtility.StartCoroutine(instance.GetJsonData(), instance);

            List<CardData> aaCards = _cardData.Filter(x => x.AAs.Count > 0);

            foreach (CardData card in aaCards)
            {
                foreach(AlternateArt AA in card.AAs)
                {
                    FindFile(AA.id.Replace("-Errata",""), card);
                }                    
            }

            Debug.Log(debugText);
            Debug.Log($"COMPLETED: {matchedCount}");
        }

        void FindFile(string ID, CardData data)
        {
            string fileName = $"{FixCharactersInClassName($"{ID}")}.asset";

            string folderName_SetID = $"{GetParseByHyphen(data.id)[0]}";
            string folderName_CardColor = $"{DataBase.CardColorNameDictionary[GetCardColors(data.color)[0]]}";
            folderName_CardColor = char.ToUpper(folderName_CardColor[0]) + folderName_CardColor.Substring(1);

            string folderName_CardKind = $"{DataBase.CardKindENNameDictionary[DictionaryUtility.GetCardKind(data.cardType.Replace("-", ""), DataBase.CardKindENNameDictionary)]}";
            string folderPath = $"Assets/CardBaseEntity/{folderName_SetID}/{folderName_CardColor}/{folderName_CardKind}";

            if (!Directory.Exists(folderPath))
                return;

            string filePath = $"{folderPath}/{fileName}".Trim().Replace("\t", "").Replace("\n", "").Replace("\r", "").Replace(" ", "");

            CEntity_Base card = GetAsset.Load<CEntity_Base>(filePath);

            if (card == null)
            {
                Debug.Log($"NO ASSET FOUND: {filePath}");
                debugText += $"{ID}\n";
                matchedCount++;
                return;
            }
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

        //Parse ScriptableObject Name
        string FixCharactersInName(string str)
        {
            string name = str;

            name = name
                .Replace(" ", "_")
                .Replace(":", "")
                .Replace("?", "")
                .Replace("!", "")
                .Replace("<", "")
                .Replace(">", "");

            return name;
        }

        //Parse ScriptableObject Class Name
        public string FixCharactersInClassName(string str)
        {
            string name = str;

            name = FixCharactersInName(name);

            name = name
                .Replace("-", "_")
                .Replace(".", "")
                .Replace("'", "")
                .Replace("&", "And")
                .Replace("(", "")
                .Replace(")", "");

            return name;
        }

        public static string[] GetParseByHyphen(string CardImageName)
        {
            string[] parseByHyphen = new string[] { CardImageName };

            if (CardImageName.Contains('-'))
            {
                parseByHyphen = CardImageName.Split('-');
            }

            return parseByHyphen;
        }

        //Parse card colors to list
        List<CardColor> GetCardColors(string colors)
        {
            List<CardColor> cardColors = new List<CardColor>();

            foreach (string cardColorName in colors.Split("/"))
            {
                foreach (string cardColorNameValues in DataBase.CardColorNameDictionary.Values)
                {
                    if (cardColorName.ToLower().Trim() == cardColorNameValues)
                    {
                        cardColors.Add(DictionaryUtility.GetCardColor(cardColorName.ToLower().Trim(), DataBase.CardColorNameDictionary));
                    }
                }
            }

            return cardColors;
        }
    }
}