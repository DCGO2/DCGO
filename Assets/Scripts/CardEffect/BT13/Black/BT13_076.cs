using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT13
{
    public class BT13_076 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return (targetPermanent.TopCard.ContainsCardName("Etemon") || targetPermanent.TopCard.ContainsCardName("Sukamon")) && targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 5;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 4, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("DP -3000 and Security Attack -1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("DP-3000_BT13_076");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns][Once Per Turn] When another Digimon with [Etemon] or [Sukamon] in its name is deleted, 1 of your opponent's Digimon gets -3000 DP and gains <Security Attack -1> until the end of your opponent's turn.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
                    {
                        if (permanent.IsDigimon)
                        {
                            if (permanent != card.PermanentOfThisCard())
                            {
                                if (permanent.TopCard.ContainsCardName("Sukamon"))
                                {
                                    return true;
                                }

                                if (permanent.TopCard.ContainsCardName("Etemon"))
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
                        if (CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable, PermanentCondition))
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
                        return true;
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
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage(
                            "Select 1 Digimon that will get effects.",
                            "The opponent is selecting 1 Digimon that will get effects.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                targetPermanent: permanent,
                                changeValue: -3000,
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass));

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonSAttack(
                                targetPermanent: permanent,
                                changeValue: -1,
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass));
                        }
                    }
                }
            }

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.ContainsCardName("Etemon"))
                        {
                            return true;
                        }

                        if (permanent.TopCard.ContainsCardName("Sukamon"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOpponentTurn(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.BlockerStaticEffect(
                    permanentCondition: PermanentCondition,
                    isInheritedEffect: false,
                    card: card,
                    condition: CanUseCondition));
            }

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.ContainsCardName("Etemon") || permanent.TopCard.ContainsCardName("Sukamon"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CardEffectCondition(ICardEffect cardEffect)
                {
                    return true;
                }

                bool CanUseCondition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOpponentTurn(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                string effectName = "Your Digimon can't return to hand";

                cardEffects.Add(CardEffectFactory.CannotReturnToHandStaticEffect(
                    permanentCondition: PermanentCondition,
                    cardEffectCondition: CardEffectCondition,
                    isInheritedEffect: false,
                    card: card,
                    condition: CanUseCondition,
                    effectName: effectName
                ));
            }

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.ContainsCardName("Etemon") || permanent.TopCard.ContainsCardName("Sukamon"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CardEffectCondition(ICardEffect cardEffect)
                {
                    return true;
                }

                bool CanUseCondition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOpponentTurn(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                string effectName = "Your Digimon can't return to deck";

                cardEffects.Add(CardEffectFactory.CannotReturnToDeckStaticEffect(
                    permanentCondition: PermanentCondition,
                    cardEffectCondition: CardEffectCondition,
                    isInheritedEffect: false,
                    card: card,
                    condition: CanUseCondition,
                    effectName: effectName
                ));
            }

            return cardEffects;
        }
    }
}