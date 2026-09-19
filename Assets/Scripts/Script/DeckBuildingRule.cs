using DCGO.CardEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeckBuildingRule : MonoBehaviour
{
    public static bool IsValidDeck(DeckData deckData)
    {
        if (ContinuousController.instance != null)
        {
            foreach (CEntity_Base cEntity_Base in deckData.DeckCards())
            {
                if (!cEntity_Base.IsStandardValid)
                {
                    return false;
                }
            }
        }

        return true;
    }

    #region インポートしたデッキデータを修正
    public static DeckData ModifiedDeckData(DeckData deckData)
    {
        List<CEntity_Base> modifiedDeck = modifiedList(deckData.AllDeckCards());
        List<CEntity_Base> modifiedDeckCards = new List<CEntity_Base>();
        List<CEntity_Base> modifiedDigitamaDeckCards = new List<CEntity_Base>();
        

        foreach (CEntity_Base cEntity_Base in modifiedDeck)
        {
            if (!cEntity_Base.cardKind.Contains(CardKind.DigiEgg))
            {
                modifiedDeckCards.Add(cEntity_Base);
            }

            else
            {
                modifiedDigitamaDeckCards.Add(cEntity_Base);
            }
        }

        List<CEntity_Base> modifiedList(List<CEntity_Base> cEntity_Bases)
        {
            //カードリストを重複なしのリストにする
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

            //規定枚数以上のカードを抜く
            if (!ContinuousController.instance.useBanlist)
                return deckCards;

            foreach (CEntity_Base cEntity_Base in DistinctDeckCards1)
            {
                foreach (Restrictions restriction in ContinuousController.instance.BanList.Restrictions)
                {
                    if (cEntity_Base.CardID == restriction.id)
                    {
                        while (cEntity_Base.SameCardIDCount(deckCards) > restriction.limit)
                        {
                            CEntity_Base removeCard = deckCards.Find(cEntity_Base1 => cEntity_Base1.CardID == cEntity_Base.CardID);

                            if (removeCard != null)
                            {
                                deckCards.Remove(removeCard);
                            }
                        }
                    }
                }

                foreach (Pair bannedPair in ContinuousController.instance.BanList.BannedPair)
                {
                    if (cEntity_Base.CardID == bannedPair.id)
                    {
                        while (deckCards.Some(cEntity_Base1 => bannedPair.pairs.Contains(cEntity_Base1.CardID)))
                        {
                            CEntity_Base removeCard = deckCards.Find(cEntity_Base1 => bannedPair.pairs.Contains(cEntity_Base1.CardID));

                            if (removeCard != null)
                            {
                                deckCards.Remove(removeCard);
                            }
                        }
                    }

                    if (bannedPair.pairs.Contains(cEntity_Base.CardID))
                    {
                        while (deckCards.Some(cEntity_Base1 => cEntity_Base1.CardID == bannedPair.id))
                        {
                            CEntity_Base removeCard = deckCards.Find(cEntity_Base1 => bannedPair.id.Contains(cEntity_Base1.CardID));

                            if (removeCard != null)
                            {
                                deckCards.Remove(removeCard);
                            }
                        }
                    }
                }
            }

            UnityEngine.Debug.Log($"Modified: {deckCards.Count}");
            return deckCards;
        }

        DeckData deckData1 = new DeckData(DeckData.GetDeckCode(deckData.DeckName, modifiedDeckCards, modifiedDigitamaDeckCards, deckData.KeyCard), deckData.DeckID);

        if (!deckData1.AllDeckCards().Contains(deckData1.KeyCard))
        {
            deckData1.KeyCardId = -1;
        }

        return deckData1;
    }
    #endregion

    public static int MaxCount_BanList(CEntity_Base cEntity_Base)
    {
        int count = cEntity_Base.MaxCountInDeck;

        if (!ContinuousController.instance.useBanlist)
            return count;

        foreach (Restrictions restriction in ContinuousController.instance.BanList.Restrictions)
        {
            if (cEntity_Base.CardID == restriction.id)
            {
                count = restriction.limit;
                break;
            }
        }

        return count;
    }

    public static bool CanAddCard(CEntity_Base cEntity_Base, DeckData deckData)
    {
        if (cEntity_Base.cardKind.Contains(CardKind.DigiEgg))
        {
            if (cEntity_Base.SameCardIDCount(deckData.DigitamaDeckCards()) >= cEntity_Base.MaxCountInDeck)
            {
                return false;
            }

            if (deckData.DigitamaDeckCards().Count >= 5)
            {
                return false;
            }
        }

        else
        {
            if (cEntity_Base.SameCardIDCount(deckData.DeckCards()) >= cEntity_Base.MaxCountInDeck)
            {
                return false;
            }
        }

        if (!ContinuousController.instance.useBanlist)
            return true;

        foreach (Restrictions restriction in ContinuousController.instance.BanList.Restrictions)
        {
            if (cEntity_Base.CardID == restriction.id)
            {
                if (cEntity_Base.SameCardIDCount(deckData.AllDeckCards()) >= restriction.limit)
                {
                    return false;
                }
            }
        }

        foreach (Pair bannedPair in ContinuousController.instance.BanList.BannedPair)
        {
            if (cEntity_Base.CardID == bannedPair.id)
            {
                if (deckData.AllDeckCards().Some(cEntity_Base1 => bannedPair.pairs.Contains(cEntity_Base1.CardID)))
                {
                    return false;
                }
            }

            if (bannedPair.pairs.Contains(cEntity_Base.CardID))
            {
                if (deckData.AllDeckCards().Some(cEntity_Base1 => cEntity_Base1.CardID == bannedPair.id))
                {
                    return false;
                }
            }
        }

        return true;
    }
}

