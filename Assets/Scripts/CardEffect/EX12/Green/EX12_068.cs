using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Ruli Tsukiyono
namespace DCGO.CardEffects.EX12
{
    public class EX12_068 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Your Turn
            if (timing == EffectTiming.OnStartTurn)
            {
                cardEffects.Add(CardEffectFactory.SetMemoryTo3TamerEffect(card));
            }
            #endregion

            #region Your Turn
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend to digivolve or use an option", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Your Turn] When one of your Digimon with [Angoramon] in its text or the [NSp] trait attacks, by suspending this Tamer, activate 1 of the effects below:\r\n- It my digivolve into a level 6 or lower card with [Angoramon] in its text or the [NSp] trait in the hand with the cost reduced by 1.\r\n- You may use 1 Option card with [Angoramon] in its text or the [NSp] trait from your hand with the cost reduced by 2.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, PermanentCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && (permanent.TopCard.HasText("Angoramon")
                            || permanent.TopCard.EqualsTraits("NSp"));
                }

                bool CanSelectDigivolveCondition(CardSource cardSource)
                {
                    return cardSource.HasLevel
                        && cardSource.Level <= 6
                        && (cardSource.HasText("Angoramon")
                            || cardSource.EqualsTraits("NSp"));
                }

                bool CanSelectOptionCondition(CardSource cardSource)
                {
                    return cardSource.IsOption
                        && (cardSource.HasText("Angoramon")
                            || cardSource.EqualsTraits("NSp"));
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool HasDigivolveCondition(CardSource cardSource)
                    {
                        List<int> costList = cardSource.CostList(GManager.instance.attackProcess.AttackingPermanent, false, false);

                        return cardSource.HasLevel
                            && cardSource.Level <= 6
                            && (cardSource.HasText("Angoramon")
                                || cardSource.EqualsTraits("NSp"))
                            && costList.Count > 0 && card.Owner.MaxMemoryCost >= costList.Min() - 1;
                    }

                    List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();
                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, HasDigivolveCondition))
                    {
                        selectionElements.Add(new(message: "Attacking Digimon Digivolves", value: 1, spriteIndex: 0));
                    }
                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectOptionCondition))
                    {
                        selectionElements.Add(new(message: "Use an Option", value: 2, spriteIndex: 0));
                    }
                    selectionElements.Add(new(message: "Suspend only", value: 3, spriteIndex: 1));
                    selectionElements.Add(new(message: "Do not use", value: 4, spriteIndex: 1));


                    string selectPlayerMessage = "Which effect will you activate?";
                    string notSelectPlayerMessage = "The opponent is choosing which effect to activate.";

                    GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    if (GManager.instance.userSelectionManager.SelectedIntValue != 4)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                        if (GManager.instance.userSelectionManager.SelectedIntValue != 3)
                        {
                            if (GManager.instance.userSelectionManager.SelectedIntValue == 1)
                            {
                                yield return ContinuousController.instance.StartCoroutine(
                                    CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                        targetPermanent: GManager.instance.attackProcess.AttackingPermanent,
                                        cardCondition: CanSelectDigivolveCondition,
                                        payCost: true,
                                        reduceCostTuple: (reduceCost: 1, reduceCostCardCondition: null),
                                        fixedCostTuple: null,
                                        ignoreDigivolutionRequirementFixedCost: -1,
                                        isHand: true,
                                        activateClass: activateClass,
                                        successProcess: null));
                            }
                            else
                            {
                                CardSource selectedCard = null;

                                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                                selectHandEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectOptionCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: 1,
                                    canNoSelect: true,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    mode: SelectHandEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectHandEffect.SetUpCustomMessage("Select 1 card to use.", "The opponent is selecting 1 card to use.");
                                selectHandEffect.SetUpCustomMessage_ShowCard("Used Card");

                                IEnumerator SelectCardCoroutine(CardSource cardSource)
                                {
                                    selectedCard = cardSource;
                                    yield return null;
                                }

                                yield return ContinuousController.instance.StartCoroutine(selectHandEffect.Activate());

                                if (selectedCard != null)
                                {
                                    #region reduce use cost
                                    ChangeCostClass changeCostClass = new ChangeCostClass();
                                    changeCostClass.SetUpICardEffect($"Use Cost -2", CanUseCondition1, card);
                                    changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: PlayOrUseCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                                    Func<EffectTiming, ICardEffect> getCardEffect = GetCardEffect;
                                    card.Owner.UntilCalculateFixedCostEffect.Add(getCardEffect);

                                    ICardEffect GetCardEffect(EffectTiming _timing)
                                    {
                                        if (_timing == EffectTiming.None)
                                        {
                                            return changeCostClass;
                                        }

                                        return null;
                                    }

                                    bool CanUseCondition1(Hashtable hashtable)
                                    {
                                        return true;
                                    }

                                    bool PlayOrUseCondition(CardSource cardSource)
                                    {
                                        return true;
                                    }

                                    int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                                    {
                                        if (PlayOrUseCondition(cardSource)
                                       && RootCondition(root)
                                       && PermanentsCondition(targetPermanents))
                                        {
                                            Cost -= 2;
                                        }

                                        return Cost;
                                    }

                                    bool PermanentsCondition(List<Permanent> targetPermanents)
                                    {
                                        return targetPermanents == null
                                                || targetPermanents.Count((targetPermanent) => targetPermanent != null) == 0;
                                    }

                                    bool RootCondition(SelectCardEffect.Root root)
                                    {
                                        return true;
                                    }

                                    bool isUpDown()
                                    {
                                        return true;
                                    }
                                    #endregion

                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayOptionCards(
                                        cardSources: new List<CardSource> { selectedCard },
                                        activateClass: activateClass,
                                        payCost: true,
                                        root: SelectCardEffect.Root.Hand));

                                    #region release effect
                                    card.Owner.UntilCalculateFixedCostEffect.Remove(getCardEffect);
                                    #endregion
                                }
                            }
                        }
                    }
                }
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }
            #endregion

            return cardEffects;
        }
    }
}
