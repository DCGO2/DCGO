using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace DCGO.CardEffects.BT17
{
    public class BT17_026 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Hand - Main

            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Your [Koji Minamoto] digivolves into this card", CanUseCondition,
                    card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[Hand] [Main] By placing 1 [Lobomon] and [KendoGarurumon] from your trash under 1 of your [Koji Minamoto]s, digivolve it into this card as if that card is a level 4 blue Digimon for a digivolution cost of 3.";
                }

                bool CanSelectLobomonCardCondition(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                        if (cardSource.Owner == card.Owner)
                        {
                            if (cardSource.EqualsCardName("Lobomon"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectKendoGarurumonCardCondition(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                        if (cardSource.Owner == card.Owner)
                        {
                            if (cardSource.EqualsCardName("KendoGarurumon"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (permanent != null)
                    {
                        if (permanent.TopCard != null)
                        {
                            if (permanent.TopCard.Owner == card.Owner)
                            {
                                if (permanent.TopCard.Owner.GetBattleAreaPermanents().Contains(permanent))
                                {
                                    if (!permanent.IsToken)
                                    {
                                        if (permanent.TopCard.EqualsCardName("Koji Minamoto") ||
                                            permanent.TopCard.EqualsCardName("KojiMinamoto"))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (card.Owner.HandCards.Contains(card))
                    {
                        if (card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                        {
                            if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card,
                                    CanSelectLobomonCardCondition))
                            {
                                if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card,
                                        CanSelectKendoGarurumonCardCondition))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.HandCards.Contains(card))
                    {
                        if (card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                        {
                            Permanent selectedPermanent = null;

                            int maxCount = Math.Min(1,
                                card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition));

                            SelectPermanentEffect selectPermanentEffect =
                                GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectPermanentCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 [Koji Minamoto].",
                                "The opponent is selecting 1 [Koji Minamoto].");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                selectedPermanent = permanent;
                                yield return null;
                            }

                            if (selectedPermanent != null)
                            {
                                if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card,
                                        (cardSource) =>
                                            CanSelectLobomonCardCondition(cardSource) ||
                                            CanSelectKendoGarurumonCardCondition(cardSource)))
                                {
                                    bool digivolve = false;

                                    maxCount = 2;

                                    if (card.Owner.TrashCards.Count((cardSource) =>
                                            CanSelectLobomonCardCondition(cardSource) ||
                                            CanSelectKendoGarurumonCardCondition(cardSource)) <
                                        maxCount)
                                    {
                                        maxCount = card.Owner.TrashCards.Count((cardSource) =>
                                            CanSelectLobomonCardCondition(cardSource) ||
                                            CanSelectKendoGarurumonCardCondition(cardSource));
                                    }

                                    List<CardSource> selectedCards = new List<CardSource>();

                                    SelectCardEffect selectCardEffect =
                                        GManager.instance.GetComponent<SelectCardEffect>();

                                    selectCardEffect.SetUp(
                                        canTargetCondition: (cardSource) =>
                                            CanSelectLobomonCardCondition(cardSource) ||
                                            CanSelectKendoGarurumonCardCondition(cardSource),
                                        canTargetCondition_ByPreSelecetedList: CanTargetConditionByPreSelectedList,
                                        canEndSelectCondition: CanEndSelectCondition,
                                        canNoSelect: () => false,
                                        selectCardCoroutine: SelectCardCoroutine,
                                        afterSelectCardCoroutine: null,
                                        message:
                                        "Select cards to place on the bottom of Digivolution cards\n(cards will be placed in the digivolution cards so that cards with lower numbers are on top).",
                                        maxCount: maxCount,
                                        canEndNotMax: false,
                                        isShowOpponent: true,
                                        mode: SelectCardEffect.Mode.Custom,
                                        root: SelectCardEffect.Root.Trash,
                                        customRootCardList: null,
                                        canLookReverseCard: true,
                                        selectPlayer: card.Owner,
                                        cardEffect: activateClass);

                                    yield return ContinuousController.instance.StartCoroutine(
                                        selectCardEffect.Activate());

                                    bool CanTargetConditionByPreSelectedList(List<CardSource> cardSources,
                                        CardSource cardSource)
                                    {
                                        if (cardSources.Count(CanSelectLobomonCardCondition) >= 1)
                                        {
                                            if (CanSelectLobomonCardCondition(cardSource))
                                            {
                                                return false;
                                            }
                                        }

                                        if (cardSources.Count(CanSelectKendoGarurumonCardCondition) >= 1)
                                        {
                                            if (CanSelectKendoGarurumonCardCondition(cardSource))
                                            {
                                                return false;
                                            }
                                        }

                                        return true;
                                    }

                                    bool CanEndSelectCondition(List<CardSource> cardSources)
                                    {
                                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card,
                                                CanSelectLobomonCardCondition))
                                        {
                                            if (cardSources.Count(CanSelectLobomonCardCondition) == 0)
                                            {
                                                return false;
                                            }
                                        }

                                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card,
                                                CanSelectKendoGarurumonCardCondition))
                                        {
                                            if (cardSources.Count(CanSelectKendoGarurumonCardCondition) == 0)
                                            {
                                                return false;
                                            }
                                        }

                                        if (cardSources.Count(CanSelectLobomonCardCondition) >= 2)
                                        {
                                            return false;
                                        }

                                        if (cardSources.Count(CanSelectKendoGarurumonCardCondition) >= 2)
                                        {
                                            return false;
                                        }

                                        return true;
                                    }

                                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                                    {
                                        selectedCards.Add(cardSource);
                                        yield return null;
                                    }

                                    if (selectedCards.Count >= 1)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(
                                            selectedPermanent.AddDigivolutionCardsBottom(selectedCards, activateClass));

                                        if (selectedCards.Count == 2)
                                        {
                                            digivolve = true;
                                        }
                                    }

                                    if (digivolve)
                                    {
                                        CardSource topCard = selectedPermanent.TopCard;

                                        ChangeCardColorClass changeCardColorClass = new ChangeCardColorClass();
                                        changeCardColorClass.SetUpICardEffect($"Treated as blue",
                                            CanUseChangeColorCondition,
                                            card);
                                        changeCardColorClass.SetUpChangeCardColorClass(
                                            ChangeCardColors: ChangeCardColors);
                                        changeCardColorClass.SetNotShowUI(true);

                                        bool CanUseChangeColorCondition(Hashtable ccHashtable)
                                        {
                                            if (selectedPermanent.TopCard != null)
                                            {
                                                if (card.Owner.GetBattleAreaPermanents().Contains(selectedPermanent))
                                                {
                                                    if (topCard == selectedPermanent.TopCard)
                                                    {
                                                        return true;
                                                    }
                                                }
                                            }

                                            return false;
                                        }

                                        List<CardColor> ChangeCardColors(CardSource cardSource,
                                            List<CardColor> cardColors)
                                        {
                                            if (cardSource == selectedPermanent.TopCard)
                                            {
                                                cardColors.Add(CardColor.Red);
                                            }

                                            return cardColors;
                                        }


                                        ChangePermanentLevelClass changePermanentLevelClass =
                                            new ChangePermanentLevelClass();
                                        changePermanentLevelClass.SetUpICardEffect($"Treated as level 4",
                                            CanUseChangeColorCondition, card);
                                        changePermanentLevelClass.SetUpChangePermanentLevelClass(GetLevel: GetLevel);
                                        changePermanentLevelClass.SetNotShowUI(true);

                                        int GetLevel(Permanent permanent, int level)
                                        {
                                            if (selectedPermanent.TopCard != null)
                                            {
                                                if (permanent == selectedPermanent)
                                                {
                                                    level = 4;
                                                }
                                            }

                                            return level;
                                        }


                                        TreatAsDigimonClass treatAsDigimonClass = new TreatAsDigimonClass();
                                        treatAsDigimonClass.SetUpICardEffect($"Treated as Digimon",
                                            CanUseChangeColorCondition,
                                            card);
                                        treatAsDigimonClass.SetUpTreatAsDigimonClass(
                                            permanentCondition: PermanentCondition);
                                        treatAsDigimonClass.SetNotShowUI(true);

                                        bool PermanentCondition(Permanent permanent)
                                        {
                                            if (selectedPermanent.TopCard != null)
                                            {
                                                if (permanent == selectedPermanent)
                                                {
                                                    return true;
                                                }
                                            }

                                            return false;
                                        }


                                        DontHaveDPClass dontHaveDPClass = new DontHaveDPClass();
                                        dontHaveDPClass.SetUpICardEffect("Don't have DP", CanUseChangeColorCondition,
                                            card);
                                        dontHaveDPClass.SetUpDontHaveDPClass(PermanentCondition: PermanentCondition);
                                        dontHaveDPClass.SetNotShowUI(true);


                                        List<Func<EffectTiming, ICardEffect>> getCardEffects =
                                            new List<Func<EffectTiming, ICardEffect>>()
                                            {
                                                _ => changeCardColorClass,
                                                _ => changePermanentLevelClass,
                                                _ => treatAsDigimonClass,
                                                _ => dontHaveDPClass,
                                            };

                                        foreach (Func<EffectTiming, ICardEffect> getCardEffect in getCardEffects)
                                        {
                                            card.Owner.PermanentEffects.Add(getCardEffect);
                                        }


                                        if (card.CanPlayCardTargetFrame(selectedPermanent.PermanentFrame, true,
                                                activateClass))
                                        {
                                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                                targetPermanent: selectedPermanent,
                                                cardCondition: source => source == card,
                                                payCost: true,
                                                reduceCostTuple: null,
                                                fixedCostTuple: (fixedCost: 3, fixedCostCardCondition: null),
                                                ignoreDigivolutionRequirementFixedCost: -1,
                                                isHand: true,
                                                activateClass: activateClass,
                                                successProcess: null,
                                                failedProcess: DigivolvedFailed(),
                                                isOptional: false));
                                        }
                                        else
                                        {
                                            yield return ContinuousController.instance.StartCoroutine("DigivolvedFailed");
                                        }

                                        IEnumerator DigivolvedFailed()
                                        {
                                            IDiscardHand discard = new IDiscardHand(card, hashtable);

                                            discard.Discard();
                                            yield return null;
                                        }

                                        foreach (Func<EffectTiming, ICardEffect> getCardEffect in getCardEffects)
                                        {
                                            card.Owner.PermanentEffects.Remove(getCardEffect);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return 1 digivolution card and 1 of your opponent's Digimon can't suspend", CanUseCondition,
                    card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Digivolving] By returning 1 card with the [Hybrid] trait from this Digimon's digivolution cards to the hand, 1 of your opponent's Digimon or Tamers can't suspend until the end of their turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanSelectSourceCardCondition(CardSource cardSource)
                {
                    return cardSource.CardTraits.Contains("Hybrid");
                }

                bool CanSelectOpponentPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card))
                    {
                        return permanent.IsDigimon || permanent.IsTamer;
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().DigivolutionCards.Count(CanSelectSourceCardCondition) >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = card.PermanentOfThisCard();

                    if (selectedPermanent.DigivolutionCards.Count(CanSelectSourceCardCondition) >= 1)
                    {
                        int maxCount = Math.Min(1, selectedPermanent.DigivolutionCards.Count(CanSelectSourceCardCondition));

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectSourceCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => false,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 card to add to your hand.",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.AddHand,
                            root: SelectCardEffect.Root.Custom,
                            customRootCardList: selectedPermanent.DigivolutionCards,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        yield return StartCoroutine(selectCardEffect.Activate());
                        
                        if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectOpponentPermanentCondition))
                        {
                            maxCount = Math.Min(1,
                                CardEffectCommons.MatchConditionPermanentCount(CanSelectOpponentPermanentCondition));

                            SelectPermanentEffect selectPermanentEffect =
                                GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition_ByPreSelecetedList: null,
                                canTargetCondition: CanSelectOpponentPermanentCondition,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon/Tamer that will be unable to suspend.",
                                "The opponent is selecting 1 Digimon/Tamer that will be unable to suspend.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                selectedPermanent = permanent;
                                CanNotSuspendClass canNotSuspendClass = new CanNotSuspendClass();
                                canNotSuspendClass.SetUpICardEffect("Can't Suspend", CanUseCanNotSuspendCondition, card);
                                canNotSuspendClass.SetUpCanNotSuspendClass(PermanentCondition: PermanentCanNotSuspendCondition);
                                selectedPermanent.UntilOwnerTurnEndEffects.Add(_ => canNotSuspendClass);

                                if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                {
                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance
                                        .GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));
                                }

                                bool CanUseCanNotSuspendCondition(Hashtable hashtableCanNotSuspend)
                                {
                                    if (selectedPermanent.TopCard != null)
                                    {
                                        if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                        {
                                            return true;
                                        }
                                    }

                                    return false;
                                }

                                bool PermanentCanNotSuspendCondition(Permanent permanentCanNotSuspend)
                                {
                                    if (permanentCanNotSuspend == selectedPermanent)
                                    {
                                        return true;
                                    }

                                    return false;
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region When Attacking - ESS

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return 1 of your opponent's Digimon to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Attacking] [Once Per Turn] If this Digimon has the [Hybrid]/[Ten Warriors] trait, return 1 of your opponent's level 4 or lower Digimon to the hand.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.Level <= 4)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().TopCard.ContainsTraits("Hybrid") ||
                            card.PermanentOfThisCard().TopCard.ContainsTraits("Ten Warriors") ||
                            card.PermanentOfThisCard().TopCard.ContainsTraits("TenWarriors"))
                        {
                                return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Bounce,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}