#region Banlist
[Serializable]
public class BanList
{
    public List<Pair> BannedPair;
    public List<Restrictions> Restrictions;

    public BanList()
    {
        BannedPair = new List<Pair>();
        Restrictions = new List<Restrictions>();
    }

    

    public CardRestriction ConvertToCardRestriction()
    {
        List<CardLimitCount> CardLimit = new List<CardLimitCount>();
        List<BannedPair> BannedPairs = new List<BannedPair>();

        foreach (Pair pair in BannedPair)
            BannedPairs.Add(new BannedPair(pair.id, pair.pairs));

        foreach (Restrictions restrict in Restrictions)
            CardLimit.Add(new CardLimitCount(restrict.id, restrict.limit));

        return new CardRestriction(CardLimit, BannedPairs);
    }
}

[Serializable]
public class Pair
{
    public string id;
    public List<string> pairs;
}

[Serializable]
public class Restrictions
{
    public string id;
    public int limit;
}
#endregion

public class CardRestriction
{
    public CardRestriction(List<CardLimitCount> cardLimitCounts, List<BannedPair> bannedPairs)
    {
        CardLimitCounts = cardLimitCounts.Clone();
        BannedPairs = bannedPairs.Clone();
    }

    public List<CardLimitCount> CardLimitCounts { get; private set; } = new List<CardLimitCount>();
    public List<BannedPair> BannedPairs { get; private set; } = new List<BannedPair>();
}

public class CardLimitCount
{
    public CardLimitCount(string cardID, int limitCount)
    {
        CardID = cardID;
        LimitCount = limitCount;
    }

    public string CardID { get; private set; } = "";
    public int LimitCount { get; private set; } = 4;
}

public class BannedPair
{
    public BannedPair(string cardID_A, List<string> cardIDs_B)
    {
        CardID_A = cardID_A;
        CardIDs_B = cardIDs_B.Clone();
    }

    public string CardID_A { get; private set; } = "";
    public List<string> CardIDs_B { get; private set; } = new List<string>();
}