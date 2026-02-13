using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT15
{
    public class BT15_063 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.CardTraits.Contains("DigiPolice") || targetPermanent.TopCard.HasXAntibodyTraits)
                    {
                        if (targetPermanent.TopCard.HasLevel)
                        {
                            if (targetPermanent.Level == 4)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            if (timing == EffectTiming.OnTappedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Unsuspend a Digimon with the [Beast Dragon] or [DigiPolice] trait.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("unsuspend_BT15_063");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns][Once Per Turn] When an effect suspends another Digimon or Tamer, unsuspend 1 of your Digimon with the [Beast Dragon] or [DigiPolice] trait.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent.IsDigimon)
                        {
                            if (permanent.TopCard.HasDigiPoliceTraits || permanent.TopCard.HasBeastDragonTraits)
                            {
                                if (permanent.IsSuspended)
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenPermanentSuspends(hashtable, (permanent) => CardEffectCommons.IsPermanentExistsOnBattleArea(permanent) && permanent != card.PermanentOfThisCard()))
                        {
                            if (CardEffectCommons.IsByEffect(hashtable, null))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(PermanentCondition))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(PermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: PermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: CanEndSelectCondition,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: true,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.UnTap,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        bool CanEndSelectCondition(List<Permanent> permanents)
                        {
                            if (CardEffectCommons.HasNoElement(permanents))
                            {
                                return false;
                            }

                            return true;
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnTappedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("This Digimon digivolves", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] When an effect suspends another Digimon or Tamer, if this Digimon has a Tamer card with the [DigiPolice] trait in it's digivolution cards, you may digivole this Digimon into a Digimon card with the [Beast Dragon] or [DigiPolice] trait from your hand without paying the cost.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.HasBeastDragonTraits || cardSource.HasDigiPoliceTraits)
                    {
                        if (cardSource.IsDigimon)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenPermanentSuspends(hashtable, (permanent) => CardEffectCommons.IsExistOnBattleAreaDigimon(card) && permanent != card.PermanentOfThisCard()))
                        {
                            if (CardEffectCommons.IsByEffect(hashtable, null))
                            {
                                if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => (cardSource.CardTraits.Contains("DigiPolice") && cardSource.IsTamer)) >= 1)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                            targetPermanent: card.PermanentOfThisCard(),
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

            return cardEffects;
        }
    }
}