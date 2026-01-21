using DCGO.CardEntities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Policy;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Networking;

namespace DCGO.Tools
{
    public class FindMissingAAs : MonoBehaviour
    {
        static FindMissingAAs instance;
        string baseURL = "https://raw.githubusercontent.com/TakaOtaku/Digimon-Cards/main/src/assets/";
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
                    string ID = AA.id.Replace("-Errata", "");

                    if (FindFile(ID, card))
                    {
                        debugText += $"{ID}\n";
                        matchedCount++;
                    }
                }
            }

            Debug.Log(debugText);
            Debug.Log($"COMPLETED: {matchedCount}");
        }

        bool FindFile(string ID, CardData data)
        {
            string fileName = $"{FixCharactersInClassName($"{ID}")}.asset";

            string folderName_SetID = $"{GetParseByHyphen(data.id)[0]}";
            string folderName_CardColor = $"{DataBase.CardColorNameDictionary[GetCardColors(data.color)[0]]}";
            folderName_CardColor = char.ToUpper(folderName_CardColor[0]) + folderName_CardColor.Substring(1);

            string folderName_CardKind = $"{DataBase.CardKindENNameDictionary[DictionaryUtility.GetCardKind(data.cardType.Replace("-", ""), DataBase.CardKindENNameDictionary)]}";
            string folderPath = $"Assets/CardBaseEntity/{folderName_SetID}/{folderName_CardColor}/{folderName_CardKind}";

            if (Directory.Exists(folderPath))
            {
                string filePath = $"{folderPath}/{fileName}".Trim().Replace("\t", "").Replace("\n", "").Replace("\r", "").Replace(" ", "");

                CEntity_Base card = GetAsset.Load<CEntity_Base>(filePath);

                if (card == null)
                {
                    return GetImage(ID);
                }
            }

            return false;
        }
        bool GetImage(string ID)
        {
            string imgURL = $"{baseURL}images/cards/{ID}.webp";

            Debug.Log(imgURL);
            try
            {
                WebClient wc = new WebClient();
                string HTMLSource = wc.DownloadString(imgURL);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
                return false;
            }
        }

        IEnumerator GetJsonData()
        {
            string url = baseURL + "cardlists/DigimonCards.json";
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