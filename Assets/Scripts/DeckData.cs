using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[System.Serializable]
public class DeckData
{
    public static int m = 256;
    public static int n = 256;
    public static int log_n_m = (int)Mathf.Log(m, n);
    public static int maxCardKind = 37000;

    public static int CardKindCellLength = (int)Math.Ceiling(Mathf.Log(maxCardKind, m));

    #region  constructor
    public DeckData(string DeckCode, string ID = "")
    {
        List<int> _DeckCardIDs = new List<int>();
        List<int> _DigitamaDeckCardIDs = new List<int>();
        //separated by commas
        string[] parseByComma = DeckCode.Split(',');

        List<int> DistinctDeckCardIDs = new List<int>();
        List<int> DistinctDeckCardCounts = new List<int>();

        List<int> DistinctDigitamaDeckCardIDs = new List<int>();
        List<int> DistinctDigitamaDeckCardCounts = new List<int>();

        DeckID = ID;

        for (int i = 0; i < parseByComma.Length; i++)
        {
            //deck name
            if (i == 0)
            {
                DeckName = parseByComma[i];
            }

            //Cards in the deck(no duplicates)
            else if (i == 1)
            {
                //Separate every 2 characters
                string[] SplitText = SplitClass.Split(parseByComma[i], CardKindCellLength);

                for (int j = 0; j < SplitText.Length; j++)
                {
                    DistinctDeckCardIDs.Add(ConvertBinaryNumber.NStringToInt(SplitText[j], m));
                }
            }

            //Number of each type of card
            else if (i == 2)
            {
                //m-ary string
                string x_m = parseByComma[i];

                if (!string.IsNullOrEmpty(x_m))
                {
                    //Convert m-ary number to n-ary number
                    string x_n = ConvertBinaryNumber.NKStringToNString(x_m, n, log_n_m);

                    //Separate n-ary string by character
                    string[] Split_x_n = SplitClass.Split(x_n, 1);

                    for (int j = 0; j < Split_x_n.Length; j++)
                    {
                        //Convert n-ary number to int
                        DistinctDeckCardCounts.Add(ConvertBinaryNumber.NStringToInt(Split_x_n[j], n) + 1);
                    }
                }
            }

            //Digitama deck cards (no duplicates)
            else if (i == 3)
            {
                //Separate every 2 characters
                string[] SplitText = SplitClass.Split(parseByComma[i], CardKindCellLength);

                for (int j = 0; j < SplitText.Length; j++)
                {
                    DistinctDigitamaDeckCardIDs.Add(ConvertBinaryNumber.NStringToInt(SplitText[j], m));
                }
            }

            //Number of each type of card
            else if (i == 4)
            {
                //m-ary string
                string x_m = parseByComma[i];

                if (!string.IsNullOrEmpty(x_m))
                {
                    //Convert m-ary number to n-ary number
                    string x_n = ConvertBinaryNumber.NKStringToNString(x_m, n, log_n_m);

                    //Separate n-ary string by character
                    string[] Split_x_n = SplitClass.Split(x_n, 1);

                    for (int j = 0; j < Split_x_n.Length; j++)
                    {
                        //Convert n-ary number to int
                        DistinctDigitamaDeckCardCounts.Add(ConvertBinaryNumber.NStringToInt(Split_x_n[j], n) + 1);
                    }
                }
            }

            //key card id
            else if (i == 5)
            {
                if (int.TryParse(parseByComma[i], out int value))
                {
                    KeyCardId = value;
                }
            }
        }

        for (int i = 0; i < DistinctDeckCardIDs.Count; i++)
        {
            if (i < DistinctDeckCardCounts.Count)
            {
                for (int j = 0; j < DistinctDeckCardCounts[i]; j++)
                {
                    _DeckCardIDs.Add(DistinctDeckCardIDs[i]);
                }
            }
        }

        for (int i = 0; i < DistinctDigitamaDeckCardIDs.Count; i++)
        {
            if (i < DistinctDigitamaDeckCardCounts.Count)
            {
                for (int j = 0; j < DistinctDigitamaDeckCardCounts[i]; j++)
                {
                    _DigitamaDeckCardIDs.Add(DistinctDigitamaDeckCardIDs[i]);
                }
            }
        }

        DeckCardIDs = _DeckCardIDs;
        DigitamaDeckCardIDs = _DigitamaDeckCardIDs;
    }
    #endregion

    #region sort value
    int _sortValue = 0;
    public int SortValue
    {
        get
        {
            return _sortValue;
        }

        set
        {
            _sortValue = value;
        }
    }
    #endregion

