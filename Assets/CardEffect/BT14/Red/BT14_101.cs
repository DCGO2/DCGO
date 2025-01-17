using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT14
{
    public class BT14_101 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasGreymonName && targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 5;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 4,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null));
            }

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    bool PermanentCondition(Permanent permanent)
                    {
                        if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                        {
                            if (permanent.IsTamer)
                            {
                                if (permanent.TopCard.ContainsCardName("Tai Kamiya"))
                                {
                                    return true;
                                }
                            }
                        }

                        return false;
                    }

                    bool PermanentCondition1(Permanent permanent)
                    {
                        if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                        {
                            if (permanent.DP >= 10000)
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    if (card.Owner.HandCards.Contains(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(PermanentCondition))
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(PermanentCondition1))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool PermanentCondition(Permanent targetPermanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnBattleArea(targetPermanent))
                    {
                        if (targetPermanent.TopCard.CardNames.Contains("Agumon"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 4,
                    ignoreDigivolutionRequirement: true,
                    card: card,
                    condition: Condition));
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("This Digimon gains Raid and can attack", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] This Digimon gains <Raid> for the turn. Then, it may attack.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainRaid(
                        targetPermanent: card.PermanentOfThisCard(),
                        effectDuration: EffectDuration.UntilEachTurnEnd,
                        activateClass: activateClass));

                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().CanAttack(activateClass))
                        {
                            SelectAttackEffect selectAttackEffect = GManager.instance.GetComponent<SelectAttackEffect>();

                            selectAttackEffect.SetUp(
                                attacker: card.PermanentOfThisCard(),
                                canAttackPlayerCondition: () => true,
                                defenderCondition: (permanent) => true,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectAttackEffect.Activate());
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect($"This Digimon gains Security Attack +1 and Pierce", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking] If you have a Tamer, this Digimon gains ÅÉSecurity A. +1ÅÑ and <Piercing> for the turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.GetBattleAreaPermanents().Some(permanent => permanent.IsTamer))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonSAttack(
                        targetPermanent: card.PermanentOfThisCard(),
                        changeValue: 1,
                        effectDuration: EffectDuration.UntilEachTurnEnd,
                        activateClass: activateClass));

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainPierce(
                        targetPermanent: card.PermanentOfThisCard(),
                        effectDuration: EffectDuration.UntilEachTurnEnd,
                        activateClass: activateClass));
                }
            }

            return cardEffects;
        }
    }
}