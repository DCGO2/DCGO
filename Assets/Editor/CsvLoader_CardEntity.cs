using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System;

[CustomEditor(typeof(LoadCSV_CardEntity))]
public class CsvLoader_CardEntity : Editor
{
    public override void OnInspectorGUI()
    {
        var LoadCSV = target as LoadCSV_CardEntity;
        DrawDefaultInspector();

        if (GUILayout.Button("Create Scriptable Objects"))
        {
            EditorCoroutineUtility.StartCoroutine(SetCsvDataToScriptableObject(LoadCSV), this);
        }
    }

    IEnumerator SetCsvDataToScriptableObject(LoadCSV_CardEntity loadCSV)
    {
        List<TextAsset> CardListTexts = GetAsset.LoadAll<TextAsset>("Assets/TextAsset");

        int value = 0;

        // Check if CSV file is null
        if (loadCSV.csvFile == null)
        {
            Debug.LogWarning("CSV File is null: " + loadCSV.name);
            yield break;
        }

        Debug.Log("CSV File Found: " + loadCSV.name);

        // CSVÇ Text
        string csvText = loadCSV.csvFile.text;

        //Split by new line
        string[] afterParse = csvText.Split('\n');

        //Skip the first line, parse by ","
        for (int i = 1; i < afterParse.Length; i++)
        {
            string[] parseByComma = afterParse[i].Split(',');

            // çskip if first column is empty
            if (string.IsNullOrEmpty(parseByComma[0]))
            {
                continue;
            }

            int column = 0;

            string cardImageName = parseByComma[column].CleanTrimString().Replace(".jpg", "").Replace(".png", "").Trim();
            column++;

            yield return EditorCoroutineUtility.StartCoroutine(CreateCardData(cardImageName), this);

            IEnumerator CreateCardData(string CardImageName)
            {
                
                // CardEntityÇ instance created
                CEntity_Base cardEntity = CreateInstance<CEntity_Base>();

                cardEntity.CardSpriteName = CardImageName;

                //Set card effect class name
                cardEntity.CardEffectClassName = parseByComma[column];
                column++;

                //Set card index number
                if (int.TryParse(parseByComma[column], out value))
                {
                    cardEntity.CardIndex = value;
                }
                column++;

                //Parse max count (most csv this is empty)
                if (!string.IsNullOrEmpty(parseByComma[column]))
                {
                    if (int.TryParse(parseByComma[column], out value))
                    {
                        cardEntity.MaxCountInDeck = value;
                    }
                }
                column++;

                //Get Card Image Name
                string[] parseByUnderBar = CEntity_Base.GetParseByUnderBar(CardImageName);

                cardEntity.CardID = $"{parseByUnderBar[0]}";

                string customCardID = "";

                if (parseByComma.Length >= column + 1)
                {
                    if (!string.IsNullOrEmpty(parseByComma[column]))
                    {
                        if (parseByComma[column].Contains("-"))
                        {
                            customCardID = parseByComma[column];
                        }
                    }
                }
                column++;

                //Get Japanese and English card ID
                List<string> CardListIDs_JPN = DataBase.CardListIDs(CEntity_Base.GetParseByHyphen(CardImageName)[0], false);
                List<string> CardListIDs_ENG = DataBase.CardListIDs(CEntity_Base.GetParseByHyphen(CardImageName)[0], true);

                List<TextAsset> CardListTexts_JPN = new List<TextAsset>();

                foreach (TextAsset textAsset in CardListTexts)
                {
                    if (CardListIDs_JPN.Contains(textAsset.name))
                    {
                        CardListTexts_JPN.Add(textAsset);
                    }
                }

                List<TextAsset> CardListTexts_ENG = new List<TextAsset>();

                foreach (TextAsset textAsset in CardListTexts)
                {
                    if (CardListIDs_ENG.Contains(textAsset.name))
                    {
                        CardListTexts_ENG.Add(textAsset);
                    }
                }

                List<string[]> CardDatas_JPN = OfficialCardListUtility.GetCardDatas(CardListTexts_JPN);
                List<string[]> CardDatas_ENG = OfficialCardListUtility.GetCardDatas(CardListTexts_ENG);

                List<int> evoCosts_level = new List<int>();
                List<int> evoCosts_memory = new List<int>();

                OfficialCardListUtility.AttachCardData(cardEntity, CardDatas_JPN, CardDatas_ENG, ref evoCosts_level, ref evoCosts_memory);

                if (cardEntity.cardKind == CardKind.Digimon)
                {
                    if (evoCosts_level.Count >= 1)
                    {
                        List<CardColor> evoCosts_color = new List<CardColor>();

                        //wikiÇgrab text from site
                        yield return new EditorWaitForSeconds(0.5f);
                        WebClient wc = new WebClient();
                        wc.Encoding = Encoding.UTF8;
                        string resultText = wc.DownloadString($"https://wikimon.net/{cardEntity.CardID}_(DCG)");
                        string[] parseByEnter = resultText.Split('\n');

                        //êiâªÉRÉXÉgÇÃêF
                        for (int j = 0; j < parseByEnter.Length; j++)
                        {
                            string text = parseByEnter[j];

                            if (!string.IsNullOrEmpty(text))
                            {
                                text = text.Replace("\n", "").Replace("\t", "").Replace(" ", "").Trim();

                                if (text.Contains(">fromLv."))
                                {
                                    if (text.Contains("linear-gradient"))
                                    {
                                        evoCosts_color.Add(CardColor.Red);
                                        evoCosts_color.Add(CardColor.Blue);
                                        evoCosts_color.Add(CardColor.Yellow);
                                        evoCosts_color.Add(CardColor.Green);
                                        evoCosts_color.Add(CardColor.Black);
                                        evoCosts_color.Add(CardColor.Purple);
                                        evoCosts_color.Add(CardColor.White);
                                    }

                                    else
                                    {
                                        int startIndex = text.IndexOf('#');
                                        int length = 7;

                                        text = text.Substring(startIndex, length);

                                        switch (text)
                                        {
                                            case "#cb2e32":
                                                evoCosts_color.Add(CardColor.Red);
                                                break;

                                            case "#0099dc":
                                                evoCosts_color.Add(CardColor.Blue);
                                                break;

                                            case "#f0ac09":
                                                evoCosts_color.Add(CardColor.Yellow);
                                                break;

                                            case "#009f6d":
                                                evoCosts_color.Add(CardColor.Green);
                                                break;

                                            case "#4e4e4e":
                                                evoCosts_color.Add(CardColor.Black);
                                                break;

                                            case "#7e67aa":
                                                evoCosts_color.Add(CardColor.Purple);
                                                break;

                                            case "#ceddef":
                                                evoCosts_color.Add(CardColor.White);
                                                break;
                                        }
                                    }
                                }
                            }
                        }

                        if (evoCosts_color.Count < evoCosts_level.Count)
                        {
                            if (cardEntity.cardColors.Count >= 1)
                            {
                                evoCosts_color.Add(cardEntity.cardColors[0]);
                            }
                        }

                        for (int j = 0; j < evoCosts_color.Count; j++)
                        {
                            EvoCost evoCost = new EvoCost();
                            evoCost.CardColor = evoCosts_color[j];

                            int level = evoCosts_level[evoCosts_level.Count - 1];

                            if (j < evoCosts_level.Count)
                            {
                                level = evoCosts_level[j];
                            }

                            evoCost.Level = level;

                            int memoryCost = evoCosts_memory[evoCosts_memory.Count - 1];

                            if (j < evoCosts_memory.Count)
                            {
                                memoryCost = evoCosts_memory[j];
                            }

                            evoCost.MemoryCost = memoryCost;

                            cardEntity.EvoCosts.Add(evoCost);
                        }
                    }

                    #region BT9-059(ÉoÉNÉÇÉì)
                    if (cardEntity.CardID == "BT9-059")
                    {
                        if (cardEntity.EvoCosts.Count((evoCost) => evoCost.CardColor == CardColor.Black && evoCost.MemoryCost == 0 && evoCost.Level == 2) == 0)
                        {
                            EvoCost evoCost = new EvoCost();
                            evoCost.CardColor = CardColor.Black;
                            evoCost.MemoryCost = 0;
                            evoCost.Level = 2;

                            cardEntity.EvoCosts.Add(evoCost);
                        }
                    }
                    #endregion

                    #region BT9-041(ÉâÉCÉYÉOÉåÉCÉÇÉìXçRëÃ)
                    if (cardEntity.CardID == "BT9-041")
                    {
                        if (cardEntity.EvoCosts.Count((evoCost) => evoCost.CardColor == CardColor.Red && evoCost.MemoryCost == 4 && evoCost.Level == 4) == 0)
                        {
                            EvoCost evoCost = new EvoCost();
                            evoCost.CardColor = CardColor.Red;
                            evoCost.MemoryCost = 4;
                            evoCost.Level = 4;

                            cardEntity.EvoCosts.Add(evoCost);
                        }
                    }
                    #endregion

                    #region BT14-018(ÉSÉbÉhÉhÉâÉÇÉì)
                    if (cardEntity.CardID == "BT14-018")
                    {
                        if (cardEntity.EvoCosts.Count((evoCost) => evoCost.CardColor == CardColor.Yellow && evoCost.MemoryCost == 4 && evoCost.Level == 5) == 0)
                        {
                            EvoCost evoCost = new EvoCost();
                            evoCost.CardColor = CardColor.Yellow;
                            evoCost.MemoryCost = 4;
                            evoCost.Level = 5;

                            cardEntity.EvoCosts.Add(evoCost);
                        }
                    }
                    #endregion

                    #region ST12-09(É{ÉãÉPÅ[ÉÇÉì)
                    if (cardEntity.CardID == "ST12-09")
                    {
                        if (cardEntity.EvoCosts.Count((evoCost) => evoCost.CardColor == CardColor.Black && evoCost.MemoryCost == 4 && evoCost.Level == 4) == 0)
                        {
                            EvoCost evoCost = new EvoCost();
                            evoCost.CardColor = CardColor.Black;
                            evoCost.MemoryCost = 4;
                            evoCost.Level = 4;

                            cardEntity.EvoCosts.Add(evoCost);
                        }
                    }
                    #endregion

                    cardEntity.EvoCosts = cardEntity.EvoCosts.Distinct().ToList();
                }

                #region ÉtÉ@ÉCÉãñºÇ∆ï€ë∂êÊ
                //ÉtÉ@ÉCÉãñº
                string fileName = $"{cardEntity.CardName_ENG}-{CardImageName}.asset";
                fileName = fileName.Replace("?", "ÅH").Replace(":", "ÅF");
                cardEntity.name = fileName.Replace(".asset", "");
                //ï€ë∂êÊ
                string folderName_SetID = $"{cardEntity.SetID}";
                string folderName_CardColor = $"{DataBase.CardColorNameDictionary[cardEntity.cardColors[0]]}";
                folderName_CardColor = char.ToUpper(folderName_CardColor[0]) + folderName_CardColor.Substring(1);
                string folderName_CardKind = $"{DataBase.CardKindENNameDictionary[cardEntity.cardKind]}";

                string folderPath = $"Assets/CardBaseEntity/{folderName_SetID}/{folderName_CardColor}/{folderName_CardKind}";

                string filePath = $"{folderPath}/{fileName}".Trim().Replace("\t", "").Replace("\n", "").Replace("\r", "").Replace(" ", "");

                //ÉtÉHÉãÉ_Ç™ñ≥ÇØÇÍÇŒçÏê¨
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                #endregion

                if (!string.IsNullOrEmpty(customCardID))
                {
                    cardEntity.CardID = customCardID;
                }

                // ÉAÉZÉbÉgÇ∆ÇµÇƒï€ë∂
                var asset = (CEntity_Base)AssetDatabase.LoadAssetAtPath(filePath, typeof(CEntity_Base));

                if (asset == null)
                {
                    // Attempt to create Asset
                    AssetDatabase.CreateAsset(cardEntity, filePath);
                }
                else
                {
                    // ï€ë∂êÊÇÃÉpÉXÇ…ÉtÉ@ÉCÉãÇ™Ç†ÇÍÇŒè„èëÇ´
                    EditorUtility.CopySerialized(cardEntity, asset);
                    AssetDatabase.SaveAssets();
                }

                AssetDatabase.Refresh();

                Debug.Log($"{i}/{afterParse.Length - 1}:{cardImageName}ÇScriptable Object Created");
            }
        }

        Debug.Log("Card Data Created for: " + loadCSV.name);
    }

