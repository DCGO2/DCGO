using System;
using System.Collections.Generic;
using System.Linq;

public partial class CardEffectCommons
{
    #region Whether the card is in the field

    public static bool IsExistOnField(CardSource card)
    {
        if (card != null)
        {
            if (card.PermanentOfThisCard() != null)
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Whether the card is in the Breeding Area

    public static bool IsExistOnBreedingArea(CardSource card)
    {
        if (IsExistOnField(card))
        {
            if (card.Owner.GetBreedingAreaPermanents().Contains(card.PermanentOfThisCard()))
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Whether the card is Digimon and in the Breeding Area

    public static bool IsExistOnBreedingAreaDigimon(CardSource card)
    {
        if (IsExistOnField(card))
        {
            if (card.Owner.GetBreedingAreaPermanents().Contains(card.PermanentOfThisCard()))
            {
                if (card.PermanentOfThisCard().IsDigimon)
                {
                    return true;
                }
            }
        }

        return false;
    }

    #endregion

    #region Whether the card is in the Battle Area

    public static bool IsExistOnBattleArea(CardSource card)
    {
        if (IsExistOnField(card))
        {
            if (card.Owner.GetBattleAreaPermanents().Contains(card.PermanentOfThisCard()))
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Whether the card is Digimon and in the Battle Area

    public static bool IsExistOnBattleAreaDigimon(CardSource card)
    {
        if (IsExistOnBattleArea(card))
        {
            if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Whether the card is in hand

    public static bool IsExistOnHand(CardSource card)
    {
        if (card.Owner.HandCards.Contains(card))
        {
            return true;
        }

        return false;
    }

    #endregion

    #region Whether the card is Linked

    public static bool IsExistLinked(CardSource card)
    {
        if (IsExistOnField(card))
            return card.PermanentOfThisCard().LinkedCards.Contains(card);

        return false;
    }

    #endregion

    #region Whether the card is in trash

    public static bool IsExistOnTrash(CardSource card)
    {
        if (card.Owner.TrashCards.Contains(card))
        {
            return true;
        }

        return false;
    }

    #endregion

    #region Whether the card is in trash

    public static bool IsExistInAnyTrash(CardSource card)
    {
        if (card.Owner.TrashCards.Contains(card))
        {
            return true;
        }

        if (card.Owner.Enemy.TrashCards.Contains(card))
        {
            return true;
        }


        return false;
    }

    #endregion

    #region Whether the card is in Executing Area

    public static bool IsExistOnExecutingArea(CardSource card)
    {
        if (card.Owner.ExecutingCards.Contains(card))
        {
            return true;
        }

        return false;
    }

    #endregion

    #region Whether the card is in Security Area

    public static bool IsExistInSecurity(CardSource card, bool isFlipped = false)
    {
        if (card.Owner.SecurityCards.Contains(card))
            return card.IsFlipped == isFlipped;

        return false;
    }

    #endregion

    #region Whether the card Can be played as a new permanent

    public static bool CanPlayAsNewPermanent(
        CardSource cardSource,
        bool payCost,
        ICardEffect cardEffect,
        SelectCardEffect.Root root = SelectCardEffect.Root.Hand,
        bool isBreedingArea = false,
        bool isPlayOption = false,
        int fixedCost = -1) =>
    cardSource != null &&
    (isPlayOption || !cardSource.IsOption) &&
    !GManager.instance.GetComponent<SelectDigiXrosClass>().PreviouslySelectedDigicrossCards.Contains(cardSource) &&
    !GManager.instance.GetComponent<SelectAssemblyClass>().PreviouslySelectedAssemblyCards.Contains(cardSource)
    && cardSource.Owner.fieldCardFrames.Some((frame) =>
    frame.IsEmptyFrame()
    && cardSource.CanPlayCardTargetFrame(frame, payCost, cardEffect, root, isBreedingArea: isBreedingArea, fixedCost: fixedCost));

    #endregion

    #region Whether the permanent is in the Field

    public static bool IsPermanentExistsOnField(Permanent permanent)
    {
        if (permanent != null)
        {
            if (permanent.TopCard != null)
            {
                if (permanent.TopCard.Owner.GetBreedingAreaPermanents().Contains(permanent))
                {
                    return true;
                }

                if (permanent.TopCard.Owner.GetBattleAreaPermanents().Contains(permanent))
                {
                    return true;
                }
            }
        }

        return false;
    }

    #endregion

    #region Whether the permanent is in the Battle Area

    public static bool IsPermanentExistsOnBattleArea(Permanent permanent)
    {
        if (permanent != null)
        {
            if (permanent.TopCard != null)
            {
                if (permanent.TopCard.Owner.GetBattleAreaPermanents().Contains(permanent))
                {
                    return true;
                }
            }
        }

        return false;
    }

    #endregion

    #region Whether the permanent is in the Breeding Area

    public static bool IsPermanentExistsOnBreedingArea(Permanent permanent)
    {
        if (permanent != null)
        {
            if (permanent.TopCard != null)
            {
                if (permanent.TopCard.Owner.GetBreedingAreaPermanents().Contains(permanent))
                {
                    return true;
                }
            }
        }

        return false;
    }

    #endregion

    #region Whether the permanent is card's owner's

    public static bool IsOwnerPermanent(Permanent permanent, CardSource card)
    {
        if (permanent != null)
        {
            if (permanent.TopCard != null)
            {
                if (card != null)
                {
                    if (permanent.TopCard.Owner == card.Owner)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    #endregion

    #region Whether the permanent is card's owner's opponent's

    public static bool IsOpponentPermanent(Permanent permanent, CardSource card)
    {
        if (permanent != null)
        {
            if (permanent.TopCard != null)
            {
                if (permanent.TopCard.Owner == card.Owner.Enemy)
                {
                    return true;
                }
            }
        }

        return false;
    }

    #endregion

    #region Whether the permanent is in the card's owner's Battle Area

    public static bool IsPermanentExistsOnOwnerBattleArea(Permanent permanent, CardSource card)
    {
        if (IsPermanentExistsOnBattleArea(permanent))
        {
            if (IsOwnerPermanent(permanent, card))
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Whether the permanent is in the card's owner's opponent's Battle Area

    public static bool IsPermanentExistsOnOpponentBattleArea(Permanent permanent, CardSource card)
    {
        if (IsPermanentExistsOnBattleArea(permanent))
        {
            if (IsOpponentPermanent(permanent, card))
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Whether the permanent is in the card's owner's Breeding Area

    public static bool IsPermanentExistsOnOwnerBreedingArea(Permanent permanent, CardSource card)
    {
        if (IsPermanentExistsOnBreedingArea(permanent))
        {
            if (IsOwnerPermanent(permanent, card))
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Whether the permanent is in the card's owner's opponent's Breeding Area

    public static bool IsPermanentExistsOnOpponentBreedingArea(Permanent permanent, CardSource card)
    {
        if (IsPermanentExistsOnBreedingArea(permanent))
        {
            if (IsOpponentPermanent(permanent, card))
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Whether the permanent is Digimon and in the Battle Area

    public static bool IsPermanentExistsOnBattleAreaDigimon(Permanent permanent)
    {
        if (IsPermanentExistsOnBattleArea(permanent))
        {
            if (permanent.IsDigimon)
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Whether the permanent is Digimon and in the card's owner's Battle Area

    public static bool IsPermanentExistsOnOwnerBattleAreaDigimon(Permanent permanent, CardSource card)
    {
        if (IsPermanentExistsOnOwnerBattleArea(permanent, card))
        {
            if (permanent.IsDigimon)
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Whether the permanent is Digimon and in the card's owner's opponent's Battle Area

    public static bool IsPermanentExistsOnOpponentBattleAreaDigimon(Permanent permanent, CardSource card)
    {
        if (IsPermanentExistsOnOpponentBattleArea(permanent, card))
        {
            if (permanent.IsDigimon)
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Whether the permanent is Tamer and in the Battle Area

    public static bool IsPermanentExistsOnBattleAreaTamer(Permanent permanent)
    {
        if (IsPermanentExistsOnBattleArea(permanent))
        {
            if (permanent.IsTamer)
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Whether the permanent is Tamer and in the card's owner's Battle Area

    public static bool IsPermanentExistsOnOwnerBattleAreaTamer(Permanent permanent, CardSource card)
    {
        if (IsPermanentExistsOnOwnerBattleArea(permanent, card))
        {
            if (permanent.IsTamer)
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Whether the permanent is Tamer and in the card's owner's opponent's Battle Area

    public static bool IsPermanentExistsOnOpponentBattleAreaTamer(Permanent permanent, CardSource card)
    {
        if (IsPermanentExistsOnOpponentBattleArea(permanent, card))
        {
            if (permanent.IsTamer)
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    #region How many permanents there are in the Battle Area that satisfy the condition

    public static int MatchConditionPermanentCount(Func<Permanent, bool> CanSelectPermanentCondition, bool isContainBreedingArea = false)
    {
        if (isContainBreedingArea)
        {
            return GManager.instance.turnStateMachine.gameContext.Players
                    .Map(player => player.GetFieldPermanents())
                    .Flat()
                    .Count(CanSelectPermanentCondition);
        }
        else
        {
            return GManager.instance.turnStateMachine.gameContext.Players
                    .Map(player => player.GetBattleAreaPermanents())
                    .Flat()
                    .Count(CanSelectPermanentCondition);
        }
    }

    #endregion

    #region How many permanents there are in the owner's Battle Area that satisfy the condition

    public static int MatchConditionOwnersPermanentCount(CardSource card, Func<Permanent, bool> CanSelectPermanentCondition)
    {
        return MatchConditionPermanentCount(permanent => CanSelectPermanentCondition(permanent) && IsOwnerPermanent(permanent, card));
    }

    #endregion

    #region How many permanents there are in the owner's opponent's Battle Area that satisfy the condition

    public static int MatchConditionOpponentsPermanentCount(CardSource card, Func<Permanent, bool> CanSelectPermanentCondition)
    {
        return MatchConditionPermanentCount(permanent => CanSelectPermanentCondition(permanent) && IsOpponentPermanent(permanent, card));
    }

    #endregion

    #region Whether there is at least 1 permanent in the Battle Area that satisfies the condition

    public static bool HasMatchConditionPermanent(Func<Permanent, bool> CanSelectPermanentCondition, bool isContainBreedingArea = false)
    {
        if (isContainBreedingArea)
        {
            return GManager.instance.turnStateMachine.gameContext.Players
                    .Map(player => player.GetFieldPermanents())
                    .Flat()
                    .Some(CanSelectPermanentCondition);
        }
        else
        {
            return GManager.instance.turnStateMachine.gameContext.Players
                    .Map(player => player.GetBattleAreaPermanents())
                    .Flat()
                    .Some(CanSelectPermanentCondition);
        }
    }

    #endregion

    #region Whether there is at least 1 card in the owner's hand that satisfies the condition

    public static bool HasMatchConditionOwnersHand(CardSource card, Func<CardSource, bool> CanSelectCardCondition)
    {
        return card.Owner.HandCards.Some(CanSelectCardCondition);
    }

    #endregion

    #region How many cards there are in the owner's hand that satisfy the condition

    public static int MatchConditionOwnersCardCountInHand(CardSource card, Func<CardSource, bool> CanSelectCardCondition)
    {
        return card.Owner.HandCards.Count(CanSelectCardCondition);
    }

    #endregion

    #region Whether there is at least 1 permanent in the owner's Battle Area that satisfies the condition

    public static bool HasMatchConditionOwnersPermanent(CardSource card, Func<Permanent, bool> CanSelectPermanentCondition)
    {
        return GManager.instance.turnStateMachine.gameContext.Players
        .Map(player => player.GetBattleAreaPermanents())
        .Flat()
        .Some(permanent => IsOwnerPermanent(permanent, card) && CanSelectPermanentCondition(permanent));
    }

    #endregion

    #region Whether there is at least 1 permanent in the owner's Breeding Area that satisfies the condition

    public static bool HasMatchConditionOwnersBreedingPermanent(CardSource card, Func<Permanent, bool> CanSelectPermanentCondition)
    {
        return GManager.instance.turnStateMachine.gameContext.Players
        .Map(player => player.GetBreedingAreaPermanents())
        .Flat()
        .Some(permanent => IsOwnerPermanent(permanent, card) && CanSelectPermanentCondition(permanent));
    }

    #endregion

    #region Whether there is at least 1 source in this cards sources that satisfies the condition

    public static bool HasMatchConditionPermanentDigivolutionCards(CardSource card, Func<CardSource, bool> CanSelectPermanentCondition)
    {
        return card.PermanentOfThisCard().DigivolutionCards.Some(cardSource => CanSelectPermanentCondition(cardSource));
    }

    #endregion

    #region Whether there is at least 1 permanent in the owner's opponent's Battle Area that satisfies the condition

    public static bool HasMatchConditionOpponentsPermanent(CardSource card, Func<Permanent, bool> CanSelectPermanentCondition)
    {
        return GManager.instance.turnStateMachine.gameContext.Players
        .Map(player => player.GetBattleAreaPermanents())
        .Flat()
        .Some(permanent => IsOpponentPermanent(permanent, card) && CanSelectPermanentCondition(permanent));
    }

    #endregion

    #region Whether there is at least 1 permanent in the owner's Security Area that satisfies the condition

    public static bool HasMatchConditionOwnersSecurity(CardSource card, Func<CardSource, bool> CanSelectPermanentCondition, bool flipped = true)
    {
        return GManager.instance.turnStateMachine.gameContext.Players
        .Map(player => player.SecurityCards)
        .Flat()
        .Some(cardSource => cardSource.IsFlipped == flipped && CanSelectPermanentCondition(cardSource) && cardSource.Owner == card.Owner);
    }

    #endregion

    #region How many cards there are in the owner's trash that satisfy the condition

    public static int MatchConditionOwnersCardCountInTrash(CardSource card, Func<CardSource, bool> CanSelectCardCondition)
    {
        return card.Owner.TrashCards.Count(CanSelectCardCondition);
    }

    #endregion

    #region How many cards there are in the owner's opponent's trash that satisfy the condition

    public static int MatchConditionOpponentsCardCountInTrash(CardSource card, Func<CardSource, bool> CanSelectCardCondition)
    {
        return card.Owner.Enemy.TrashCards.Count(CanSelectCardCondition);
    }

    #endregion

    #region Whether there is at least 1 card in the owner's trash that satisfies the condition

    public static bool HasMatchConditionOwnersCardInTrash(CardSource card, Func<CardSource, bool> CanSelectCardCondition)
    {
        return card.Owner.TrashCards.Some(CanSelectCardCondition);
    }

    #endregion

    #region Whether there is at least 1 card in the owner's opponent's trash that satisfies the condition

    public static bool HasMatchConditionOpponentsCardInTrash(CardSource card, Func<CardSource, bool> CanSelectCardCondition)
    {
        return card.Owner.Enemy.TrashCards.Some(CanSelectCardCondition);
    }

    #endregion

    #region Whether the list has no element

    public static bool HasNoElement<T>(List<T> list) => list.Count <= 0;

    #endregion

    #region Wheter it is the player's turn

    public static bool IsOwnerTurn(CardSource card) => GManager.instance.turnStateMachine.gameContext.TurnPlayer == card.Owner;

    public static bool IsOpponentTurn(CardSource card) => !IsOwnerTurn(card);

    #endregion

    #region Whether the effect is your effect

    public static bool IsOwnerEffect(ICardEffect cardEffect, CardSource card)
    {
        if (cardEffect != null)
        {
            if (cardEffect.EffectSourceCard != null)
            {
                if (cardEffect.EffectSourceCard.Owner == card.Owner)
                {
                    return true;
                }
            }
        }

        return false;
    }

    #endregion

    #region Whether the effect is opponent's effect

    public static bool IsOpponentEffect(ICardEffect cardEffect, CardSource card)
    {
        if (cardEffect != null)
        {
            if (cardEffect.EffectSourceCard != null)
            {
                if (cardEffect.EffectSourceCard.Owner == card.Owner.Enemy)
                {
                    return true;
                }
            }
        }

        return false;
    }

    #endregion

    #region Get unique colour count from permanents in owners battle area

    public static int GetUniqueColourCountOnOwnerBattleArea(CardSource card, Func<Permanent, bool> canGetCardColour)
    {
        var uniqueColors = card.Owner.GetBattleAreaPermanents()
        .Filter(x => canGetCardColour(x))
        .Select(x => x.TopCard)
        .SelectMany(x => x.CardColors)
        .Distinct()
        .ToHashSet();
        return uniqueColors.Count;
    }

    #endregion

    #region Get unique colour count from permanents in opponents battle area

    public static int GetUniqueColourCountOnOpponentsBattleArea(CardSource card, Func<Permanent, bool> canGetCardColour)
    {
        var uniqueColors = card.Owner.Enemy.GetBattleAreaPermanents()
        .Filter(x => canGetCardColour(x))
        .Select(x => x.TopCard)
        .SelectMany(x => x.CardColors)
        .Distinct()
        .ToHashSet();
        return uniqueColors.Count;
    }

    #endregion

    #region Does Owner has 1 or less tamers on the field

    public static bool OwnerHas1OrLessTamers(CardSource card)
    {
        return card.Owner.GetBattleAreaPermanents()
            .Count(permanent => permanent.IsTamer && permanent.TopCard.Owner == card.Owner) <= 1;
    }

    #endregion

    #region Universial Root Can No Select Condition

    public static bool UniversalRootCanNoSelectCondition(CardSource cardSource)
    {
        if (cardSource.PayingCost(SelectCardEffect.Root.Hand, null, checkAvailability: false) > cardSource.Owner.MaxMemoryCost) return false;
        if (cardSource.PayingCost(SelectCardEffect.Root.Trash, null, checkAvailability: false) > cardSource.Owner.MaxMemoryCost) return false;
        if (cardSource.PayingCost(SelectCardEffect.Root.DigivolutionCards, null, checkAvailability: false) > cardSource.Owner.MaxMemoryCost) return false;
        if (cardSource.PayingCost(SelectCardEffect.Root.Library, null, checkAvailability: false) > cardSource.Owner.MaxMemoryCost) return false;
        if (cardSource.PayingCost(SelectCardEffect.Root.DigivolutionCards, null, checkAvailability: false) > cardSource.Owner.MaxMemoryCost) return false;
        if (cardSource.PayingCost(SelectCardEffect.Root.LinkedCards, null, checkAvailability: false) > cardSource.Owner.MaxMemoryCost) return false;
        if (cardSource.PayingCost(SelectCardEffect.Root.Security, null, checkAvailability: false) > cardSource.Owner.MaxMemoryCost) return false;
        return true;
    }

    #endregion
}