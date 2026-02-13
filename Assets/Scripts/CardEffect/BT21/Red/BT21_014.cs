using System.Collections;
using System.Collections.Generic;

// BurningGreymon
namespace DCGO.CardEffects.BT21
{
    public class BT21_014 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Condition - Tamer

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.IsTamer)
                    {
                        return targetPermanent.TopCard.CardColors.Contains(CardColor.Red);
                    }
                    return false;
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

            #region Alternative Digivolution Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.EqualsCardName("Agunimon"))
                    {
                        return true;
                    }
                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 1,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }

            #endregion

            #region On Play / When Digivolving Shared

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainPierce(
                    card.PermanentOfThisCard(),
                    EffectDuration.UntilEachTurnEnd,
                    activateClass: activateClass
                    ));
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                    targetPermanent: card.PermanentOfThisCard(),
                    changeValue: 3000,
                    effectDuration: EffectDuration.UntilEachTurnEnd,
                    activateClass: activateClass));
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Gain <Piercing> and 3000 DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "[On Play] This Digimon gains <Piercing> (When this Digimon attacks and deletes an opponent's Digimon and survives the battle, it performs any security checks it normally would) and +3000 DP for the turn.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerOnPlay(hashtable, card))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        return true;
                    }
                    return false;
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Gain <Piercing> and 3000 DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, hashtable => SharedActivateCoroutine(hashtable, activateClass), -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "[When Digivolving] This Digimon gains <Piercing> (When this Digimon attacks and deletes an opponent's Digimon and survives the battle, it performs any security checks it normally would) and +3000 DP for the turn.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        return true;
                    }
                    return false;
                }
            }

            #endregion

            #region Your turn

            if (timing == EffectTiming.OnLoseSecurity)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into a level 5 [Hybrid]", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "[Your Turn] When your opponent's security stack is removed from, this Digimon may digivolve into a level 5 Digimon card with the [Hybrid] trait in the hand with the digivolution cost reduced by 1.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerWhenLoseSecurity(hashtable, PlayerCondition))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCard))
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanSelectCard(CardSource cardSource)
                {
                    if (card.IsDigimon)
                    {
                        if (cardSource.HasLevel && cardSource.Level == 5)
                        {
                            if (cardSource.EqualsTraits("Hybrid"))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                bool PlayerCondition(Player player)
                {
                    if (player == card.Owner.Enemy)
                    {
                        return true;
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                        targetPermanent: card.PermanentOfThisCard(),
                        cardCondition: CanSelectCard,
                        payCost: true,
                        reduceCostTuple: (reduceCost: 1, reduceCostCardCondition: null),
                        fixedCostTuple: null,
                        ignoreDigivolutionRequirementFixedCost: -1,
                        isHand: true,
                        activateClass: activateClass,
                        successProcess: null));
                }
            }

            #endregion

            #region ESS

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        return true;
                    }
                    return false;
                }
                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(changeValue: 2000, isInheritedEffect: true, card: card, condition: Condition));
            }

            #endregion

            return cardEffects;
        }
    }
}