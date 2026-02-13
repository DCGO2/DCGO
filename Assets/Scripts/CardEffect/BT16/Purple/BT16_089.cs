using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT16
{
    public class BT16_089 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Your Turn

            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete this Tamer to reduce Play Cost", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetHashString("DeleteTamer_BT13_103");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] When [Arukenimon] or [Mummymon] would be played from your hand, by deleting 1 of this Tamer, reduce the play cost by 3.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent == card.PermanentOfThisCard())
                        {
                            if (!permanent.TopCard.CanNotBeAffected(activateClass))
                            {
                                if (permanent.CanBeDestroyedBySkill(activateClass))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CardCondition(CardSource cardSource)
                {
                    if (cardSource.Owner == card.Owner)
                    {
                        if (cardSource.CardNames.Contains("Mummymon") || cardSource.CardNames.Contains("Arukenimon"))
                        {
                            if (CardEffectCommons.IsExistOnHand(cardSource))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, CardCondition))
                    {
                        if (card.Owner.HandCards.Count(CardCondition) >= 1)
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
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
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
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Tamer to delete.", "The opponent is selecting 1 Tamer to delete.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(
                                targetPermanents: new List<Permanent>() { permanent },
                                activateClass: activateClass,
                                successProcess: permanents => SuccessProcess(),
                                failureProcess: null));

                            IEnumerator SuccessProcess()
                            {
                                int reduceCost = 3;

                                if (reduceCost <= 0) yield break;

                                ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                                ChangeCostClass changeCostClass = new ChangeCostClass();
                                changeCostClass.SetUpICardEffect($"Play Cost -{reduceCost}", CanUseCondition1, card);
                                changeCostClass.SetUpChangeCostClass(
                                    changeCostFunc: ChangeCost,
                                    cardSourceCondition: CardSourceCondition,
                                    rootCondition: RootCondition,
                                    isUpDown: isUpDown,
                                    isCheckAvailability: () => false,
                                    isChangePayingCost: () => true);
                                card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(_hashtable));

                                bool CanUseCondition1(Hashtable hashtable)
                                {
                                    return true;
                                }

                                int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                                {
                                    if (CardSourceCondition(cardSource))
                                    {
                                        if (RootCondition(root))
                                        {
                                            if (PermanentsCondition(targetPermanents))
                                            {
                                                Cost -= reduceCost;
                                            }
                                        }
                                    }

                                    return Cost;
                                }

                                bool PermanentsCondition(List<Permanent> targetPermanents)
                                {
                                    if (targetPermanents == null)
                                    {
                                        return true;
                                    }
                                    else
                                    {
                                        if (targetPermanents.Count((targetPermanent) => targetPermanent != null) == 0)
                                        {
                                            return true;
                                        }
                                    }

                                    return false;
                                }

                                bool CardSourceCondition(CardSource cardSource)
                                {
                                    return cardSource != null;
                                }

                                bool RootCondition(SelectCardEffect.Root root)
                                {
                                    return true;
                                }

                                bool isUpDown()
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region On Deletion

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 Digimon from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Deletion] You may play 1 level 5 or lower Digimon card with [Myotismon] in it's text from your trash without paying the cost.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.Level <= 5)
                        {
                            if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                            {
                                if (cardSource.HasLevel)
                                {
                                    if (cardSource.HasText("Myotismon"))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanActivateOnDeletion(card))
                    {
                        if (card.Owner.TrashCards.Count >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, (cardSource) => CanSelectCardCondition(cardSource)))
                    {
                        int maxCount = Math.Min(1, card.Owner.TrashCards.Count((cardSource) => CanSelectCardCondition(cardSource)));

                        List<CardSource> selectedCards = new List<CardSource>();

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 Digimon to play.",
                                    maxCount: maxCount,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Trash,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                        selectCardEffect.SetUpCustomMessage("Select 1 Digimon to play.", "The opponent is selecting 1 Digimon to play.");
                        selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                        yield return StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass, payCost: false, isTapped: false, root: SelectCardEffect.Root.Trash, activateETB: true));

                        foreach (CardSource selectedCard in selectedCards)
                        {
                            if (CardEffectCommons.IsExistOnBattleArea(selectedCard))
                            {
                                Permanent selectedPermanent = selectedCard.PermanentOfThisCard();

                                if (selectedPermanent != null)
                                {
                                    ActivateClass activateClass1 = new ActivateClass();
                                    activateClass1.SetUpICardEffect("Delete the Digimon", CanUseCondition1, card);
                                    activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, "");
                                    card.Owner.UntilOpponentTurnEndEffects.Add(GetCardEffect);

                                    bool CanUseCondition1(Hashtable hashtable)
                                    {
                                        if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                                        {
                                            if (CardEffectCommons.IsOpponentTurn(card))
                                            {
                                                return true;
                                            }
                                        }

                                        return false;
                                    }

                                    bool CanActivateCondition1(Hashtable hashtable)
                                    {
                                        if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                                        {
                                            if (selectedPermanent.CanBeDestroyedBySkill(activateClass1))
                                            {
                                                if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass1))
                                                {
                                                    return true;
                                                }
                                            }
                                        }

                                        return false;
                                    }

                                    IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(new List<Permanent>() { selectedPermanent }, CardEffectCommons.CardEffectHashtable(activateClass1)).Destroy());
                                    }

                                    ICardEffect GetCardEffect(EffectTiming _timing)
                                    {
                                        if (_timing == EffectTiming.OnEndTurn)
                                        {
                                            return activateClass1;
                                        }

                                        return null;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region Security Skill

            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }

            #endregion

            return cardEffects;
        }
    }
}