    string GetCardDataURL(string CardImageName)
    {
        string URL = "";

        string[] parseByUnderBar = CEntity_Base.GetParseByUnderBar(CardImageName);

        URL = $"https://www.unionarena-tcg.com/jp/cardlist/detail_iframe.php?card_no={parseByUnderBar[3]}/{parseByUnderBar[0]}-{parseByUnderBar[1]}-{parseByUnderBar[2]}";

        return URL;
    }
}

/// <summary>
/// string extension methods
/// </summary>
public static partial class StringExtensions
{

    /// <summary>
    /// éwíËÇµÇΩï∂éöóÒÇ™Ç¢Ç≠Ç¬Ç†ÇÈÇ©
    /// </summary>
    public static int CountOf(this string self, params string[] strArray)
    {
        int count = 0;

        foreach (string str in strArray)
        {
            int index = self.IndexOf(str, 0);
            while (index != -1)
            {
                count++;
                index = self.IndexOf(str, index + str.Length);
            }
        }

        return count;
    }

    /// <summary>
    /// Removes all instances of the specified string from the current string.
    /// </summary>
    public static string CleanTrimString(this string self)
    {
        string cleanString = self;

        if (!string.IsNullOrEmpty(self))
        {
            foreach (string str in new string[] { " ", "\n", "\r", "\t" })
            {
                cleanString = cleanString.Replace(str, "");
            }
        }

        return cleanString.Trim();
    }

}