    #region deck id
    string _deckID = "";
    public string DeckID
    {
        get
        {
            if (string.IsNullOrEmpty(_deckID))
            {
                StringBuilder builder = new StringBuilder();
                Enumerable
                   .Range(65, 26)
                    .Select(e => ((char)e).ToString())
                    .Concat(Enumerable.Range(97, 26).Select(e => ((char)e).ToString()))
                    .Concat(Enumerable.Range(0, 10).Select(e => e.ToString()))
                    .OrderBy(e => Guid.NewGuid())
                    .Take(11)
                    .ToList().ForEach(e => builder.Append(e));

                _deckID = builder.ToString();
            }

            return _deckID;
        }

        set
        {
            _deckID = value;
        }
    }
    #endregion

    #region deck name
    string _deckName = "";
    public string DeckName
    {
        get
        {
            if (string.IsNullOrEmpty(_deckName))
            {
                return "NewDeck";
            }

            return _deckName;
        }

        set
        {
            _deckName = value;
        }
    }
    #endregion

    #region List of cards included in the deck
    public List<CEntity_Base> DeckCards()
    {
        List<CEntity_Base> deckCards = new List<CEntity_Base>();

        if (DeckCardIDs != null)
        {
            foreach (int DeckCardID in DeckCardIDs)
            {
                CEntity_Base cEntity_Base = ContinuousController.instance.getCardEntityByCardID(DeckCardID);

                if (cEntity_Base != null)
                {
                    deckCards.Add(cEntity_Base);
                }
            }
        }

        return deckCards;
    }
    #endregion

    #region List of cards included in the Digitama deck
    public List<CEntity_Base> DigitamaDeckCards()
    {
        List<CEntity_Base> deckCards = new List<CEntity_Base>();

        foreach (int DeckCardID in DigitamaDeckCardIDs)
        {
            CEntity_Base cEntity_Base = ContinuousController.instance.getCardEntityByCardID(DeckCardID);

            if (cEntity_Base != null)
            {
                deckCards.Add(cEntity_Base);
            }
        }

        return deckCards;
    }
    #endregion

    #region all cards in the deck
    public List<CEntity_Base> AllDeckCards()
    {
        List<CEntity_Base> AllDeckCards = new List<CEntity_Base>();

        foreach (CEntity_Base cEntity_Base in DigitamaDeckCards())
        {
            AllDeckCards.Add(cEntity_Base);
        }

        foreach (CEntity_Base cEntity_Base in DeckCards())
        {
            AllDeckCards.Add(cEntity_Base);
        }

        return AllDeckCards;
    }
    #endregion

    #region key card
    public int KeyCardId { get; set; } = -1;

    public CEntity_Base KeyCard
    {
        get
        {
            if (KeyCardId >= 0)
            {
                foreach (CEntity_Base Card in ContinuousController.instance.CardList)
                {
                    if (Card.CardIndex == KeyCardId)
                    {
                        return Card;
                    }
                }
            }

            if (DeckCards().Count >= 1)
            {
                List<CEntity_Base> deckCards = new List<CEntity_Base>();

                foreach (CEntity_Base cEntity_Base in DeckCards())
                {
                    deckCards.Add(cEntity_Base);
                }

                deckCards = deckCards
                    .OrderBy(value => Array.IndexOf(DataBase.cardKinds, value.cardKind))
            .ThenByDescending(value => value.Level)
            .ThenBy(value => Array.IndexOf(DataBase.cardColors, value.cardColors[0]))
            .ThenByDescending(value => value.PlayCost)
            .ThenByDescending(value => value.DP)
            .ThenBy(value => value.CardIndex)
            .ToList();

                return deckCards[0];
            }

            else if (DigitamaDeckCards().Count >= 1)
            {
                return DigitamaDeckCards()[0];
            }

            return null;
        }
    }
    #endregion

    #region List of card IDs included in the deck
    public List<int> DeckCardIDs { get; set; } = new List<int>();
    public List<int> DigitamaDeckCardIDs { get; set; } = new List<int>();

    #region カードを追加する
    public void AddCard(CEntity_Base cEntity_Base)
    {
        if (cEntity_Base.cardKind == CardKind.DigiEgg)
        {
            AddDigitamaDeckCard(cEntity_Base);
        }

        else
        {
            AddDeckCard(cEntity_Base);
        }
    }
    #endregion

