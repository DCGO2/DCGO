using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DCGO.CardEffects.BT17
{
    public class BT17_023 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution

            // Koji Minamoto
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return true;
                }

                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Koji Minamoto");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false,
                    card: card, condition: Condition));
            }

            // Lobomon
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsCardName("Lobomon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 1, ignoreDigivolutionRequirement: false,
                    card: card, condition: null));
            }

            // Any yellow tamer
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    return card.Owner.HandCards.Contains(card);
                }

                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.IsTamer && targetPermanent.TopCard.CardColors.Contains(CardColor.Yellow);
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false,
                    card: card, condition: Condition));
            }

            #endregion

            #region As if tamer is a digimon
            if(timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("As if it were a level 3 digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, "");
                activateClass.SetIsBackgroundProcess(true);
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnHand(card))
                    {
                        bool PermanentCondition(Permanent targetPermanent)
                        {
                            return targetPermanent.TopCard.CardColors.Contains(CardColor.Red) 
                                && targetPermanent.IsTamer
                                && !targetPermanent.TopCard.EqualsCardName("Koji Minamoto");
                        }

                        bool CardCondition(CardSource cardSource)
                        {
                            return cardSource == card;
                        }

                        if (CardEffectCommons.CanTriggerWhenPermanentWouldDigivolve(hashtable, PermanentCondition, CardCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    Permanent selectedPermanent = CardEffectCommons.GetPermanentsFromHashtable(_hashtable)[0];

                    bool CanUseChangeCondition(Hashtable ccHashtable)
                    {
                        if (selectedPermanent.TopCard != null)
                        {
                            if (card.Owner.GetBattleAreaPermanents().Contains(selectedPermanent))
                            {
                                if (card == selectedPermanent.TopCard)
                                {
                                    return true;
                                }
                            }
                        }

                        return false;
                    }


                    ChangePermanentLevelClass changePermanentLevelClass = new ChangePermanentLevelClass();
                    changePermanentLevelClass.SetUpICardEffect($"Treated as level 3", CanUseChangeCondition, card);
                    changePermanentLevelClass.SetUpChangePermanentLevelClass(GetLevel: GetLevel);
                    changePermanentLevelClass.SetNotShowUI(true);

                    int GetLevel(Permanent permanent, int level)
                    {
                        if (selectedPermanent.TopCard != null)
                        {
                            if (permanent == selectedPermanent)
                            {
                                level = 3;
                            }
                        }

                        return level;
                    }


                    TreatAsDigimonClass treatAsDigimonClass = new TreatAsDigimonClass();
                    treatAsDigimonClass.SetUpICardEffect($"Treated as Digimon", CanUseChangeCondition, card);
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
                    dontHaveDPClass.SetUpICardEffect("Don't have DP", CanUseChangeCondition, card);
                    dontHaveDPClass.SetUpDontHaveDPClass(PermanentCondition: PermanentCondition);
                    dontHaveDPClass.SetNotShowUI(true);

                    List<Func<EffectTiming, ICardEffect>> getCardEffects =
                        new List<Func<EffectTiming, ICardEffect>>()
                        {
                                                    _ => changePermanentLevelClass,
                                                    _ => treatAsDigimonClass,
                                                    _ => dontHaveDPClass,
                        };

                    foreach (Func<EffectTiming, ICardEffect> getCardEffect in getCardEffects)
                    {
                    card.Owner.UntilAfterPlayEffect.Add(getCardEffect);
                    }

                    yield return null;
                }
            }
            #endregion

            #region When Attacking Draw 1

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false,
                    EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Attacking] [Draw 1].";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.LibraryCards.Count >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        new DrawClass(card.Owner, 1, activateClass).Draw());
                }
            }

            #endregion

            #region When Attacking

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("This Digimon digivolves", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true,
                    EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Attacking] This Digimon may digivolve into a Digimon card with the [Hybrid] trait in the hand with the digivolution cost reduced by 1.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool IsHybridDigimonCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon && cardSource.ContainsTraits("Hybrid");
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.HandCards.Count(IsHybridDigimonCardCondition) >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.HandCards.Count(IsHybridDigimonCardCondition) >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                targetPermanent: card.PermanentOfThisCard(),
                                cardCondition: IsHybridDigimonCardCondition,
                                payCost: true,
                                reduceCostTuple: (reduceCost: 1, reduceCostCardCondition: null),
                                fixedCostTuple: null,
                                ignoreDigivolutionRequirementFixedCost: -1,
                                isHand: true,
                                activateClass: activateClass,
                                successProcess: null));
                    }
                }
            }

            #endregion

            #region When Attacking - ESS

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false,
                    EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Attacking] If you have 7 or fewer cards in your hand, [Draw 1].";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.HandCards.Count <= 7)
                        {
                            if (card.Owner.LibraryCards.Count >= 1)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(
                        new DrawClass(card.Owner, 1, activateClass).Draw());
                }
            }

            #endregion

            return cardEffects;
        }
    }
}
