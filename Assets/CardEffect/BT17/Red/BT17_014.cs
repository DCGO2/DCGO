using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace DCGO.CardEffects.BT17
{
    public class BT17_014 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Hand - Main

            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Your [Takuya Kanbara] digivolves into this card", CanUseCondition,
                    card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[Hand] [Main] By placing 1 [Agunimon] and [BurningGreymon] from your trash under 1 of your [Takuya Kanbara]s, digivolve it into this card as if that card is a level 4 red Digimon for a digivolution cost of 3.";
                }

                bool CanSelectAgunimonCardCondition(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                        if (cardSource.Owner == card.Owner)
                        {
                            if (cardSource.EqualsCardName("Agunimon"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectBurningGreymonCardCondition(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                        if (cardSource.Owner == card.Owner)
                        {
                            if (cardSource.EqualsCardName("BurningGreymon"))
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
                                        if (permanent.TopCard.EqualsCardName("Takuya Kanbara"))
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
                                    CanSelectAgunimonCardCondition))
                            {
                                if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card,
                                        CanSelectBurningGreymonCardCondition))
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

                            selectPermanentEffect.SetUpCustomMessage("Select 1 [Takuya Kanbara].",
                                "The opponent is selecting 1 [Takuya Kanbara].");

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
                                            CanSelectAgunimonCardCondition(cardSource) ||
                                            CanSelectBurningGreymonCardCondition(cardSource)))
                                {
                                    bool digivolve = false;

                                    maxCount = 2;

                                    if (card.Owner.TrashCards.Count((cardSource) =>
                                            CanSelectAgunimonCardCondition(cardSource) ||
                                            CanSelectBurningGreymonCardCondition(cardSource)) <
                                        maxCount)
                                    {
                                        maxCount = card.Owner.TrashCards.Count((cardSource) =>
                                            CanSelectAgunimonCardCondition(cardSource) ||
                                            CanSelectBurningGreymonCardCondition(cardSource));
                                    }

                                    List<CardSource> selectedCards = new List<CardSource>();

                                    SelectCardEffect selectCardEffect =
                                        GManager.instance.GetComponent<SelectCardEffect>();

                                    selectCardEffect.SetUp(
                                        canTargetCondition: (cardSource) =>
                                            CanSelectAgunimonCardCondition(cardSource) ||
                                            CanSelectBurningGreymonCardCondition(cardSource),
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
                                        if (cardSources.Count(CanSelectAgunimonCardCondition) >= 1)
                                        {
                                            if (CanSelectAgunimonCardCondition(cardSource))
                                            {
                                                return false;
                                            }
                                        }

                                        if (cardSources.Count(CanSelectBurningGreymonCardCondition) >= 1)
                                        {
                                            if (CanSelectBurningGreymonCardCondition(cardSource))
                                            {
                                                return false;
                                            }
                                        }

                                        return true;
                                    }

                                    bool CanEndSelectCondition(List<CardSource> cardSources)
                                    {
                                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card,
                                                CanSelectAgunimonCardCondition))
                                        {
                                            if (cardSources.Count(CanSelectAgunimonCardCondition) == 0)
                                            {
                                                return false;
                                            }
                                        }

                                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card,
                                                CanSelectBurningGreymonCardCondition))
                                        {
                                            if (cardSources.Count(CanSelectBurningGreymonCardCondition) == 0)
                                            {
                                                return false;
                                            }
                                        }

                                        if (cardSources.Count(CanSelectAgunimonCardCondition) >= 2)
                                        {
                                            return false;
                                        }

                                        if (cardSources.Count(CanSelectBurningGreymonCardCondition) >= 2)
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
                                        changeCardColorClass.SetUpICardEffect($"Treated as red",
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
                                                successProcess:null,
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
                activateClass.SetUpICardEffect(
                    "Delete 1 Digimon with 6000 DP or less",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false,
                    EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Digivolving] Delete 1 of your opponent's Digimon with 6000 DP or less.";
                }

                bool CanSelectOpponentPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.DP <= card.Owner.MaxDP_DeleteEffect(6000, activateClass))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentPermanentCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentPermanentCondition))
                    {
                        int enemyCount = Math.Min(1,
                            CardEffectCommons.MatchConditionPermanentCount(CanSelectOpponentPermanentCondition));

                        SelectPermanentEffect selectEnemyEffect =
                            GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectEnemyEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectOpponentPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: enemyCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Destroy,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectEnemyEffect.Activate());
                    }
                }
            }

            #endregion

            #region Your Turn - ESS

            if (timing == EffectTiming.None)
            {
                DisableEffectClass invalidationClass = new DisableEffectClass();
                invalidationClass.SetUpICardEffect("Ignore Security effects on Option cards", CanUseCondition, card);
                invalidationClass.SetUpDisableEffectClass(DisableCondition: InvalidateCondition);
                invalidationClass.SetIsInheritedEffect(true);

                cardEffects.Add(invalidationClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                bool InvalidateCondition(ICardEffect cardEffect)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (card.PermanentOfThisCard().TopCard.ContainsTraits("Hybrid") ||
                                card.PermanentOfThisCard().TopCard.ContainsTraits("TenWarriors") ||
                                card.PermanentOfThisCard().TopCard.ContainsTraits("Ten Warriors"))
                            {
                                if (cardEffect != null)
                                {
                                    if (cardEffect.EffectSourceCard != null)
                                    {
                                        if (cardEffect.EffectSourceCard.IsOption)
                                        {
                                            if (cardEffect.IsSecurityEffect)
                                            {
                                                if (GManager.instance.attackProcess.AttackingPermanent == card.PermanentOfThisCard())
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

                    return false;
                }
            }

            #endregion

            return cardEffects;
        }
    }
}