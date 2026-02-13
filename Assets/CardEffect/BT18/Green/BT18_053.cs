using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT18
{
    public class BT18_053 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.IsLevel4 && targetPermanent.TopCard.EqualsTraits("Hybrid")
                                                            && (targetPermanent.TopCard.CardColors.Contains(CardColor.Green) ||
                                                                targetPermanent.TopCard.CardColors.Contains(CardColor.Red));
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }

            #endregion

            #region Hand - Main

            if (timing == EffectTiming.OnDeclaration)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Your green or red tamer digivolves into this card", CanUseCondition,
                    card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[Hand] [Main] By placing 1 [Kazemon] and [Zephyrmon] from your trash under 1 of your green or red Tamers, that Tamer digivolves into this card for digivolution cost of 3, ignoring its digivolution requirements.";
                }

                bool CanSelectHumanSpiritCardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsCardName("Kazemon");
                }

                bool CanSelectBeastSpiritCardCondition(CardSource cardSource)
                {
                    return cardSource.EqualsCardName("Zephyrmon");
                }

                bool CanSelectTamerPermanentCondition(Permanent permanent)
                {
                    return permanent != null && CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card) &&
                           permanent.IsTamer &&
                           (permanent.TopCard.CardColors.Contains(CardColor.Green) || permanent.TopCard.CardColors.Contains(CardColor.Red));
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return card.Owner.HandCards.Contains(card) &&
                           CardEffectCommons.HasMatchConditionPermanent(CanSelectTamerPermanentCondition) &&
                           CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectHumanSpiritCardCondition) &&
                           CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectBeastSpiritCardCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectTamerPermanentCondition))
                    {
                        Permanent selectedPermanent = null;

                        SelectPermanentEffect selectPermanentEffect =
                            GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectTamerPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 tamer to digivolve.",
                            "The opponent is selecting 1 tamer to digivolve.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;

                            yield return null;
                        }

                        if (selectedPermanent != null &&
                            CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectHumanSpiritCardCondition) &&
                            CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectBeastSpiritCardCondition))
                        {
                            bool digivolve = false;


                            List<CardSource> selectedCards = new List<CardSource>();

                            SelectCardEffect selectCardEffect =
                                GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: (cardSource) =>
                                    CanSelectHumanSpiritCardCondition(cardSource) ||
                                    CanSelectBeastSpiritCardCondition(cardSource),
                                canTargetCondition_ByPreSelecetedList: CanTargetConditionByPreSelectedList,
                                canEndSelectCondition: CanEndSelectCondition,
                                canNoSelect: () => false,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message:
                                "Select cards to place on the bottom of Digivolution cards\n(cards will be placed in the digivolution cards so that cards with lower numbers are on top).",
                                maxCount: 2,
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
                                if (cardSources.Count(CanSelectHumanSpiritCardCondition) >= 1)
                                {
                                    if (CanSelectHumanSpiritCardCondition(cardSource))
                                    {
                                        return false;
                                    }
                                }

                                if (cardSources.Count(CanSelectBeastSpiritCardCondition) >= 1)
                                {
                                    if (CanSelectBeastSpiritCardCondition(cardSource))
                                    {
                                        return false;
                                    }
                                }

                                return true;
                            }

                            bool CanEndSelectCondition(List<CardSource> cardSources)
                            {
                                if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card,
                                        CanSelectHumanSpiritCardCondition))
                                {
                                    if (cardSources.Count(CanSelectHumanSpiritCardCondition) == 0)
                                    {
                                        return false;
                                    }
                                }

                                if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card,
                                        CanSelectBeastSpiritCardCondition))
                                {
                                    if (cardSources.Count(CanSelectBeastSpiritCardCondition) == 0)
                                    {
                                        return false;
                                    }
                                }

                                if (cardSources.Count(CanSelectHumanSpiritCardCondition) >= 2)
                                {
                                    return false;
                                }

                                if (cardSources.Count(CanSelectBeastSpiritCardCondition) >= 2)
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
                                if (!card.CanNotEvolve(selectedPermanent))
                                {
                                    yield return ContinuousController.instance.StartCoroutine(
                                    CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                        targetPermanent: selectedPermanent,
                                        cardCondition: source => source == card,
                                        payCost: true,
                                        reduceCostTuple: null,
                                        fixedCostTuple: null,
                                        ignoreDigivolutionRequirementFixedCost: 3,
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

                            }

                            IEnumerator DigivolvedFailed()
                            {
                                IDiscardHand discard = new IDiscardHand(card, hashtable);

                                discard.Discard();
                                yield return null;
                            }
                        }
                    }
                }
            }

            #endregion

            #region Raid

            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.RaidSelfEffect(
                    isInheritedEffect: false,
                    card: card,
                    condition: null)
                );
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend 1 opponent's Digimon or Tamers", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Digivolving] Suspend 1 of your opponent's Digimon or Tamers. It can't unsuspend until the end of their turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card) &&
                           permanent.CanSuspend && (permanent.IsDigimon || permanent.IsTamer);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Tap,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage(
                        "Select 1 Digimon or Tamer that will get suspended and unable to unsuspend.",
                        "The opponent is selecting 1 Digimon that will get suspended and unable to unsuspend.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                    {
                        foreach (Permanent permanent in permanents)
                        {
                            yield return ContinuousController.instance.StartCoroutine(
                                CardEffectCommons.GainCantUnsuspendUntilOpponentTurnEnd(
                                    targetPermanent: permanent,
                                    activateClass: activateClass
                                ));
                        }
                    }
                }
            }

            #endregion

            #region Your Turn - ESS

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(changeValue: 2000, isInheritedEffect: true,
                    card: card, condition: Condition));
            }

            #endregion

            return cardEffects;
        }
    }
}