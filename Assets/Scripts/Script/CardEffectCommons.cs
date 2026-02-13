using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Digivolution Requirements

    public enum IgnoreRequirement
    {
        None,
        All,
        Level,
        Color
    }

    #endregion

    #region Play cards as new permanents

    public static IEnumerator PlayPermanentCards(List<CardSource> cardSources, ICardEffect activateClass, bool payCost, bool isTapped, SelectCardEffect.Root root, bool activateETB, bool isBreedingArea = false, int fixedCost = -1)
    {
        if (cardSources == null) yield break;

        cardSources = cardSources
        .Filter(cardSource => cardSource != null)
        .Filter(cardSource => CanPlayAsNewPermanent(
            cardSource: cardSource,
            payCost: payCost,
            cardEffect: activateClass,
            isBreedingArea: isBreedingArea,
            fixedCost: fixedCost));

        if (cardSources.Count == 0) yield break;

        PlayCardClass playCardClass = new PlayCardClass(
            cardSources: cardSources,
            hashtable: CardEffectHashtable(activateClass),
            payCost: payCost,
            targetPermanent: null,
            isTapped: isTapped,
            root: root,
            activateETB: activateETB);

        if (isBreedingArea)
            playCardClass.SetIsBreedingArea();

        playCardClass.SetFixedCost(fixedCost);

        yield return ContinuousController.instance.StartCoroutine(playCardClass.PlayCard());
    }

    #endregion

    #region Play option cards

    public static IEnumerator PlayOptionCards(List<CardSource> cardSources, ICardEffect activateClass, bool payCost, SelectCardEffect.Root root,
        bool setAddSecurityEndOption = false)
    {
        if (cardSources == null) yield break;

        cardSources = cardSources
        .Filter(cardSource => cardSource != null)
        .Filter(cardSource => !cardSource.CanNotPlayThisOption);

        if (cardSources.Count == 0) yield break;

        PlayCardClass playCard = new PlayCardClass(
                cardSources: cardSources,
                hashtable: CardEffectHashtable(activateClass),
                payCost: payCost,
                targetPermanent: null,
                isTapped: false,
                root: root,
                activateETB: true);

        if (activateClass != null)
        {
            playCard.SetShowEffect();
        }

        if (setAddSecurityEndOption)
        {
            playCard.SetAddSecurityEndOption();
        }

        yield return ContinuousController.instance.StartCoroutine(playCard.PlayCard());
    }

    #endregion

    #region Play Delay Option cards as new permanents

    public static IEnumerator PlaceDelayOptionCards(CardSource card, ICardEffect cardEffect, SelectCardEffect.Root root = SelectCardEffect.Root.Execution)
    {
        if (CanPlayAsNewPermanent(cardSource: card, payCost: false, cardEffect: cardEffect, isPlayOption: true))
        {
            PlayPermanentClass playPermanent = new PlayPermanentClass(
                cardSources: new List<CardSource>() { card },
                hashtable: CardEffectHashtable(cardEffect),
                targetPermanent: null,
                isTapped: false,
                root: root,
                ActivateETB: true);

            playPermanent.SetISPlayOption();

            yield return ContinuousController.instance.StartCoroutine(playPermanent.PlayPermanent());

            if (IsExistOnBattleArea(card))
            {
                card.PermanentOfThisCard().IsPlayedOptionPermanent = true;
            }
        }
    }

    #endregion

    #region Play 1 Token

    public static IEnumerator PlayToken(CEntity_Base tokenData, ICardEffect activateClass, bool isOwnerPermanent, bool isTapped, int quantity = 1)
    {
        if (activateClass == null) yield break;
        if (activateClass.EffectSourceCard == null) yield break;
        if (tokenData == null) yield break;

        CardSource card = activateClass.EffectSourceCard;

        Player player = isOwnerPermanent ? card.Owner : card.Owner.Enemy;

        if (card.Owner.fieldCardFrames.Count((frame) => frame.IsEmptyFrame() && frame.IsBattleAreaFrame()) >= quantity)
        {
            List<CardSource> playCards = new List<CardSource>();

            for (int i = 0; i < quantity; i++)
            {
                CardSource tokenCard = CardObjectController.CreateCardSource(
                player.PlayerID,
                tokenData,
                true);

                playCards.Add(tokenCard);
            }

            if (CanPlayAsNewPermanent(cardSource: playCards[0], payCost: false, cardEffect: activateClass))
            {
                yield return ContinuousController.instance.StartCoroutine(new PlayCardClass(
                    cardSources: playCards,
                    hashtable: CardEffectHashtable(activateClass),
                    payCost: false,
                    targetPermanent: null,
                    isTapped: isTapped,
                    root: SelectCardEffect.Root.None,
                    activateETB: true).PlayCard());
            }
        }
    }

    #endregion

    #region Play 1 [Diaboromon] Token

    public static IEnumerator PlayDiaboromonToken(ICardEffect activateClass, int quantity = 1)
    {
        yield return ContinuousController.instance.StartCoroutine(PlayToken(
            tokenData: ContinuousController.instance.DiaboromonToken,
            activateClass: activateClass,
            isOwnerPermanent: true,
            isTapped: false,
            quantity: quantity
        ));
    }

    #endregion

    #region Play 1 [Amon of Crimson Flame] Token

    public static IEnumerator PlayAmonToken(ICardEffect activateClass)
    {
        yield return ContinuousController.instance.StartCoroutine(PlayToken(
            tokenData: ContinuousController.instance.AmonToken,
            activateClass: activateClass,
            isOwnerPermanent: true,
            isTapped: false
        ));
    }

    #endregion

    #region Play 1 [Umon of Blue Thunder] Token

    public static IEnumerator PlayUmonToken(ICardEffect activateClass)
    {
        yield return ContinuousController.instance.StartCoroutine(PlayToken(
            tokenData: ContinuousController.instance.UmonToken,
            activateClass: activateClass,
            isOwnerPermanent: true,
            isTapped: false
        ));
    }

    #endregion

    #region Play 1 [Fujitsumon] Token

    public static IEnumerator PlayFujitsumonToken(ICardEffect activateClass, bool isOwnerPermanent)
    {
        yield return ContinuousController.instance.StartCoroutine(PlayToken(
            tokenData: ContinuousController.instance.FujitsumonToken,
            activateClass: activateClass,
            isOwnerPermanent: isOwnerPermanent,
            isTapped: true
        ));
    }

    #endregion

    #region Play 1 [Gyuukimon] Token

    public static IEnumerator PlayGyuukimonToken(ICardEffect activateClass)
    {
        yield return ContinuousController.instance.StartCoroutine(PlayToken(
            tokenData: ContinuousController.instance.GyuukimonToken,
            activateClass: activateClass,
            isOwnerPermanent: true,
            isTapped: false
        ));
    }

    #endregion

    #region Play 1 [KoHagurumon] Token

    public static IEnumerator PlayKoHagurumonToken(ICardEffect activateClass)
    {
        yield return ContinuousController.instance.StartCoroutine(PlayToken(
            tokenData: ContinuousController.instance.KoHagurumonToken,
            activateClass: activateClass,
            isOwnerPermanent: true,
            isTapped: false
        ));
    }

    #endregion

    #region Play 1 [Familiar] Token

    public static IEnumerator PlayFamiliarToken(ICardEffect activateClass, int quantity = 1)
    {
        yield return ContinuousController.instance.StartCoroutine(PlayToken(
            tokenData: ContinuousController.instance.FamiliarToken,
            activateClass: activateClass,
            isOwnerPermanent: true,
            isTapped: false,
            quantity: quantity
        ));
    }

    #endregion

    #region Play 1 Self Deleting [Familiar] Token

    public static IEnumerator PlaySelfDeleteFamiliarToken(ICardEffect activateClass, int quantity = 1)
    {
        yield return ContinuousController.instance.StartCoroutine(PlayToken(
            tokenData: ContinuousController.instance.SelfDeleteFamiliarToken,
            activateClass: activateClass,
            isOwnerPermanent: true,
            isTapped: false,
            quantity: quantity
        ));
    }

    #endregion

    #region Play 1 [Vol�e & Zerdr�cken] Token

    public static IEnumerator PlayVoleeZerdrucken(ICardEffect activateClass)
    {
        yield return ContinuousController.instance.StartCoroutine(PlayToken(
            tokenData: ContinuousController.instance.VoleeZerdruckenToken,
            activateClass: activateClass,
            isOwnerPermanent: true,
            isTapped: false
        ));
    }

    #endregion

    #region Play 1 [Uka-no-Mitama] Token

    public static IEnumerator PlayUkaNoMitama(ICardEffect activateClass)
    {
        yield return ContinuousController.instance.StartCoroutine(PlayToken(
            tokenData: ContinuousController.instance.UkaNoMitamaToken,
            activateClass: activateClass,
            isOwnerPermanent: true,
            isTapped: false
        ));
    }

    #endregion

    #region Play 1 [WarGrowlmon] Token

    public static IEnumerator PlayWarGrowlmonToken(ICardEffect activateClass)
    {
        yield return ContinuousController.instance.StartCoroutine(PlayToken(
            tokenData: ContinuousController.instance.WarGrowlmonToken,
            activateClass: activateClass,
            isOwnerPermanent: true,
            isTapped: false
        ));
    }

    #endregion

    #region Play 1 [Taomon] Token

    public static IEnumerator PlayTaomonToken(ICardEffect activateClass)
    {
        yield return ContinuousController.instance.StartCoroutine(PlayToken(
            tokenData: ContinuousController.instance.TaomonToken,
            activateClass: activateClass,
            isOwnerPermanent: true,
            isTapped: false
        ));
    }

    #endregion

    #region Play 1 [Rapidmon] Token

    public static IEnumerator PlayRapidmonToken(ICardEffect activateClass)
    {
        yield return ContinuousController.instance.StartCoroutine(PlayToken(
            tokenData: ContinuousController.instance.RapidmonToken,
          activateClass: activateClass,
            isOwnerPermanent: true,
            isTapped: false
        ));
    }

    #endregion

    #region Play 1 [Pipe-Fox] Token

    public static IEnumerator PlayPipeFox(ICardEffect activateClass)
    {
        yield return ContinuousController.instance.StartCoroutine(PlayToken(
            tokenData: ContinuousController.instance.PipeFoxToken,
            activateClass: activateClass,
            isOwnerPermanent: true,
            isTapped: false
        ));
    }

    #endregion

    #region Play 1 [AthoRenePor] Token

    public static IEnumerator PlayAthoRenePorToken(ICardEffect activateClass)
    {
        yield return ContinuousController.instance.StartCoroutine(PlayToken(
            tokenData: ContinuousController.instance.AthoRenePorToken,
            activateClass: activateClass,
            isOwnerPermanent: true,
            isTapped: false
            ));
    }

    #endregion

    #region Play 1 [Hinukamuy] Token

    public static IEnumerator PlayHinukamuyToken(ICardEffect activateClass)
    {
        yield return ContinuousController.instance.StartCoroutine(PlayToken(
            tokenData: ContinuousController.instance.HinukamuyToken,
            activateClass: activateClass,
            isOwnerPermanent: true,
            isTapped: false
            ));
    }

    #endregion

    #region Play 1 [Petrification] Token

    public static IEnumerator PlayPetrificationToken(ICardEffect activateClass)
    {
        yield return ContinuousController.instance.StartCoroutine(PlayToken(
            tokenData: ContinuousController.instance.PetrificationToken,
            activateClass: activateClass,
            isOwnerPermanent: false,
            isTapped: false
            ));
    }

    #endregion

    #region Security effect of "add this card to hand"

    public static IEnumerator AddThisCardToHand(CardSource card1, ICardEffect activateClass)
    {
        yield return new WaitForSeconds(0.5f);

        yield return ContinuousController.instance.StartCoroutine(card1.Owner.brainStormObject.CloseBrainstrorm(card1));

        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddHandCards(new List<CardSource>() { card1 }, false, activateClass));
    }

    #endregion

    #region Delete target permanents, and the effect determines whether the permanent has been deleted or not

    public static IEnumerator DeletePeremanentAndProcessAccordingToResult(List<Permanent> targetPermanents, ICardEffect activateClass, Func<List<Permanent>, IEnumerator> successProcess, Func<IEnumerator> failureProcess)
    {
        DestroyPermanentsClass destroyPermanentsClass = new DestroyPermanentsClass(targetPermanents, CardEffectHashtable(activateClass));

        yield return ContinuousController.instance.StartCoroutine(destroyPermanentsClass.Destroy());

        if (targetPermanents.Some((permanent) => destroyPermanentsClass.IsDestroyed(permanent)))
        {
            if (successProcess != null)
            {
                yield return ContinuousController.instance.StartCoroutine(successProcess(destroyPermanentsClass.DestroyedPermanents));
            }
        }
        else
        {
            if (failureProcess != null)
            {
                yield return ContinuousController.instance.StartCoroutine(failureProcess());
            }
        }
    }

    #endregion

    #region Bounce target 1 permanent, and the effect determines whether the permanent has been bounced or not

    public static IEnumerator BouncePeremanentAndProcessAccordingToResult(List<Permanent> targetPermanents, ICardEffect activateClass, IEnumerator successProcess, IEnumerator failureProcess)
    {
        HandBounceClaass bouncePermanentClass = new HandBounceClaass(targetPermanents, CardEffectHashtable(activateClass));

        yield return ContinuousController.instance.StartCoroutine(bouncePermanentClass.Bounce());

        if (targetPermanents.Some((permanent) => bouncePermanentClass.IsBounced(permanent)))
        {
            if (successProcess != null)
            {
                yield return ContinuousController.instance.StartCoroutine(successProcess);
            }
        }
        else
        {
            if (failureProcess != null)
            {
                yield return ContinuousController.instance.StartCoroutine(failureProcess);
            }
        }
    }

    #endregion

    #region Deck Bounce target 1 permanent, and the effect determines whether the permanent has been bounced or not

    public static IEnumerator DeckBouncePeremanentAndProcessAccordingToResult(List<Permanent> targetPermanents, ICardEffect activateClass, IEnumerator successProcess, IEnumerator failureProcess)
    {
        DeckBottomBounceClass deckBounceClass = new DeckBottomBounceClass(targetPermanents, CardEffectHashtable(activateClass));

        yield return ContinuousController.instance.StartCoroutine(deckBounceClass.DeckBounce());

        if (targetPermanents.Some((permanent) => deckBounceClass.IsDeckBounced(permanent)))
        {
            if (successProcess != null)
            {
                yield return ContinuousController.instance.StartCoroutine(successProcess);
            }
        }
        else
        {
            if (failureProcess != null)
            {
                yield return ContinuousController.instance.StartCoroutine(failureProcess);
            }
        }
    }

    #endregion

    #region Trash target digivolution cards, and the effect determines whether the digivolution cards were trashed or not

    public static IEnumerator TrashDigivolutionCardsAndProcessAccordingToResult(Permanent targetPermanent, List<CardSource> targetDigivolutionCards, ICardEffect activateClass, Func<List<CardSource>, IEnumerator> successProcess, Func<IEnumerator> failureProcess)
    {
        ITrashDigivolutionCards trashDigivolutionCards = new ITrashDigivolutionCards(targetPermanent, targetDigivolutionCards, activateClass);

        yield return ContinuousController.instance.StartCoroutine(trashDigivolutionCards.TrashDigivolutionCards());

        if (targetDigivolutionCards.Some((sourceCard) => trashDigivolutionCards.IsTrashed(sourceCard)))
        {
            if (successProcess != null)
            {
                yield return ContinuousController.instance.StartCoroutine(successProcess(trashDigivolutionCards.TrashedCards));
            }
        }
        else
        {
            if (failureProcess != null)
            {
                yield return ContinuousController.instance.StartCoroutine(failureProcess());
            }
        }
    }

    #endregion

    #region Trash target link cards, and the effect determines whether the link cards were trashed or not

    public static IEnumerator TrashLinkCardsAndProcessAccordingToResult(Permanent targetPermanent, List<CardSource> targetLinkCards, ICardEffect activateClass, Func<List<CardSource>, IEnumerator> successProcess, Func<IEnumerator> failureProcess)
    {
        ITrashLinkCards trashLinkCards = new ITrashLinkCards(targetPermanent, targetLinkCards, activateClass);

        yield return ContinuousController.instance.StartCoroutine(trashLinkCards.TrashLinkCards());

        if (targetLinkCards.Some((sourceCard) => trashLinkCards.IsTrashed(sourceCard)))
        {
            if (successProcess != null)
            {
                yield return ContinuousController.instance.StartCoroutine(successProcess(trashLinkCards.TrashedLinkCards));
            }
        }
        else
        {
            if (failureProcess != null)
            {
                yield return ContinuousController.instance.StartCoroutine(failureProcess());
            }
        }
    }

    #endregion

    #region Trash Security cards and the effect determines whether the security were trashed or not

    public static IEnumerator TrashSecurityAndProcessAccordingToResult(Player player, int trashAmount, ICardEffect activateClass, bool fromTop, Func<List<CardSource>, IEnumerator> successProcess, Func<IEnumerator> failureProcess)
    {
        IDestroySecurity destroySecurity = new IDestroySecurity(player, trashAmount, activateClass, fromTop);

        yield return ContinuousController.instance.StartCoroutine(destroySecurity.DestroySecurity());

        if (destroySecurity.DestroyedSecurity.Any())
        {
            if (successProcess != null)
            {
                yield return ContinuousController.instance.StartCoroutine(successProcess(destroySecurity.DestroyedSecurity));
            }
        }
        else
        {
            if (failureProcess != null)
            {
                yield return ContinuousController.instance.StartCoroutine(failureProcess());
            }
        }
    }

    #endregion

    #region Trash digivolution cards from top or bottom

    public static IEnumerator TrashDigivolutionCardsFromTopOrBottom(Permanent targetPermanent, int trashCount, bool isFromTop, ICardEffect activateClass, Func<CardSource, bool> cardCondition = null)
    {
        if (activateClass == null) yield break;
        if (targetPermanent == null) yield break;
        if (targetPermanent.TopCard == null) yield break;
        if (targetPermanent.DigivolutionCards.Count((cardSource) => !cardSource.CanNotTrashFromDigivolutionCards(activateClass)) == 0) yield break;
        if (targetPermanent.TopCard.CanNotBeAffected(activateClass)) yield break;
        if (trashCount <= 0) yield break;

        List<CardSource> trashTargetCards = new List<CardSource>();

        for (int i = 0; i < targetPermanent.DigivolutionCards.Count; i++)
        {
            if (trashTargetCards.Count >= trashCount)
                break;

            int index = isFromTop ? i : targetPermanent.DigivolutionCards.Count - 1 - i;

            CardSource trashTargetCard = targetPermanent.DigivolutionCards[index];

            bool trashCard = true;

            if (cardCondition != null)
                trashCard = cardCondition(trashTargetCard);

            if (trashCard)
                trashTargetCards.Add(trashTargetCard);
        }

        yield return ContinuousController.instance.StartCoroutine(new ITrashDigivolutionCards(targetPermanent, trashTargetCards, activateClass).TrashDigivolutionCards());
    }

    #endregion

    #region this Option card's Main effect

    public static ActivateClass OptionMainEffect(CardSource card) => card.EffectList(EffectTiming.OptionSkill).Find(cardEffect => cardEffect != null && cardEffect is ActivateClass && cardEffect.EffectDiscription.Contains("[Main]")) as ActivateClass;

    #endregion

    #region this Option card's Security effect

    public static ActivateClass OptionSecurityEffect(CardSource card) => card.EffectList(EffectTiming.SecuritySkill).Find(cardEffect => cardEffect != null && cardEffect is ActivateClass && cardEffect.EffectDiscription.Contains("[Security]")) as ActivateClass;

    #endregion

    #region Add Option's Security effect that "Activate this card's Main effect" to cardEffects

    public static void AddActivateMainOptionSecurityEffect(CardSource card, ref List<ICardEffect> cardEffects, string effectName, string effectDiscription = "", Func<ICardEffect, IEnumerator> afterMainEffect = null)
    {
        if (OptionMainEffect(card) == null) return;

        cardEffects.Add(CardEffectFactory.ActivateMainOptionSecurityEffect(card, effectName, effectDiscription, afterMainEffect));
    }

    #endregion

    #region Target permanent Digivolves into Digimon card from hand or trash

    public static IEnumerator DigivolveIntoHandOrTrashCard(
        Permanent targetPermanent,
        Func<CardSource, bool> cardCondition,
        bool payCost,
        (int reduceCost, Func<CardSource, bool> reduceCostCardCondition)? reduceCostTuple,
        (int fixedCost, Func<CardSource, bool> fixedCostCardCondition)? fixedCostTuple,
        int ignoreDigivolutionRequirementFixedCost,
        bool isHand,
        ICardEffect activateClass,
        IEnumerator successProcess,
        //bool ignoreLevel = false,
        bool ignoreSelection = false,
        IgnoreRequirement ignoreRequirements = IgnoreRequirement.None,
        IEnumerator failedProcess = null,
        bool isOptional = true)
    {
        if (targetPermanent == null) yield break;
        if (targetPermanent.TopCard == null) yield break;
        if (activateClass == null) yield break;
        if (activateClass.EffectSourceCard == null) yield break;

        Player owner = targetPermanent.TopCard.Owner;
        SelectCardEffect.Root root = isHand ? SelectCardEffect.Root.Hand : SelectCardEffect.Root.Trash;
        bool ignoreDigivolutionRequirement = !ignoreRequirements.Equals(IgnoreRequirement.None) || ignoreDigivolutionRequirementFixedCost >= 0;//  ignoreDigivolutionRequirementFixedCost >= 0 || ignoreRequirements;

        int fixedCost = -1;

        bool successful = false;

        if (fixedCostTuple != null)
        {
            fixedCost = fixedCostTuple.Value.fixedCost;
        }

        if (ignoreDigivolutionRequirement)
        {
            fixedCost = ignoreDigivolutionRequirementFixedCost;
        }

        bool CanSelectCardCondition(CardSource cardSource)
        {
            if (cardSource.IsDigimon)
            {
                if (cardCondition == null || cardCondition(cardSource))
                {
                    if (!cardSource.CanNotEvolve(targetPermanent))
                    {
                        bool isBreeding = cardSource.Owner.GetBreedingAreaPermanents().Contains(targetPermanent);

                        if (cardSource.CanPlayCardTargetFrame(
                            frame: targetPermanent.PermanentFrame,
                            PayCost: payCost,
                            cardEffect: activateClass,
                            root: root,
                            fixedCost: fixedCost,
                            ignore: ignoreRequirements,
                            isBreedingArea: isBreeding))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        #region reduce cost

        Func<EffectTiming, ICardEffect> getChangeCostEffect = null;

        if (reduceCostTuple != null)
        {
            bool PermanentCondition(Permanent permanent)
            {
                return permanent == targetPermanent;
            }

            bool CardCondition(CardSource cardSource)
            {
                if (cardSource != null)
                {
                    if (cardSource.Owner == owner)
                    {
                        if (reduceCostTuple.Value.reduceCostCardCondition == null || reduceCostTuple.Value.reduceCostCardCondition(cardSource))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            getChangeCostEffect = ChangeDigivolutionCostPlayerEffect(
                permanentCondition: PermanentCondition,
                cardCondition: CardCondition,
                rootCondition: null,
                changeValue: -reduceCostTuple.Value.reduceCost,
                activateClass: activateClass,
                setFixedCost: false);

            if (getChangeCostEffect != null)
            {
                AddEffectToPlayer(
                    effectDuration: EffectDuration.UntilCalculateFixedCost,
                    card: targetPermanent.TopCard,
                    cardEffect: null,
                    timing: EffectTiming.None,
                    getCardEffect: getChangeCostEffect);
            }
        }

        #endregion

        #region set fixed cost

        Func<EffectTiming, ICardEffect> getFixedCostEffect = null;

        if (fixedCostTuple != null)
        {
            bool PermanentCondition(Permanent permanent)
            {
                return permanent == targetPermanent;
            }

            bool CardCondition(CardSource cardSource)
            {
                if (cardSource != null)
                {
                    if (cardSource.Owner == owner)
                    {
                        if (fixedCostTuple.Value.fixedCostCardCondition == null || fixedCostTuple.Value.fixedCostCardCondition(cardSource))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            getFixedCostEffect = ChangeDigivolutionCostPlayerEffect(
                permanentCondition: PermanentCondition,
                cardCondition: CardCondition,
                rootCondition: null,
                changeValue: fixedCost,
                activateClass: activateClass,
                setFixedCost: true);

            if (getChangeCostEffect != null)
            {
                AddEffectToPlayer(
                    effectDuration: EffectDuration.UntilCalculateFixedCost,
                    card: targetPermanent.TopCard,
                    cardEffect: null,
                    timing: EffectTiming.None,
                    getCardEffect: getFixedCostEffect);
            }
        }

        #endregion

        #region ignore digivolution requirement

        Func<EffectTiming, ICardEffect> getIgnoreDigivolutionRequirementEffect = null;

        if (ignoreDigivolutionRequirement)
        {
            bool PermanentCondition(Permanent permanent)
            {
                return permanent == targetPermanent;
            }

            bool CardCondition(CardSource cardSource)
            {
                if (cardSource == null)
                {
                    return false;
                }

                if (cardSource.Owner != owner)
                {
                    return false;
                }

                if (cardCondition == null || cardCondition(cardSource))
                {
                    return true;
                }

                return false;
            }

            getIgnoreDigivolutionRequirementEffect = GainIgnoreDigivolutionRequirementPlayerEffect(
                permanentCondition: PermanentCondition,
                cardCondition: CardCondition,
                ignoreDigivolutionRequirement: ignoreDigivolutionRequirement,
                digivolutionCost: fixedCost,
                activateClass: activateClass);

            if (getIgnoreDigivolutionRequirementEffect != null)
            {
                AddEffectToPlayer(
                effectDuration: EffectDuration.UntilCalculateFixedCost,
                card: targetPermanent.TopCard,
                cardEffect: null,
                timing: EffectTiming.None,
                getCardEffect: getIgnoreDigivolutionRequirementEffect);
            }
        }

        #endregion

        List<CardSource> selectedCards = new List<CardSource>();

        IEnumerator SelectCardCoroutine(CardSource cardSource)
        {
            selectedCards.Add(cardSource);

            yield return null;
        }

        if (isHand)
        {
            if (owner.HandCards.Count(CanSelectCardCondition) >= 1)
            {
                int maxCount = Mathf.Min(1, owner.HandCards.Count(CanSelectCardCondition));

                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                selectHandEffect.SetUp(
                    selectPlayer: owner,
                    canTargetCondition: CanSelectCardCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: maxCount,
                    canNoSelect: isOptional,
                    canEndNotMax: false,
                    isShowOpponent: true,
                    selectCardCoroutine: SelectCardCoroutine,
                    afterSelectCardCoroutine: null,
                    mode: SelectHandEffect.Mode.Custom,
                    cardEffect: activateClass);

                selectHandEffect.SetUpCustomMessage("Select 1 card to digivolve.", "The opponent is selecting 1 card to digivolve.");
                selectHandEffect.SetUpCustomMessage_ShowCard("Selected Card");

                yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
            }
        }
        else
        {
            if (HasMatchConditionOwnersCardInTrash(targetPermanent.TopCard, CanSelectCardCondition) && !ignoreSelection)
            {
                int maxCount = 1;

                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => isOptional,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 card to digivolve.",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: owner,
                            cardEffect: activateClass);

                selectCardEffect.SetUpCustomMessage("Select 1 card to digivolve.", "The opponent is selecting 1 card to digivolve.");
                selectCardEffect.SetUpCustomMessage_ShowCard("Selected Card");

                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
            }
            else
            {
                if (ignoreSelection)
                    selectedCards.Add(activateClass.EffectSourceCard);
            }
        }

        PlayCardClass playCardClass = new PlayCardClass(
            cardSources: selectedCards,
            hashtable: CardEffectHashtable(activateClass),
            payCost: payCost,
            targetPermanent: targetPermanent,
            isTapped: false,
            root: root,
            activateETB: true);

        //if (ignoreRequirements.Equals(IgnoreRequirement.All) || ignoreRequirements.Equals(IgnoreRequirement.Level))
        //    playCardClass.SetIgnoreLevel();
        playCardClass.SetIgnoreRequirements(ignoreRequirements);

        //!ignoreRequirements
        if (fixedCost >= 0)
            playCardClass.SetFixedCost(fixedCost);

        yield return ContinuousController.instance.StartCoroutine(playCardClass.PlayCard());

        #region release effect

        if (getChangeCostEffect != null) owner.UntilCalculateFixedCostEffect.Remove(getChangeCostEffect);

        #endregion

        #region release effect

        if (getFixedCostEffect != null) owner.UntilCalculateFixedCostEffect.Remove(getFixedCostEffect);

        #endregion

        #region release effect

        if (getIgnoreDigivolutionRequirementEffect != null) owner.UntilCalculateFixedCostEffect.Remove(getIgnoreDigivolutionRequirementEffect);

        #endregion

        if (selectedCards.Count >= 1)
        {
            if (IsDigivolvedByTheEffect(targetPermanent, selectedCards[0], activateClass))
            {
                successful = true;
            }
        }

        if (successful)
        {
            if (successProcess != null)
                yield return ContinuousController.instance.StartCoroutine(successProcess);
        }
        else
        {
            if (failedProcess != null)
                yield return ContinuousController.instance.StartCoroutine(failedProcess);
        }
    }

    #endregion

    #region Target permanent Digivolves into Digimon card execution area

    public static IEnumerator DigivolveIntoExcecutingAreaCard(
        Permanent targetPermanent,
        Func<CardSource, bool> cardCondition,
        bool payCost,
        (int reduceCost, Func<CardSource, bool> reduceCostCardCondition)? reduceCostTuple,
        (int fixedCost, Func<CardSource, bool> fixedCostCardCondition)? fixedCostTuple,
        int ignoreDigivolutionRequirementFixedCost,
        ICardEffect activateClass,
        IEnumerator successProcess,
        bool ignoreSelection = false,
        IgnoreRequirement ignoreRequirements = IgnoreRequirement.None)
    {
        if (targetPermanent == null) yield break;
        if (targetPermanent.TopCard == null) yield break;
        if (activateClass == null) yield break;
        if (activateClass.EffectSourceCard == null) yield break;

        Player owner = targetPermanent.TopCard.Owner;
        SelectCardEffect.Root root = SelectCardEffect.Root.Execution;
        bool ignoreDigivolutionRequirement = !ignoreRequirements.Equals(IgnoreRequirement.None) || ignoreDigivolutionRequirementFixedCost >= 0;//  ignoreDigivolutionRequirementFixedCost >= 0 || ignoreRequirements;

        int fixedCost = -1;

        if (fixedCostTuple != null)
        {
            fixedCost = fixedCostTuple.Value.fixedCost;
        }

        if (ignoreDigivolutionRequirement)
        {
            fixedCost = ignoreDigivolutionRequirementFixedCost;
        }
        bool CanSelectCardCondition(CardSource cardSource)
        {
            if (cardSource.IsDigimon)
            {
                if (cardCondition == null || cardCondition(cardSource))
                {
                    if (!cardSource.CanNotEvolve(targetPermanent))
                    {
                        if (cardSource.CanPlayCardTargetFrame(
                            frame: targetPermanent.PermanentFrame,
                            PayCost: payCost,
                            cardEffect: activateClass,
                            root: root,
                            fixedCost: fixedCost,
                            ignore: ignoreRequirements))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        #region reduce cost

        Func<EffectTiming, ICardEffect> getChangeCostEffect = null;

        if (reduceCostTuple != null)
        {
            bool PermanentCondition(Permanent permanent)
            {
                return permanent == targetPermanent;
            }

            bool CardCondition(CardSource cardSource)
            {
                if (cardSource != null)
                {
                    if (cardSource.Owner == owner)
                    {
                        if (reduceCostTuple.Value.reduceCostCardCondition == null || reduceCostTuple.Value.reduceCostCardCondition(cardSource))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            getChangeCostEffect = ChangeDigivolutionCostPlayerEffect(
                permanentCondition: PermanentCondition,
                cardCondition: CardCondition,
                rootCondition: null,
                changeValue: -reduceCostTuple.Value.reduceCost,
                activateClass: activateClass,
                setFixedCost: false);

            if (getChangeCostEffect != null)
            {
                AddEffectToPlayer(
                    effectDuration: EffectDuration.UntilCalculateFixedCost,
                    card: targetPermanent.TopCard,
                    cardEffect: null,
                    timing: EffectTiming.None,
                    getCardEffect: getChangeCostEffect);
            }
        }

        #endregion

        #region set fixed cost

        Func<EffectTiming, ICardEffect> getFixedCostEffect = null;

        if (fixedCostTuple != null)
        {
            bool PermanentCondition(Permanent permanent)
            {
                return permanent == targetPermanent;
            }

            bool CardCondition(CardSource cardSource)
            {
                if (cardSource != null)
                {
                    if (cardSource.Owner == owner)
                    {
                        if (fixedCostTuple.Value.fixedCostCardCondition == null || fixedCostTuple.Value.fixedCostCardCondition(cardSource))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            getFixedCostEffect = ChangeDigivolutionCostPlayerEffect(
                permanentCondition: PermanentCondition,
                cardCondition: CardCondition,
                rootCondition: null,
                changeValue: fixedCost,
                activateClass: activateClass,
                setFixedCost: true);

            if (getChangeCostEffect != null)
            {
                AddEffectToPlayer(
                    effectDuration: EffectDuration.UntilCalculateFixedCost,
                    card: targetPermanent.TopCard,
                    cardEffect: null,
                    timing: EffectTiming.None,
                    getCardEffect: getFixedCostEffect);
            }
        }

        #endregion

        #region ignore digivolution requirement

        Func<EffectTiming, ICardEffect> getIgnoreDigivolutionRequirementEffect = null;

        if (ignoreDigivolutionRequirement)
        {
            bool PermanentCondition(Permanent permanent)
            {
                return permanent == targetPermanent;
            }

            bool CardCondition(CardSource cardSource)
            {
                if (cardSource == null)
                {
                    return false;
                }

                if (cardSource.Owner != owner)
                {
                    return false;
                }

                if (cardCondition == null || cardCondition(cardSource))
                {
                    return true;
                }

                return false;
            }

            getIgnoreDigivolutionRequirementEffect = GainIgnoreDigivolutionRequirementPlayerEffect(
                permanentCondition: PermanentCondition,
                cardCondition: CardCondition,
                ignoreDigivolutionRequirement: ignoreDigivolutionRequirement,
                digivolutionCost: fixedCost,
                activateClass: activateClass);

            if (getIgnoreDigivolutionRequirementEffect != null)
            {
                AddEffectToPlayer(
                effectDuration: EffectDuration.UntilCalculateFixedCost,
                card: targetPermanent.TopCard,
                cardEffect: null,
                timing: EffectTiming.None,
                getCardEffect: getIgnoreDigivolutionRequirementEffect);
            }
        }

        #endregion

        List<CardSource> selectedCards = new List<CardSource>();

        IEnumerator SelectCardCoroutine(CardSource cardSource)
        {
            selectedCards.Add(cardSource);

            yield return null;
        }

        if (owner.ExecutingCards.Count(CanSelectCardCondition) > 1)
        {
            int maxCount = 1;

            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

            selectCardEffect.SetUp(
                        canTargetCondition: CanSelectCardCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 card to digivolve.",
                        maxCount: maxCount,
                        canEndNotMax: false,
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Execution,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: owner,
                        cardEffect: activateClass);

            selectCardEffect.SetUpCustomMessage("Select 1 card to digivolve.", "The opponent is selecting 1 card to digivolve.");
            selectCardEffect.SetUpCustomMessage_ShowCard("Selected Card");

            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
        }
        else
        {
            selectedCards.Add(activateClass.EffectSourceCard);
        }

        PlayCardClass playCardClass = new PlayCardClass(
            cardSources: selectedCards,
            hashtable: CardEffectHashtable(activateClass),
            payCost: payCost,
            targetPermanent: targetPermanent,
            isTapped: false,
            root: root,
            activateETB: true);

        playCardClass.SetIgnoreRequirements(ignoreRequirements);

        if (!ignoreDigivolutionRequirement && fixedCost >= 0) playCardClass.SetFixedCost(fixedCost);

        yield return ContinuousController.instance.StartCoroutine(playCardClass.PlayCard());

        #region release effect

        if (getChangeCostEffect != null) owner.UntilCalculateFixedCostEffect.Remove(getChangeCostEffect);

        #endregion

        #region release effect

        if (getFixedCostEffect != null) owner.UntilCalculateFixedCostEffect.Remove(getFixedCostEffect);

        #endregion

        #region release effect

        if (getIgnoreDigivolutionRequirementEffect != null) owner.UntilCalculateFixedCostEffect.Remove(getIgnoreDigivolutionRequirementEffect);

        #endregion

        if (selectedCards.Count >= 1)
        {
            if (IsDigivolvedByTheEffect(targetPermanent, selectedCards[0], activateClass))
            {
                if (successProcess != null)
                {
                    yield return ContinuousController.instance.StartCoroutine(successProcess);
                }
            }
        }
    }

    #endregion

    #region Get the effect given by EffectTiming

    public static Func<EffectTiming, ICardEffect> GetCardEffectByEffectTiming(EffectTiming timing, ICardEffect cardEffect) => (_timing) => _timing == timing ? cardEffect : null;

    #endregion

    #region Draw cards and trash cards

    public static IEnumerator DrawAndDiscardCards((Player drawPlayer, Player trashPlayer) player,
                                                  int drawAmount,
                                                  int trashAmount,
                                                  CardSource card,
                                                  ICardEffect activateClass,
                                                  Func<CardSource, bool> canTrashTargetCondition = null,
                                                  Func<List<CardSource>, CardSource, bool> canTargetCondition_ByPreSelecetedList = null,
                                                  Func<List<CardSource>, bool> canEndSelectCondition = null,
                                                  bool canNoSelect = false,
                                                  bool canEndNotMax = false,
                                                  bool isShowOpponent = true,
                                                  Func<List<CardSource>, IEnumerator> afterSelectPermanentCoroutine = null)
    {
        // Callback Setup
        Func<List<CardSource>, IEnumerator> AfterSelectCardCoroutine = afterSelectPermanentCoroutine ?? null;

        // Variable setup
        Func<CardSource, bool> targetTrashCondition = canTrashTargetCondition ?? (cs => true);

        yield return ContinuousController.instance.StartCoroutine(new DrawClass(player.drawPlayer, drawAmount, activateClass).Draw());
        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

        selectHandEffect.SetUp(
            selectPlayer: player.trashPlayer,
            canTargetCondition: targetTrashCondition,
            canTargetCondition_ByPreSelecetedList: canTargetCondition_ByPreSelecetedList,
            canEndSelectCondition: canEndSelectCondition,
            maxCount: trashAmount,
            canNoSelect: canNoSelect,
            canEndNotMax: canEndNotMax,
            isShowOpponent: isShowOpponent,
            selectCardCoroutine: null,
            afterSelectCardCoroutine: AfterSelectCardCoroutine,
            mode: SelectHandEffect.Mode.Discard,
            cardEffect: activateClass);

        yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());
    }

    #endregion
}