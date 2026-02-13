using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT16
{
    public class BT16_021 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement, Blocker

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.CardNames.Contains("Wormmon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 2,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );

                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }

            #endregion

            #region Armor Purge

            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.ArmorPurgeEffect(card: card));
            }

            #endregion

            #region All Turns

            if (timing == EffectTiming.OnTappedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 digivolution card & stun 1 Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When an opponent's Digimon becomes suspended, trash the top digivolution card of 1 of their Digimon. Then, 1 of their Digimon with no digivolution cards can't attack or block until the end of their turn.";
                }

                bool CanSelectPermanentSourceStripCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanSelectPermanentStunCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.HasNoDigivolutionCards)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenPermanentSuspends(hashtable, PermanentCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    SelectPermanentEffect selectPermanentSourceStripEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentSourceStripEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentSourceStripCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentSourceStripCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentSourceStripEffect.SetUpCustomMessage("Select 1 Digimon that will trash digivolution cards.",
                        "The opponent is selecting 1 Digimon that will trash digivolution cards.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentSourceStripEffect.Activate());

                    IEnumerator SelectPermanentSourceStripCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: permanent,
                                trashCount: 1, isFromTop: true, activateClass: activateClass));
                    }

                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentStunCondition))
                    {
                        SelectPermanentEffect selectPermanentStunEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentStunEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentStunCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentStunCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentStunEffect.SetUpCustomMessage("Select 1 Digimon that will get effects.", "The opponent is selecting 1 Digimon that will get effects.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentStunEffect.Activate());

                        IEnumerator SelectPermanentStunCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotAttack(
                                targetPermanent: permanent,
                                defenderCondition: null,
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass,
                                effectName: "Can't Attack"));

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotBlock(
                                targetPermanent: permanent,
                                attackerCondition: null,
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass,
                                effectName: "Can't Block"));
                        }
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}