    #region カードを除く
    public void RemoveCard(CEntity_Base cEntity_Base)
    {
        if (cEntity_Base.cardKind == CardKind.DigiEgg)
        {
            RemoveDigitamaDeckCard(cEntity_Base);
        }

        else
        {
            RemoveDeckCard(cEntity_Base);
        }
    }
    #endregion

    #region デッキにカードを追加する
    void AddDeckCard(CEntity_Base cEntity_Base)
    {
        List<CEntity_Base> _DeckCards = DeckCards();

        _DeckCards.Add(cEntity_Base);
        _DeckCards = SortedDeckCardsList(_DeckCards);

        DeckCardIDs = GetDeckCardCodes(_DeckCards);
    }
    #endregion

    #region デッキからカードを抜く
    void RemoveDeckCard(CEntity_Base cEntity_Base)
    {
        List<CEntity_Base> _DeckCards = DeckCards();

        _DeckCards.Remove(cEntity_Base);

        _DeckCards = SortedDeckCardsList(_DeckCards);

        DeckCardIDs = GetDeckCardCodes(_DeckCards);
    }
    #endregion

    #region デジタマデッキにカードを追加する
    void AddDigitamaDeckCard(CEntity_Base cEntity_Base)
    {
        List<CEntity_Base> _DeckCards = DigitamaDeckCards();

        _DeckCards.Add(cEntity_Base);

        _DeckCards = SortedDeckCardsList(_DeckCards);

        DigitamaDeckCardIDs = GetDeckCardCodes(_DeckCards);
    }
    #endregion

    #region デジタマデッキからカードを抜く
    void RemoveDigitamaDeckCard(CEntity_Base cEntity_Base)
    {
        List<CEntity_Base> _DeckCards = DigitamaDeckCards();

        _DeckCards.Remove(cEntity_Base);

        _DeckCards = SortedDeckCardsList(_DeckCards);

        DigitamaDeckCardIDs = GetDeckCardCodes(_DeckCards);
    }
    #endregion

    #region カードリストからカードIDリストを取得
    public static List<int> GetDeckCardCodes(List<CEntity_Base> DeckCards)
    {
        List<int> _DeckCardCodes = new List<int>();

        foreach (CEntity_Base DeckCard in DeckCards)
        {
            _DeckCardCodes.Add(DeckCard.CardIndex);
        }

        return _DeckCardCodes;
    }
    #endregion

    #endregion

    #region Sort card list

    public static List<CEntity_Base> SortedDeckCardsList(List<CEntity_Base> DeckCards)
    {
        List<CEntity_Base> DeckCards1 = new List<CEntity_Base>();

        foreach (CEntity_Base cEntity_Base in DeckCards)
        {
            DeckCards1.Add(cEntity_Base);
        }

        DeckCards1 = DeckCards1
            .OrderBy(value => Array.IndexOf(DataBase.cardKinds, value.cardKind))
            .ThenBy(value => value.Level)
            .ThenBy(value => Array.IndexOf(DataBase.cardColors, value.cardColors[0]))
            .ThenByDescending(value => value.PlayCost)
            .ThenByDescending(value => value.DP)
            .ThenBy(value => value.CardIndex)
            .ToList();

        return DeckCards1;
    }

    public static List<CEntity_Base> SortedCardPoolList(List<CEntity_Base> DeckCards)
    {
        List<CEntity_Base> DeckCards1 = new List<CEntity_Base>();

        foreach (CEntity_Base cEntity_Base in DeckCards)
        {
            DeckCards1.Add(cEntity_Base);
        }

        DeckCards1 = DeckCards1
            .OrderBy(value => Array.IndexOf(DataBase.SetIDs, value.SetID))
            .ThenBy(value => Array.IndexOf(DataBase.cardColors, value.cardColors[0]))
            .ThenBy(value => value.CardID)
            .ThenBy(value => value.CardIndex)
            .ToList();

        return DeckCards1;
    }

    public static List<CardSource> SortedCardsList(List<CardSource> DeckCards)
    {
        List<CardSource> DeckCards1 = new List<CardSource>();

        foreach (CardSource cardSource in DeckCards)
        {
            DeckCards1.Add(cardSource);
        }

        DeckCards1 = DeckCards1
            .OrderBy(value => value.SetID)
            .ThenBy(value => Array.IndexOf(DataBase.cardKinds, value.CardKind))
            .ThenBy(value => Array.IndexOf(DataBase.cardColors, value.BaseCardColorsFromEntity[0]))
            .ThenByDescending(value => value.BaseEvoCostsFromEntity.Count)
            .ThenByDescending(value => value.CardDP)
            .ThenBy(value => value.CardEntityIndex)
            .ToList();

        return DeckCards1;
    }
    #endregion

