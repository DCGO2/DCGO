using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

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
            Debug.Log($"DeckCode\n{DeckBuilderDeckCode}");

            using (StringReader reader = new StringReader(DeckBuilderDeckCode))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (String.IsNullOrEmpty(line))
                        continue;

                    if (char.IsDigit(line[0]))
                    {
                        int count = 0;
                        int spaceIndex = line.IndexOf(" ");

                        if (int.TryParse(line.Substring(0, spaceIndex).Trim(), out value))
                            count = value + plusCount;

                        for (int i = 0; i < 4; i++)
                        {
                            line = line.TrimEnd();

                            int lastSpaceIndex = line.LastIndexOf(" ");

                            string cardID = line.Substring(lastSpaceIndex + 1);

                            CEntity_Base cEntity_Base = GetCardFromCardID(cardID);

                            if (cEntity_Base != null)
                            {
                                for (int k = 0; k < count; k++)
                                    AllDeckCards.Add(cEntity_Base);

                                Debug.Log($"SUCCESSFULLY ADDED: {count}: {cardID}");
                                break;
                            }
                            else
                            {
                                line += "/" + reader.ReadLine();
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
        CEntity_Base card = null;
        card = ContinuousController.instance.CardList.ToList().Find(cEntity_Base => cEntity_Base.CardID == cardID);

        if (card == null)
            return ContinuousController.instance.CardList.ToList().Find(cEntity_Base => cEntity_Base.CardSpriteName == cardID);

        return card;
    }
}
