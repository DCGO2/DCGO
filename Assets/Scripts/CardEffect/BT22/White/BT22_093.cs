using DCGO.CardEntities;
using System.Collections;
using System.Collections.Generic;

// Ami Aiba
namespace DCGO.CardEffects.BT22
{
    public class BT22_093 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Start of Main Phase

            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Memory +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Start of Your Main Phase] If your opponent has a Digimon, gain 1 memory.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
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
                        if (card.Owner.Enemy.GetBattleAreaDigimons().Count >= 1)
                        {
                            if (card.Owner.CanAddMemory(activateClass))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }

            #endregion

            #region Your Turn

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                Permanent DigivolvingDigimon = null;

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into a [CS] Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn] When any of your Digimon digivolve into a Digimon with the [CS] trait, if it has a digivolution card with the same level as the digivolved Digimon, by suspending this Tamer, that Digimon may digivolve into a Digimon card with the [CS] trait in the hand without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnField(card) &&
                           CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, DigivolvingCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (DigivolvingDigimon != null)
                    {
                        if (!DigivolvingCondition(DigivolvingDigimon))
                            return false;
                    }

                    return CardEffectCommons.IsExistOnField(card)
                        && CardEffectCommons.IsOwnerTurn(card)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card);
                }

                bool DigivolvingCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.HasCSTraits)
                        {
                            if (permanent.DigivolutionCards.Filter(x => x.HasLevel && x.Level == permanent.Level).Count >= 1)
                            {
                                DigivolvingDigimon = permanent;
                                return true;
                            }
                        }
                    }
                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon && cardSource.HasCSTraits)
                    {
                        if (cardSource.CanPlayCardTargetFrame(DigivolvingDigimon.PermanentFrame, true, activateClass))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(
                        new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    if (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition))
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                        targetPermanent: DigivolvingDigimon,
                        cardCondition: CanSelectCardCondition,
                        payCost: false,
                        reduceCostTuple: null,
                        fixedCostTuple: null,
                        ignoreDigivolutionRequirementFixedCost: -1,
                        isHand: true,
                        activateClass: activateClass,
                        successProcess: null));
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