    #region Get 256 hexadecimal deck code
    #region Get the 256-decimal deck code from the deck name and deck card.
    public static string GetDeckCode(string _DeckName, List<CEntity_Base> _DeckCards, List<CEntity_Base> _DigitamaDeckCards, CEntity_Base keyCard)
    {
        string _DeckDataString = null;

        //deck name
        _DeckDataString += _DeckName + ",";

        SetDeckCard(_DeckCards);
        SetDeckCard(_DigitamaDeckCards);

        if (keyCard != null)
        {
            _DeckDataString += $"{keyCard.CardIndex},";
        }

        else
        {
            _DeckDataString += $"-1,";
        }

        //Debug.Log($"raw deck cord:{_DeckDataString}");

        void SetDeckCard(List<CEntity_Base> cEntity_Bases)
        {
            //Make the card list a non-duplicate list
            List<CEntity_Base> DistinctDeckCards = cEntity_Bases.Distinct().ToList();

            //Register a card ID list without duplicates
            foreach (CEntity_Base cardData in DistinctDeckCards)
            {
                _DeckDataString += cardData.CardIndex_String;
            }

            _DeckDataString += ",";

            //Save the number of cards of each card type (reduce by 1)
            List<int> _DistinctDeckCardCounts = new List<int>();

            foreach (CEntity_Base cardData in DistinctDeckCards)
            {
                _DistinctDeckCardCounts.Add(cEntity_Bases.Count((_CardData) => _CardData == cardData) - 1);
            }

            string x_n = null;

            //Convert the number of cards in the deck to n-ary number

            for (int i = 0; i < _DistinctDeckCardCounts.Count; i++)
            {
                x_n += ConvertBinaryNumber.IntToNString(_DistinctDeckCardCounts[i], n);
            }

            //Fill with 0s so that the number of digits is log(n)m digits
            if (x_n != null)
            {
                while (x_n.Count() % log_n_m != 0)
                {
                    x_n += "0";
                }
            }

            //Convert n-ary number to m-ary number
            if (x_n != null)
            {
                string x_m = ConvertBinaryNumber.NStringToNKString(x_n, n, log_n_m);
                _DeckDataString += x_m;
            }

            _DeckDataString += ",";
        }

        return _DeckDataString;
    }
    #endregion

    #region Get the deck code for this deck
    public string GetThisDeckCode()
    {
        return DeckData.GetDeckCode(DeckName, DeckCards(), DigitamaDeckCards(), KeyCard);
    }
    #endregion
    #endregion

    #region Is the string suitable as a deck code?
    public static bool IsValidDeckCode(string DeckCode)
    {
        if (!DeckCode.Contains(","))
        {
            return false;
        }

        string[] parseByComma = DeckCode.Split(',');

        if (parseByComma.Length != 5 && parseByComma.Length != 6)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(parseByComma[1]))
        {
            string[] SplitText = SplitClass.Split(parseByComma[1], 1);

            for (int j = 0; j < SplitText.Length; j++)
            {
                if (ConvertBinaryNumber.NStringToInt(SplitText[j], m) == 114514)
                {
                    return false;
                }
            }
        }

        else
        {
            return false;
        }

        if (!string.IsNullOrEmpty(parseByComma[2]))
        {
            //256進数の文字列
            string x_m = parseByComma[2];

            //256進数をn進数に変換
            string x_n = ConvertBinaryNumber.NKStringToNString(x_m, n, log_n_m);

            if (x_n == "114514")
            {
                Debug.Log("[2]:114514");
                return false;
            }
        }

        else
        {
            return false;
        }

        if (!string.IsNullOrEmpty(parseByComma[3]))
        {
            string[] SplitText = SplitClass.Split(parseByComma[3], 1);

            for (int j = 0; j < SplitText.Length; j++)
            {
                if (ConvertBinaryNumber.NStringToInt(SplitText[j], m) == 114514)
                {
                    return false;
                }
            }
        }

        if (!string.IsNullOrEmpty(parseByComma[4]))
        {
            //256進数の文字列
            string x_m = parseByComma[4];

            //256進数をn進数に変換
            string x_n = ConvertBinaryNumber.NKStringToNString(x_m, n, log_n_m);

            if (x_n == "114514")
            {
                Debug.Log("[4]:114514");
                return false;
            }
        }

        if (parseByComma.Length >= 6)
        {
            if (!string.IsNullOrEmpty(parseByComma[5]))
            {
                if (!int.TryParse(parseByComma[5], out int value))
                {
                    return false;
                }
            }
        }

