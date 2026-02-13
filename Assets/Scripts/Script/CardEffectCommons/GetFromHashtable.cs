using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Get CardEffect from hashtable
    public static ICardEffect GetCardEffectFromHashtable(Hashtable hashtable)
    {
        if (hashtable != null)
        {
            if (hashtable.ContainsKey("CardEffect"))
            {
                if (hashtable["CardEffect"] is ICardEffect)
                {
                    ICardEffect CardEffect = (ICardEffect)hashtable["CardEffect"];

                    return CardEffect;
                }
            }
        }

        return null;
    }
    #endregion

    #region Get SkillInfo from hashtable
    public static List<SkillInfo> GetSkillFromHashtable(Hashtable hashtable)
    {
        if (hashtable != null)
        {
            if (hashtable.ContainsKey("SkillInfo"))
            {
                if (hashtable["SkillInfo"] is List<SkillInfo>)
                {
                    List<SkillInfo> skillInfo = (List<SkillInfo>)hashtable["SkillInfo"];

                    return skillInfo;
                }
            }
        }

        return null;
    }
    #endregion

    #region Get Root from hashtable
    public static SelectCardEffect.Root GetRootFromHashtable(Hashtable hashtable)
    {
        if (hashtable != null)
        {
            if (hashtable.ContainsKey("Root"))
            {
                if (hashtable["Root"] is SelectCardEffect.Root)
                {
                    SelectCardEffect.Root Root = (SelectCardEffect.Root)hashtable["Root"];

                    return Root;
                }
            }
        }

        return SelectCardEffect.Root.None;
    }
    #endregion

    #region Get IsAttack from hashtable
    public static bool IsAttack(Hashtable hashtable)
    {
        if (hashtable != null)
        {
            if (hashtable.ContainsKey("IsAttack"))
            {
                if (hashtable["IsAttack"] is bool)
                {
                    return (bool)hashtable["IsAttack"];
                }
            }
        }

        return false;
    }
    #endregion

    #region Get IsBlock from hashtable
    public static bool IsBlock(Hashtable hashtable)
    {
        if (hashtable != null)
        {
            if (hashtable.ContainsKey("IsBlock"))
            {
                if (hashtable["IsBlock"] is bool)
                {
                    return (bool)hashtable["IsBlock"];
                }
            }
        }

        return false;
    }
    #endregion

    #region Get IsBurst from hashtable
    public static bool IsBurst(Hashtable hashtable)
    {
        if (hashtable != null)
        {
            if (hashtable.ContainsKey("IsBurst"))
            {
                if (hashtable["IsBurst"] is bool)
                {
                    return (bool)hashtable["IsBurst"];
                }
            }
        }

        return false;
    }
    #endregion

    #region Get IsFromSameDigimon from hashtable
    public static bool IsFromSameDigimon(Hashtable hashtable)
    {
        if (hashtable != null)
        {
            if (hashtable.ContainsKey("isFromSameDigimon"))
            {
                if (hashtable["isFromSameDigimon"] is bool)
                {
                    return (bool)hashtable["isFromSameDigimon"];
                }
            }
        }

        return false;
    }
    #endregion

    #region Get IsFromDigimon from hashtable
    public static bool IsFromDigimon(Hashtable hashtable)
    {
        if (hashtable != null)
        {
            if (hashtable.ContainsKey("isFromDigimon"))
            {
                if (hashtable["isFromDigimon"] is bool)
                {
                    return (bool)hashtable["isFromDigimon"];
                }
            }
        }

        return false;
    }
    #endregion

    #region Get IsFromDigimonDigivolutionCards from hashtable
    public static bool IsFromDigimonDigivolutionCards(Hashtable hashtable)
    {
        if (hashtable != null)
        {
            if (hashtable.ContainsKey("isFromDigimonDigivolutionCards"))
            {
                if (hashtable["isFromDigimonDigivolutionCards"] is bool)
                {
                    return (bool)hashtable["isFromDigimonDigivolutionCards"];
                }
            }
        }

        return false;
    }
    #endregion

    #region Get TopCard from hashtable of card effect
    public static CardSource GetTopCardFromEffectHashtable(Hashtable hashtable)
    {
        List<Hashtable> hashtables = GetHashtablesFromHashtable(hashtable);

        if (hashtables != null)
        {
            foreach (Hashtable hashtable1 in hashtables)
            {
                CardSource TopCard = GetTopCardFromOneHashtable(hashtable1);

                if (TopCard != null)
                {
                    return TopCard;
                }
            }
        }

        return null;
    }
    #endregion

    #region Get EvoRootTops from hashtable of card effect
    public static List<CardSource> GetEvoRootTopsFromEnterFieldHashtable(Hashtable hashtable, Func<Permanent, bool> permanentCondition)
    {
        List<Hashtable> hashtables = GetHashtablesFromHashtable(hashtable);

        if (hashtables != null)
        {
            foreach (Hashtable hashtable1 in hashtables)
            {
                Permanent permanent1 = GetPermanentFromHashtable(hashtable1);

                if (permanent1 != null)
                {
                    if (permanentCondition == null || permanentCondition(permanent1))
                    {
                        if (hashtable1 != null)
                        {
                            if (hashtable1.ContainsKey("evoRootTops"))
                            {
                                if (hashtable1["evoRootTops"] is List<CardSource>)
                                {
                                    return (List<CardSource>)hashtable1["evoRootTops"];
                                }
                            }
                        }
                    }
                }
            }
        }

        return null;
    }
    #endregion

    #region Get Permanents from hashtable
    public static List<Permanent> GetPlayedPermanentsFromEnterFieldHashtable(Hashtable hashtable, Func<SelectCardEffect.Root, bool> rootCondition)
    {
        List<Hashtable> hashtables = GetHashtablesFromHashtable(hashtable);

        if (hashtables != null)
        {
            return hashtables
            .Filter(hashtable1 => rootCondition == null || rootCondition(GetRootFromHashtable(hashtable1)))
            .Map(hashtable1 => GetPermanentFromHashtable(hashtable1)).Filter(permanent => permanent != null && permanent.TopCard != null);
        }

        return null;
    }
    #endregion

    #region Get Attacking Permanent from hashtable
    public static Permanent GetAttackerFromHashtable(Hashtable hashtable)
    {
        if (hashtable != null)
        {
            if (hashtable.ContainsKey("AttackingPermanent"))
            {
                if (hashtable["AttackingPermanent"] is Permanent)
                {
                    Permanent permanent = (Permanent)hashtable["AttackingPermanent"];

                    return permanent;
                }
            }
        }

        return null;
    }

    #endregion

    #region Get hashtables from hashtable of card effect
    public static List<Hashtable> GetHashtablesFromHashtable(Hashtable hashtable)
    {
        if (hashtable != null)
        {
            if (hashtable.ContainsKey("hashtables"))
            {
                if (hashtable["hashtables"] is List<Hashtable>)
                {
                    List<Hashtable> hashtables = (List<Hashtable>)hashtable["hashtables"];

                    if (hashtables != null)
                    {
                        return hashtables;
                    }
                }
            }
        }

        return null;
    }

    #endregion

    #region Get TopCard from 1 hashtable
    public static CardSource GetTopCardFromOneHashtable(Hashtable hashtable)
    {
        if (hashtable != null)
        {
            if (hashtable.ContainsKey("TopCard"))
            {
                if (hashtable["TopCard"] is CardSource)
                {
                    CardSource TopCard = (CardSource)hashtable["TopCard"];

                    return TopCard;
                }
            }
        }

        return null;
    }

    #endregion

    #region Get Card from 1 hashtable
    public static CardSource GetCardFromHashtable(Hashtable hashtable)
    {
        if (hashtable != null)
        {
            if (hashtable.ContainsKey("Card"))
            {
                if (hashtable["Card"] is CardSource)
                {
                    CardSource Card = (CardSource)hashtable["Card"];

                    return Card;
                }
            }
        }

        return null;
    }

    #endregion

    #region Get Is Face Up from hashtable
    public static bool GetFaceDownFromHashtable(Hashtable hashtable)
    {
        if (hashtable != null)
        {
            if (hashtable.ContainsKey("isFaceDown"))
            {
                if (hashtable["isFaceDown"] is bool)
                {
                    return (bool)hashtable["isFaceDown"];
                }
            }
        }

        return true;
    }

    #endregion

    #region Get battle info from hashtable
    public static IBattle GetBattleFromHashtable(Hashtable hashtable)
    {
        if (hashtable != null)
        {
            if (hashtable.ContainsKey("battle"))
            {
                if (hashtable["battle"] is IBattle)
                {
                    return (IBattle)hashtable["battle"];
                }
            }
        }

        return null;
    }
    #endregion

    #region Get IsDP0 Delete from 1 hashtable
    public static bool IsDPZeroDelete(Hashtable hashtable)
    {
        if (hashtable != null)
        {
            if (hashtable.ContainsKey("DPZero"))
            {
                if (hashtable["DPZero"] is bool)
                {
                    bool DPZero = (bool)hashtable["DPZero"];

                    if (DPZero)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
    #endregion

    #region Get whehter only 1 permanent is played from hashtable
    public static bool IsOnly1CardPlayed(Hashtable hashtable)
    {
        PlayCardClass playCardClass = GetPlayCardClassFromHashtable(hashtable);

        if (playCardClass != null)
        {
            if (playCardClass.CardSources.Count == 1)
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    #region Get PlayCardClass from hashtable
    public static PlayCardClass GetPlayCardClassFromHashtable(Hashtable hashtable)
    {
        if (hashtable != null)
        {
            if (hashtable.ContainsKey("PlayCardClass"))
            {
                if (hashtable["PlayCardClass"] is PlayCardClass)
                {
                    PlayCardClass IPlayCard = (PlayCardClass)hashtable["PlayCardClass"];

                    if (IPlayCard != null)
                    {
                        return IPlayCard;
                    }
                }
            }
        }

        return null;
    }
    #endregion

    #region Get IsEvolution Delete from 1 hashtable
    public static bool IsEvolution(Hashtable hashtable)
    {
        if (hashtable.ContainsKey("isEvolution"))
        {
            if (hashtable["isEvolution"] is bool)
            {
                bool isEvolution = (bool)hashtable["isEvolution"];

                if (isEvolution)
                {
                    return true;
                }
            }
        }

        return false;
    }
    #endregion

    #region Get Player from hashtable
    public static Player GetPlayerFromHashtable(Hashtable hashtable)
    {
        if (hashtable != null)
        {
            if (hashtable.ContainsKey("Player"))
            {
                if (hashtable["Player"] is Player)
                {
                    Player Player = (Player)hashtable["Player"];

                    return Player;
                }
            }
        }

        return null;
    }
    #endregion

    #region Get Players from hashtable
    public static List<Player> GetPlayersFromHashtable(Hashtable hashtable)
    {
        if (hashtable != null)
        {
            if (hashtable.ContainsKey("Players"))
            {
                if (hashtable["Players"] is List<Player>)
                {
                    List<Player> Players = (List<Player>)hashtable["Players"];

                    if (Players != null)
                    {
                        return Players;
                    }
                }
            }
        }

        return null;
    }
    #endregion

    #region Get Permanents from hashtable
    public static List<Permanent> GetPermanentsFromHashtable(Hashtable hashtable)
    {
        if (hashtable != null)
        {
            if (hashtable.ContainsKey("Permanents"))
            {
                if (hashtable["Permanents"] is List<Permanent>)
                {
                    List<Permanent> Permanents = (List<Permanent>)hashtable["Permanents"];

                    if (Permanents != null)
                    {
                        return Permanents;
                    }
                }
            }
        }

        return null;
    }
    #endregion

    #region Get WinnerPermanents_real from hashtable
    public static List<Permanent> GetWinnerPermanentsRealFromHashtable(Hashtable hashtable)
    {
        if (hashtable != null)
        {
            if (hashtable.ContainsKey("WinnerPermanents_real"))
            {
                if (hashtable["WinnerPermanents_real"] is List<Permanent>)
                {
                    List<Permanent> WinnerPermanents_real = (List<Permanent>)hashtable["WinnerPermanents_real"];

                    if (WinnerPermanents_real != null)
                    {
                        return WinnerPermanents_real;
                    }
                }
            }
        }

        return null;
    }
    #endregion

    #region Get LoserPermanents from hashtable
    public static List<Permanent> GetLoserPermanentsFromHashtable(Hashtable hashtable)
    {
        if (hashtable != null)
        {
            if (hashtable.ContainsKey("LoserPermanents"))
            {
                if (hashtable["LoserPermanents"] is List<Permanent>)
                {
                    List<Permanent> LoserPermanents = (List<Permanent>)hashtable["LoserPermanents"];

                    if (LoserPermanents != null)
                    {
                        return LoserPermanents;
                    }
                }
            }
        }

        return null;
    }
    #endregion

    #region Get Discarded Cards from hashtable
    public static List<CardSource> GetDiscardedCardsFromHashtable(Hashtable hashtable)
    {
        if (hashtable != null)
        {
            if (hashtable.ContainsKey("DiscardedCards"))
            {
                if (hashtable["DiscardedCards"] is List<CardSource>)
                {
                    List<CardSource> DiscardedCards = (List<CardSource>)hashtable["DiscardedCards"];

                    if (DiscardedCards != null)
                    {
                        return DiscardedCards;
                    }
                }
            }
        }

        return null;
    }
    #endregion

    #region Get CardSources from hashtable
    public static List<CardSource> GetCardSourcesFromHashtable(Hashtable hashtable)
    {
        if (hashtable != null)
        {
            if (hashtable.ContainsKey("CardSources"))
            {
                if (hashtable["CardSources"] is List<CardSource>)
                {
                    List<CardSource> CardSources = (List<CardSource>)hashtable["CardSources"];

                    if (CardSources != null)
                    {
                        return CardSources;
                    }
                }
            }
        }

        return null;
    }
    #endregion

    #region Get DigivolutionSources from hashtable
    public static List<CardSource> GetDigivolutionSourcesFromHashtable(Hashtable hashtable)
    {
        if (hashtable != null)
        {
            if (hashtable.ContainsKey("DigivolutionSources"))
            {
                if (hashtable["DigivolutionSources"] is List<CardSource>)
                {
                    List<CardSource> CardSources = (List<CardSource>)hashtable["DigivolutionSources"];

                    if (CardSources != null)
                    {
                        return CardSources;
                    }
                }
            }
        }

        return null;
    }
    #endregion

    #region Get DeckBottom Cards from hashtable
    public static List<CardSource> GetDeckBottomCardsFromHashtable(Hashtable hashtable)
    {
        if (hashtable != null)
        {
            if (hashtable.ContainsKey("DeckBottomCards"))
            {
                if (hashtable["DeckBottomCards"] is List<CardSource>)
                {
                    List<CardSource> DeckBottomCards = (List<CardSource>)hashtable["DeckBottomCards"];

                    if (DeckBottomCards != null)
                    {
                        return DeckBottomCards;
                    }
                }
            }
        }

        return null;
    }
    #endregion

    #region Get Digivolution roots from hashtable
    public static List<CardSource> GetDigivolutionRootsFromEnterFieldHashtable(Hashtable hashtable, Func<Permanent, bool> permanentCondition)
    {
        List<Hashtable> hashtables = GetHashtablesFromHashtable(hashtable);

        if (hashtables != null)
        {
            foreach (Hashtable hashtable1 in hashtables)
            {
                Permanent permanent1 = GetPermanentFromHashtable(hashtable1);

                if (permanent1 != null)
                {
                    if (permanentCondition == null || permanentCondition(permanent1))
                    {
                        if (hashtable1 != null)
                        {
                            if (hashtable1.ContainsKey("evoRoots"))
                            {
                                if (hashtable1["evoRoots"] is List<CardSource>)
                                {
                                    List<CardSource> evoRoots = (List<CardSource>)hashtable1["evoRoots"];

                                    if (evoRoots != null)
                                    {
                                        return evoRoots;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return null;
    }
    #endregion

    #region Get Permanent from hashtable
    public static Permanent GetPermanentFromHashtable(Hashtable hashtable)
    {
        if (hashtable.ContainsKey("Permanent"))
        {
            if (hashtable["Permanent"] is Permanent)
            {
                Permanent Permanent = (Permanent)hashtable["Permanent"];

                if (Permanent != null)
                {
                    return Permanent;
                }
            }
        }

        return null;
    }
    #endregion

    #region Get whether to digivolve from the same level from hashtable
    public static bool IsDigivolvedFromSameLevelFromEnterFieldHashtable(Hashtable hashtable, Permanent permanent)
    {
        if (IsPermanentExistsOnBattleArea(permanent))
        {
            if (permanent.TopCard.HasLevel)
            {
                List<Hashtable> hashtables = GetHashtablesFromHashtable(hashtable);

                if (hashtables != null)
                {
                    foreach (Hashtable hashtable1 in hashtables)
                    {
                        Permanent permanent1 = GetPermanentFromHashtable(hashtable1);

                        if (permanent1 == permanent)
                        {
                            if (hashtable1 != null)
                            {
                                if (hashtable1.ContainsKey("oldLevels"))
                                {
                                    if (hashtable1["oldLevels"] is List<int>)
                                    {
                                        List<int> oldLevels = (List<int>)hashtable1["oldLevels"];

                                        if (oldLevels != null)
                                        {
                                            if (oldLevels.Count((oldLevel) => oldLevel == permanent.Level) >= 1)
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return false;
    }
    #endregion

    #region Get IsAlliance from 1 hashtable
    public static bool IsAlliance(Hashtable hashtable)
    {
        ICardEffect CardEffect = GetCardEffectFromHashtable(hashtable);

        if (CardEffect != null)
        {
            if (CardEffect.EffectName == "Alliance")
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    #region Get isJogress from hashtable
    public static bool IsJogress(Hashtable hashtable)
    {
        if (hashtable != null)
        {
            if (hashtable.ContainsKey("isJogress"))
            {
                if (hashtable["isJogress"] is bool)
                {
                    return (bool)hashtable["isJogress"];
                }
            }
        }

        return false;
    }
    #endregion

    #region Get IsDijiXros from hashtable
    public static bool IsDijiXros(Hashtable hashtable, Func<int, bool> digixrosCountCondition)
    {
        int digiXrosCount = GetDigiXrosCount(hashtable);

        if (digixrosCountCondition != null)
        {
            return digixrosCountCondition(digiXrosCount);
        }

        return false;
    }
    #endregion

    #region Get DigiXrosCount from hashtable
    public static int GetDigiXrosCount(Hashtable hashtable)
    {
        if (hashtable != null)
        {
            if (hashtable.ContainsKey("DigiXrosCount"))
            {
                if (hashtable["DigiXrosCount"] is int)
                {
                    return (int)hashtable["DigiXrosCount"]; ;
                }
            }
        }

        return 0;
    }
    #endregion
}