using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Text.RegularExpressions;

public class DeckCodeUtility
{
    public static string GetDeckBuilderFile(DeckData data)
    {
        string DeckBuilderFile = "";

        DeckBuilderFile += $"Name: {data.DeckName}\n";
        DeckBuilderFile += $"Key Card: {data.KeyCardId}\n";
        DeckBuilderFile += $"Sort Index: {data.SortValue}\n\n";

        DeckBuilderFile += GetDeckBuilderDeckCode(data.AllDeckCards());

        return DeckBuilderFile;
    }

    [Obsolete]
    public static string GetTTSDeckCode(List<CEntity_Base> AllDeckCards)
    {
        string TTSDeckCode = "";

        TTSDeckCode += "[\"Exported from DCGO\"";

        foreach (CEntity_Base cEntity_Base in AllDeckCards)
        {
            TTSDeckCode += $",\"{cEntity_Base.CardID}\"";
        }

        TTSDeckCode += "]";

        return TTSDeckCode;
    }

    public static string GetDeckBuilderDeckCode(List<CEntity_Base> AllDeckCards)
    {
        string DeckBuilderDeckCode = "";

        DeckBuilderDeckCode += "// DeckList\n\n";

        List<CEntity_Base> distinctAllDeckCards = new List<CEntity_Base>();

        foreach (CEntity_Base cEntity_Base in AllDeckCards)
        {
            if (distinctAllDeckCards.Count((cEntity_Base1) => cEntity_Base.CardSpriteName == cEntity_Base1.CardSpriteName) == 0)
            {
                distinctAllDeckCards.Add(cEntity_Base);
            }
        }

        foreach (CEntity_Base cEntity_Base in distinctAllDeckCards)
        {
            string lineString = "";

            int count = AllDeckCards.Count((cEntity_Base1) => cEntity_Base.CardSpriteName == cEntity_Base1.CardSpriteName);

            if (count >= 1)
            {
                lineString += $"{count} {cEntity_Base.CardName_ENG}   {cEntity_Base.CardSpriteName} \n";
            }

            DeckBuilderDeckCode += lineString;
        }

        DeckBuilderDeckCode = DataBase.ReplaceToASCII(DeckBuilderDeckCode);

        return DeckBuilderDeckCode;
    }

    public static List<CEntity_Base> GetAllDeckCardsFromTTSDeckCode(string TTSDeckCode)
    {
        List<CEntity_Base> AllDeckCards = new List<CEntity_Base>();

        if (!string.IsNullOrEmpty(TTSDeckCode))
        {
            TTSDeckCode = TTSDeckCode.Replace("[", "").Replace("]", "").Replace("\"", "");

            string[] parseByComma = TTSDeckCode.Split(',');

            for (int i = 0; i < parseByComma.Length; i++)
            {
                // if (i != 0)
                {
                    CEntity_Base cEntity_Base = GetCardFromCardID(parseByComma[i]);

                    if (cEntity_Base == null)
                    {
                        cEntity_Base = GetCardFromSpriteName(parseByComma[i]);
                    }

                    if (cEntity_Base != null)
                    {
                        AllDeckCards.Add(cEntity_Base);
                    }
                }
            }
        }

        return AllDeckCards;
    }

    public static List<CEntity_Base> GetAllDeckCardsFromDeckBuilderDeckCode(string DeckBuilderDeckCode, int plusCount = 0)
    {
        int value;

        List<CEntity_Base> AllDeckCards = new List<CEntity_Base>();

        if (!string.IsNullOrEmpty(DeckBuilderDeckCode))
        {
            DeckBuilderDeckCode = DeckBuilderDeckCode.Replace("\r", "\n");

            string[] parseByEnter = DeckBuilderDeckCode.Split('\n');

            for (int i = 0; i < parseByEnter.Length; i++)
            {
                if (!string.IsNullOrEmpty(parseByEnter[i]))
                {
                    if (parseByEnter[i].Length >= 5)
                    {
                        if (parseByEnter[i][0] == '/' && parseByEnter[i][1] == '/')
                        {
                            continue;
                        }

                        int count = 0;

                        string numberString = "";

                        for (int j = 0; j < parseByEnter[i].Length; j++)
                        {
                            if (int.TryParse(parseByEnter[i][j].ToString(), out value))
                            {
                                numberString += parseByEnter[i][j].ToString();
                            }

                            else
                            {
                                break;
                            }
                        }

                        if (int.TryParse(numberString, out value))
                        {
                            count = value + plusCount;
                        }

                        if (count >= 1)
                        {
                            if (parseByEnter[i][numberString.Length].ToString() == " " || parseByEnter[i][numberString.Length].ToString() == "\t")
                            {
                                string cardIDString = "";

                                for (int j = 0; j < parseByEnter[i].Length; j++)
                                {
                                    char targetChar = parseByEnter[i][parseByEnter[i].Length - 1 - j];

                                    if (targetChar.ToString() == " " || targetChar.ToString() == "\t")
                                    {
                                        if (!String.IsNullOrEmpty(cardIDString))
                                        {
                                            break;
                                        }
                                    }

                                    else
                                    {
                                        cardIDString += targetChar;
                                    }
                                }

                                cardIDString = new string(cardIDString.Reverse().ToArray());

                                string cardIDStringCopy = cardIDString;

                                for (int j = 0; j < cardIDStringCopy.Length - 1; j++)
                                {
                                    bool result = Regex.IsMatch(cardIDStringCopy[j].ToString(), "[A-Z]");

                                    if (result)
                                    {
                                        break;
                                    }

                                    cardIDString = cardIDStringCopy.Substring(j + 1);
                                }

                                CEntity_Base cEntity_Base = GetCardFromCardID(cardIDString);

                                if (cEntity_Base == null)
                                {
                                    cEntity_Base = GetCardFromSpriteName(cardIDString);
                                }

                                if (cEntity_Base != null)
                                {
                                    for (int k = 0; k < count; k++)
                                    {
                                        AllDeckCards.Add(cEntity_Base);
                                    }
                                }

                                else
                                {
                                    Debug.Log($"cardIDString:{cardIDString}, cardEntity = null");
                                }
                            }
                        }
                    }
                }
            }
        }

        return AllDeckCards;
    }

    static CEntity_Base GetCardFromCardID(string cardID)
    {
        return ContinuousController.instance.CardList.ToList().Find(cEntity_Base => cEntity_Base.CardID == cardID);
    }

    static CEntity_Base GetCardFromSpriteName(string cardSpriteName)
    {
        return ContinuousController.instance.CardList.ToList().Find(cEntity_Base => cEntity_Base.CardSpriteName == cardSpriteName);
    }
}