        return true;
    }
    #endregion

    #region Can this deck data be used in battle?
    public bool IsValidDeckData()
    {
        //The number of cards in the deck is exactly 50.
        if (DeckCards().Count != 50)
        {
            return false;
        }

        if (DigitamaDeckCards().Count > 5)
        {
            return false;
        }

        foreach (CEntity_Base cEntity_Base in DeckCards())
        {
            if (!cEntity_Base.IsStandardValid)
            {
                return false;
            }
        }

        foreach (CEntity_Base cEntity_Base in DigitamaDeckCards())
        {
            if (!cEntity_Base.IsStandardValid)
            {
                return false;
            }
        }

        return true;
    }
    #endregion

    #region blank deck code
    public static DeckData EmptyDeckData()
    {
        return new DeckData("");
    }
    #endregion

    #region Correct imported deck data
    public DeckData ModifiedDeckData()
    {
        List<CEntity_Base> deckCards = new List<CEntity_Base>();
        List<CEntity_Base> digitamaDeckCards = new List<CEntity_Base>();

        foreach (CEntity_Base cEntity_Base in AllDeckCards())
        {
            if (cEntity_Base.cardKind != CardKind.DigiEgg)
            {
                deckCards.Add(cEntity_Base);
            }

            else
            {
                digitamaDeckCards.Add(cEntity_Base);
            }
        }

        List<CEntity_Base> modifiedDeckCards = modifiedList(deckCards);
        List<CEntity_Base> modifiedDigitamaDeckCards = modifiedList(digitamaDeckCards);

        List<CEntity_Base> modifiedList(List<CEntity_Base> cEntity_Bases)
        {
            //Make the card list a non-duplicate list
            List<CEntity_Base> DistinctDeckCards = cEntity_Bases.Distinct().ToList();

            List<CEntity_Base> DistinctDeckCards1 = new List<CEntity_Base>();

            foreach (CEntity_Base cEntity_Base in DistinctDeckCards)
            {
                if (DistinctDeckCards1.Count((cEntity_Base1) => cEntity_Base.CardID == cEntity_Base1.CardID) == 0)
                {
                    DistinctDeckCards1.Add(cEntity_Base);
                }
            }

            List<CEntity_Base> deckCards = new List<CEntity_Base>();

            foreach (CEntity_Base cEntity_Base in cEntity_Bases)
            {
                deckCards.Add(cEntity_Base);
            }

            //Remove more than the specified number of cards
            foreach (CEntity_Base cEntity_Base in DistinctDeckCards1)
            {
                while (cEntity_Base.SameCardIDCount(deckCards) > cEntity_Base.MaxCountInDeck)
                {
                    CEntity_Base removeCard = null;

                    foreach (CEntity_Base cEntity_Base1 in deckCards)
                    {
                        if (cEntity_Base1.CardID == cEntity_Base.CardID)
                        {
                            removeCard = cEntity_Base1;
                            break;
                        }
                    }

                    if (removeCard != null)
                    {
                        deckCards.Remove(removeCard);
                    }
                }
            }

            return deckCards;
        }

        DeckData deckData = new DeckData(GetDeckCode(this._deckName, modifiedDeckCards, modifiedDigitamaDeckCards, KeyCard), DeckID);

        if (!deckData.AllDeckCards().Contains(deckData.KeyCard))
        {
            deckData.KeyCardId = -1;
        }

        DeckData deckData1 = DeckBuildingRule.ModifiedDeckData(deckData);
        return deckData1;
    }
    #endregion

    #region Fixed deck name
    public static string ValidateDeckName(string deckName)
    {
        if (String.IsNullOrEmpty(deckName))
        {
            return deckName;
        }

        StringBuilder sb = new StringBuilder();
        foreach (char c in deckName)
        {
            if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c == ' '))
            {
                sb.Append(c);
            }
        }

        deckName = sb.ToString();

        var filter = new ProfanityFilter.ProfanityFilter();
        deckName = filter.CensorString(deckName);

        return deckName;
    }

    #endregion
}

#region split string
public static class SplitClass
{
    public static string[] Split(this string str, int count)
    {
        var list = new List<string>();
        int length = (int)Math.Ceiling((double)str.Length / count);

        for (int i = 0; i < length; i++)
        {
            int start = count * i;
            if (str.Length <= start)
            {
                break;
            }
            if (str.Length < start + count)
            {
                list.Add(str.Substring(start));
            }
            else
            {
                list.Add(str.Substring(start, count));
            }
        }

        return list.ToArray();
    }
}
#endregion

