using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCGO.CardEffects.EX1
{
    public class EX1_071 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                IgnoreColorConditionClass ignoreColorConditionClass = new IgnoreColorConditionClass();
                ignoreColorConditionClass.SetUpICardEffect("Ignore color requirements", CanUseCondition, card);
                ignoreColorConditionClass.SetUpIgnoreColorConditionClass(cardCondition: CardCondition);
                cardEffects.Add(ignoreColorConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return card.Owner.GetBattleAreaPermanents().Some(permanet => permanet.IsTamer);
                }

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }
            }

            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] The next time one of your Digimon digivolves this turn, you may trash 1 Digimon card in your hand of the same color as the digivolving Digimon to reduce the memory cost of the digivolution by 4.";
                }
                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                    yield return new WaitForSeconds(0.2f);

                    ActivateClass activateClass1 = new ActivateClass();
                    Func<EffectTiming, ICardEffect> getCardEffect = GetCardEffect;
                    activateClass1.SetUpICardEffect("Digivolution Cost -4", CanUseCondition1, card);
                    activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, true, EffectDiscription1());
                    CardEffectCommons.AddEffectToPlayer(effectDuration: EffectDuration.UntilEachTurnEnd, card: card, cardEffect: null, timing: EffectTiming.None, getCardEffect: getCardEffect);

                    ActivateClass activateClass2 = new ActivateClass();
                    Func<EffectTiming, ICardEffect> getCardEffect1 = GetCardEffect1;
                    activateClass2.SetUpICardEffect("Remove Effect", CanUseCondition1, card);
                    activateClass2.SetUpActivateClass(null, ActivateCoroutine2, -1, false, "");
                    activateClass2.SetIsBackgroundProcess(true);
                    CardEffectCommons.AddEffectToPlayer(effectDuration: EffectDuration.UntilEachTurnEnd, card: card, cardEffect: null, timing: EffectTiming.None, getCardEffect: getCardEffect1);

                    string EffectDiscription1()
                    {
                        return "You may trash 1 Digimon card in your hand of the same color as the digivolving Digimon to reduce the memory cost of the digivolution by 4.";
                    }

                    bool PermanentCondition(Permanent targetPermanent)
                    {
                        return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(targetPermanent, card);
                    }

                    bool CanUseCondition1(Hashtable hashtable)
                    {
                        if (CardEffectCommons.CanTriggerWhenPermanentWouldDigivolve(
                            hashtable: hashtable,
                            permanentCondition: PermanentCondition,
                            cardCondition: null))
                        {
                            return true;
                        }

                        return false;
                    }

                    bool CanActivateCondition1(Hashtable hashtable)
                    {
                        List<Permanent> permanents = CardEffectCommons.GetPermanentsFromHashtable(hashtable);

                        if (permanents != null)
                        {
                            bool CanSelectCardCondition(CardSource cardSource)
                            {
                                if (cardSource.IsDigimon)
                                {
                                    List<CardColor> permanentColors = permanents
                                    .Map(permanent => permanent.TopCard.CardColors)
                                    .Flat();

                                    if (cardSource.CardColors.Some((cardColor) => permanentColors.Contains(cardColor)))
                                    {
                                        return true;
                                    }
                                }

                                return false;
                            }

                            if (card.Owner.HandCards.Some(CanSelectCardCondition))
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                    {
                        List<Permanent> permanents = CardEffectCommons.GetPermanentsFromHashtable(_hashtable1);

                        if (permanents != null)
                        {
                            bool CanSelectCardCondition(CardSource cardSource)
                            {
                                if (cardSource.IsDigimon)
                                {
                                    List<CardColor> permanentColors = permanents
                                    .Map(permanent => permanent.TopCard.CardColors)
                                    .Flat();

                                    if (cardSource.CardColors.Some((cardColor) => permanentColors.Contains(cardColor)))
                                    {
                                        return true;
                                    }
                                }

                                return false;
                            }

                            if (card.Owner.HandCards.Some(CanSelectCardCondition))
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
                                    ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                                    ChangeCostClass changeCostClass = new ChangeCostClass();
                                    changeCostClass.SetUpICardEffect("Digivolution Cost -4", CanUseCondition1, card);
                                    changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
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
                                                    Cost -= 4;
                                                }
                                            }
                                        }

                                        return Cost;
                                    }

                                    bool PermanentsCondition(List<Permanent> targetPermanents)
                                    {
                                        if (targetPermanents != null)
                                        {
                                            if (targetPermanents.Count(PermanentCondition) >= 1)
                                            {
                                                return true;
                                            }
                                        }

                                        return false;
                                    }

                                    bool PermanentCondition(Permanent targetPermanent)
                                    {
                                        return CardEffectCommons.IsPermanentExistsOnBattleArea(targetPermanent);
                                    }

                                    bool CardSourceCondition(CardSource cardSource)
                                    {
                                        return true;
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

                    ICardEffect GetCardEffect(EffectTiming _timing)
                    {
                        if (_timing == EffectTiming.BeforePayCost)
                        {
                            return activateClass1;
                        }

                        return null;
                    }

                    IEnumerator ActivateCoroutine2(Hashtable _hashtable1)
                    {
                        if (CardEffectCommons.CanTriggerWhenPermanentWouldDigivolve(
                            hashtable: _hashtable1,
                            permanentCondition: PermanentCondition,
                            cardCondition: null))
                        {
                            card.Owner.UntilEachTurnEndEffects.Remove(getCardEffect);
                            card.Owner.UntilEachTurnEndEffects.Remove(getCardEffect1);
                            yield return null;
                        }
                    }

                    ICardEffect GetCardEffect1(EffectTiming _timing)
                    {
                        if (_timing == EffectTiming.AfterPayCost)
                        {
                            return activateClass2;
                        }

                        return null;
                    }
                }
            }

            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect($"Add this card to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Security] Add this card to its owner's hand.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.AddThisCardToHand(card, activateClass));
                }
            }

            return cardEffects;
        }
    }
}