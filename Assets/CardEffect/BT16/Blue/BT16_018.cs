using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT16
{
    public class BT16_018 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            var cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.CardNames.Contains("Veemon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 2,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null)
                );
            }

            #endregion

            #region Inherited Effect

            if (timing == EffectTiming.None)
            {
                bool InheritedEffectCondition()
                {
                    return CardEffectCommons.IsOwnerTurn(card);
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect(
                    changeValue: 2000,
                    isInheritedEffect: true,
                    card: card,
                    condition: InheritedEffectCondition));
            }

            #endregion

            #region Raid

            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.RaidSelfEffect(
                        isInheritedEffect: false,
                        card: card,
                        condition: null)
                    );
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activatePlayClass = new ActivateClass();
                activatePlayClass.SetUpICardEffect("Select 1 of your Digimon to gain battle protection", CanUsePlayCondition, card);
                activatePlayClass.SetUpActivateClass(CanActivateCondition, ActivatePlayCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activatePlayClass);

                string EffectDiscription()
                {
                    return "[On Play] 1 of your Digimon can't be deleted in battle until the end of your opponent's turn.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool CanUsePlayCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                        return CardEffectCommons.CanTriggerOnPlay(hashtable, card);

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivatePlayCoroutine(Hashtable hashtable)
                {
                    var selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activatePlayClass);

                    selectPermanentEffect.SetUpCustomMessage(
                        "Select 1 Digimon that will get effects.",
                        "The opponent is selecting 1 Digimon that will get effects.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    yield break;

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotBeDeletedByBattle(
                            targetPermanent: permanent,
                            canNotBeDestroyedByBattleCondition: CanNotBeDestroyedByBattleCondition,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activatePlayClass,
                            effectName: "Can't be deleted in battle"));
                        yield break;

                        bool CanNotBeDestroyedByBattleCondition(Permanent permanent1, Permanent attackingPermanent, Permanent defendingPermanent, CardSource defendingCard)
                        {
                            return permanent1 == attackingPermanent || permanent1 == defendingPermanent;
                        }
                    }
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateDigivolveClass = new ActivateClass();
                activateDigivolveClass.SetUpICardEffect("Select 1 of your Digimon to gain battle protection", CanUseDigivolveCondition, card);
                activateDigivolveClass.SetUpActivateClass(CanActivateCondition, ActivateDigivolveCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateDigivolveClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] 1 of your Digimon can't be deleted in battle until the end of your opponent's turn.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool CanUseDigivolveCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                        return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateDigivolveCoroutine(Hashtable hashtable)
                {
                    var selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateDigivolveClass);

                    selectPermanentEffect.SetUpCustomMessage(
                        "Select 1 Digimon that will get effects.",
                        "The opponent is selecting 1 Digimon that will get effects.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    yield break;

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotBeDeletedByBattle(
                            targetPermanent: permanent,
                            canNotBeDestroyedByBattleCondition: CanNotBeDestroyedByBattleCondition,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateDigivolveClass,
                            effectName: "Can't be deleted in battle"));
                        yield break;

                        bool CanNotBeDestroyedByBattleCondition(Permanent permanent1, Permanent attackingPermanent, Permanent defendingPermanent, CardSource defendingCard)
                        {
                            return permanent == attackingPermanent || permanent == defendingPermanent;
                        }
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}