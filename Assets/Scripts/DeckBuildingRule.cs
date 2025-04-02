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
            if (cEntity_Base.cardKind != CardKind.DigiEgg)
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
            foreach (CEntity_Base cEntity_Base in DistinctDeckCards1)
            {
                foreach (CardLimitCount cardLimitCount in ContinuousController.instance.BanList.CardLimitCounts)
                {
                    if (cEntity_Base.CardID == cardLimitCount.CardID)
                    {
                        while (cEntity_Base.SameCardIDCount(deckCards) > cardLimitCount.LimitCount)
                        {
                            CEntity_Base removeCard = deckCards.Find(cEntity_Base1 => cEntity_Base1.CardID == cEntity_Base.CardID);

                            if (removeCard != null)
                            {
                                deckCards.Remove(removeCard);
                            }
                        }
                    }
                }

                foreach (BannedPair bannedPair in ContinuousController.instance.BanList.BannedPairs)
                {
                    if (cEntity_Base.CardID == bannedPair.CardID_A)
                    {
                        UnityEngine.Debug.Log($"Banned Pair Found: {deckCards.Some(cEntity_Base1 => bannedPair.CardIDs_B.Contains(cEntity_Base1.CardID))}");
                        while (deckCards.Some(cEntity_Base1 => bannedPair.CardIDs_B.Contains(cEntity_Base1.CardID)))
                        {
                            CEntity_Base removeCard = deckCards.Find(cEntity_Base1 => bannedPair.CardIDs_B.Contains(cEntity_Base1.CardID));
                            UnityEngine.Debug.Log($"Banned Pair: {removeCard.CardID}");
                            if (removeCard != null)
                            {
                                deckCards.Remove(removeCard);
                            }
                        }
                    }

                    if (bannedPair.CardIDs_B.Contains(cEntity_Base.CardID))
                    {
                        while (deckCards.Some(cEntity_Base1 => cEntity_Base1.CardID == bannedPair.CardID_A))
                        {
                            CEntity_Base removeCard = deckCards.Find(cEntity_Base1 => bannedPair.CardID_A.Contains(cEntity_Base1.CardID));
                            UnityEngine.Debug.Log($"Banned Pair: {removeCard.CardID}");
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

        foreach (CardLimitCount cardLimitCount in ContinuousController.instance.BanList.CardLimitCounts)
        {
            if (cEntity_Base.CardID == cardLimitCount.CardID)
            {
                count = cardLimitCount.LimitCount;
                break;
            }
        }

        return count;
    }

    public static bool CanAddCard(CEntity_Base cEntity_Base, DeckData deckData)
    {
        if (cEntity_Base.cardKind == CardKind.DigiEgg)
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

        foreach (CardLimitCount cardLimitCount in ContinuousController.instance.BanList.CardLimitCounts)
        {
            if (cEntity_Base.CardID == cardLimitCount.CardID)
            {
                if (cEntity_Base.SameCardIDCount(deckData.AllDeckCards()) >= cardLimitCount.LimitCount)
                {
                    return false;
                }
            }
        }

        foreach (BannedPair bannedPair in ContinuousController.instance.BanList.BannedPairs)
        {
            if (cEntity_Base.CardID == bannedPair.CardID_A)
            {
                if (deckData.AllDeckCards().Some(cEntity_Base1 => bannedPair.CardIDs_B.Contains(cEntity_Base1.CardID)))
                {
                    return false;
                }
            }

            if (bannedPair.CardIDs_B.Contains(cEntity_Base.CardID))
            {
                if (deckData.AllDeckCards().Some(cEntity_Base1 => cEntity_Base1.CardID == bannedPair.CardID_A))
                {
                    return false;
                }
            }
        }

        return true;
    }
}

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