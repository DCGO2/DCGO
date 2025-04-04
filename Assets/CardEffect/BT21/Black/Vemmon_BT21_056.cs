using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT21
{
    public class Vemmon_BT21_056 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 card with [Vemmon] in text from hand to return 1 non-Digi-Egg card with [Vemmon] in text from trash to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] By trashing 1 card with [Vemmon] in its text from your hand, you may return 1 non-Digi-Egg card with [Vemmon] in its text from your trash to the hand.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                        if (cardSource.Owner == card.Owner)
                        {
                            if (cardSource.HasText("Vemmon") && !cardSource.IsDigiEgg)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.HandCards.Count >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (isExistOnField(card))
                    {
                        if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                        {
                            if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                            {
                                bool discarded = false;

                                int discardCount = 1;

                                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                                selectHandEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectCardCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: discardCount,
                                    canNoSelect: true,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    selectCardCoroutine: null,
                                    afterSelectCardCoroutine: AfterSelectCardCoroutine,
                                    mode: SelectHandEffect.Mode.Discard,
                                    cardEffect: activateClass);

                                yield return StartCoroutine(selectHandEffect.Activate());

                                IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                                {
                                    if (cardSources.Count >= 1)
                                    {
                                        discarded = true;

                                        yield return null;
                                    }
                                }

                                if (discarded)
                                {
                                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                                    {
                                        int maxCount = Math.Min(1, card.Owner.TrashCards.Count(CanSelectCardCondition));

                                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                        selectCardEffect.SetUp(
                                            canTargetCondition: CanSelectCardCondition,
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
                                            root: SelectCardEffect.Root.Trash,
                                            customRootCardList: null,
                                            canLookReverseCard: true,
                                            selectPlayer: card.Owner,
                                            cardEffect: activateClass);

                                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                                    }
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region Your Turn Inherited

            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolution Cost -1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("DigivolutionCost-1_BT21_056");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] [Once Per Turn] When this Digimon would digivolve into a Digimon card with [Vemmon] in its text, reduce the digivolution cost by 1.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                        permanent == card.PermanentOfThisCard();
                }

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon &&
                        cardSource.HasText("Vemmon");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                        CardEffectCommons.IsOwnerTurn(card) &&
                        CardEffectCommons.CanTriggerWhenPermanentWouldDigivolve(hashtable, PermanentCondition, CardCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (isExistOnField(card))
                    {
                        Hashtable hashtable = new Hashtable();
                        hashtable.Add("CardEffect", activateClass);

                        ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                        ChangeCostClass changeCostClass = new ChangeCostClass();
                        changeCostClass.SetUpICardEffect("Digivolution Cost -1", CanUseCondition1, card);
                        changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                        card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(_hashtable));

                        bool CanUseCondition1(Hashtable hashtable)
                        {
                            return true;
                        }

                        int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                        {
                            if (CardSourceCondition(cardSource) && RootCondition(root) && PermanentsCondition(targetPermanents))
                            {
                                Cost -= 1;
                            }

                            return Cost;
                        }

                        bool PermanentsCondition(List<Permanent> targetPermanents)
                        {
                            return targetPermanents != null &&
                                targetPermanents.Count(PermanentCondition) >= 1;
                        }

                        bool PermanentCondition(Permanent targetPermanent)
                        {
                            return targetPermanent.TopCard != null &&
                                targetPermanent.TopCard.Owner == card.Owner &&
                                targetPermanent.TopCard.Owner.GetBattleAreaPermanents().Contains(targetPermanent);
                        }

                        bool CardSourceCondition(CardSource cardSource)
                        {
                            return cardSource != null &&
                                cardSource.Owner == card.Owner &&
                                cardSource.HasText("Vemmon");
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

            #endregion

            return cardEffects;
        }
    }
}