using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class CheckDeckCodeParse : MonoBehaviour
{
    static CEntity_Base GetCardFromCardID(string cardID)
    {
        CEntity_Base card = null;
        card = ContinuousController.instance.CardList.ToList().Find(cEntity_Base => cEntity_Base.CardID == cardID);

        if(card == null)
            return ContinuousController.instance.CardList.ToList().Find(cEntity_Base => cEntity_Base.CardSpriteName == cardID);

        return card;
    }

    static string GetCardID(string line)
    {
        line = line.TrimEnd();

        int lastSpaceIndex = line.LastIndexOf(" ");

        string cardID = line.Substring(lastSpaceIndex + 1);

        return cardID;
    }

    [MenuItem("Window/DCGO/CheckDeckCodeParse")]

    static void CheckDeckParse()
    {
        string deckCode = GUIUtility.systemCopyBuffer;

        Debug.Log($"DeckCode\n{deckCode}");

        int value;

        List<CEntity_Base> AllDeckCards = new List<CEntity_Base>();

        if (!string.IsNullOrEmpty(deckCode))
        {
            Debug.Log($"DeckCode\n{deckCode}");

            using (StringReader reader = new StringReader(deckCode))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (String.IsNullOrEmpty(line))
                        continue;

                    if(char.IsDigit(line[0]))
                    {
                        int count = 0;

                        if (int.TryParse(line[0].ToString(), out value))
                            count = value;

                        for (int i = 0; i < 4; i++)
                        {
                            Debug.Log($"Found Card Count: {line}");

                            string cardID = GetCardID(line);

                            Debug.Log($"Identified ID: {cardID}");
                            CEntity_Base cEntity_Base = GetCardFromCardID(cardID);

                            if (cEntity_Base != null)
                            {
                                Debug.Log($"SUCCESSFULLY ADDED: {count}: {cardID}");
                                break;
                            }
                            else
                            {
                                line += "/" + reader.ReadLine();
                                Debug.Log($"cardIDString:{cardID}, cardEntity = null");
                            }
                        }
                        
                    }
                }
            }
        }
    }
}
