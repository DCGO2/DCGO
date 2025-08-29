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
    public class FixErrataImages : MonoBehaviour
    {
        static FixErrataImages instance;
        string baseURL = "https://raw.githubusercontent.com/TakaOtaku/Digimon-Cards/main/src/";
        public List<CardData> _cardData;

        string debugText = "";
        int matchedCount = 0;

        #region Fix Errata Images
        [MenuItem("Window/DCGO/Repair/Fix Errata Images")]
        static void ErrataImages()
        {
            instance = new FixErrataImages();
            EditorCoroutineUtility.StartCoroutine(instance.FixImages(), instance);
        }

        IEnumerator FixImages()
        {
            yield return EditorCoroutineUtility.StartCoroutine(instance.GetJsonData(), instance);

            List<CEntity_Base> Entities = GetAsset.LoadAll<CEntity_Base>("Assets/CardBaseEntity/");

            foreach (CEntity_Base card in Entities)
            {
                CardData data = _cardData.Where(x => x.id == card.CardID).First();

                if (card.CardSpriteName.Contains("-Errata"))
                {
                    Debug.Log($"Already is an Errata: {card.CardID}");
                    continue;
                }

                string errataName = FindMatchingErrata(card.CardSpriteName, data);

                if (card.CardSpriteName != errataName)
                {
                    card.CardSpriteName = errataName;
                    EditorUtility.SetDirty(card);
                }
            }

            Debug.Log(debugText);
            Debug.Log($"COMPLETED: {matchedCount}");
        }

        string FindMatchingErrata(string imageName, CardData data)
        {
            string name = imageName;

            foreach (AlternateArt AA in data.AAs)
            {
                if (AA.id.Replace("-Errata", "") != imageName)
                    continue;

                name = AA.id;
            }

            if (name != imageName)
            {
                debugText += $"{imageName}\n";
                matchedCount++;
            }

            return name;
        }

        #endregion

        #region Fix Sample Images
        [MenuItem("Window/DCGO/Repair/Fix Sample Images")]
        static void SampleImages()
        {
            instance = new FixErrataImages();
            instance.FixSampleImages();
        }

        void FixSampleImages()
        {
            string path = "Assets/CardBaseEntity/";

            if (Selection.assetGUIDs.Length != 0)
                path = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);

            Debug.Log($"ASSET PATH: {path}");

            List<CEntity_Base> Entities = GetAsset.LoadAll<CEntity_Base>(path);

            foreach (CEntity_Base card in Entities)
            {
                string errataName = FindMatchingSample(card.CardSpriteName);

                if (card.CardSpriteName != errataName)
                {
                    card.CardSpriteName = errataName;
                    EditorUtility.SetDirty(card);
                }
            }

            Debug.Log($"COMPLETED");
        }

        string FindMatchingSample(string imageName)
        {
            string name = imageName;

            if (!name.Contains("_P"))
                name += "-Sample";

            return name;
        }

        #endregion

        #region Remove Sample Images
        [MenuItem("Window/DCGO/Repair/Remove Sample Images")]
        static void RemoveSample()
        {
            instance = new FixErrataImages();
            instance.RemoveSampleImages();
        }

        void RemoveSampleImages()
        {
            string path = "Assets/CardBaseEntity/";

            if (Selection.assetGUIDs.Length != 0)
                path = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);

            Debug.Log($"ASSET PATH: {path}");

            List<CEntity_Base> Entities = GetAsset.LoadAll<CEntity_Base>(path);

            foreach (CEntity_Base card in Entities)
            {
                string errataName = RemoveMatchingSample(card.CardSpriteName);

                if (card.CardSpriteName != errataName)
                {
                    card.CardSpriteName = errataName;
                    EditorUtility.SetDirty(card);
                }
            }

            Debug.Log($"COMPLETED");
        }

        string RemoveMatchingSample(string imageName)
        {
            string name = imageName;

            if (name.Contains("-Sample"))
                name = name.Remove(name.IndexOf("-Sample"));

            return name;
        }

        #endregion



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

        void SetSpriteName(string ID, CardData data)
        {
            string fileName = $"{FixCharactersInClassName($"{ID}")}.asset";

            string folderName_SetID = $"{GetParseByHyphen(data.id)[0]}";

            string folderName_CardColor = $"{DataBase.CardColorNameDictionary[GetCardColors(data.color)[0]]}";
            folderName_CardColor = char.ToUpper(folderName_CardColor[0]) + folderName_CardColor.Substring(1);

            string folderName_CardKind = $"{DataBase.CardKindENNameDictionary[DictionaryUtility.GetCardKind(data.cardType.Replace("-", ""), DataBase.CardKindENNameDictionary)]}";
            string folderPath = $"Assets/CardBaseEntity/{folderName_SetID}/{folderName_CardColor}/{folderName_CardKind}";
            string filePath = $"{folderPath}/{fileName}".Trim().Replace("\t", "").Replace("\n", "").Replace("\r", "").Replace(" ", "");

            CEntity_Base card = GetAsset.Load<CEntity_Base>(filePath);

            if (card == null)
            {
                Debug.Log($"NO ASSET FOUND: {fileName}");
                return;
            }
            string errataName = GetImageURL(data.cardImage, data.cardNumber, data.AAs);

            if (!errataName.Equals(card.CardSpriteName))
            {
                debugText += $"{fileName}: {errataName}\n";
                matchedCount++;
                //card.CardSpriteName = errataName;
                //EditorUtility.SetDirty(card);
            }
        }

        string GetImageURL(string url, string ID, List<AlternateArt> AA)
        {
            string[] urlSplit = url.Split(".");
            string startURL = urlSplit[0].Substring(0, urlSplit[0].LastIndexOf("/"));

            if (!ID.Contains("Errata") && !ID.Contains("_") && AA.Count > 0)
            {
                AlternateArt errata = AA.Where(x => x.id.StartsWith(ID) && x.id.Contains("Errata")).FirstOrDefault();

                if(ID.Contains("BT16-077"))

                if (errata != null)
                    ID = errata.id;
            }

            return $"{ID}";
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