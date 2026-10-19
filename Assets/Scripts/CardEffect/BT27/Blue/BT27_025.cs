using System;
using System.Collections;
using System.Collections.Generic;

// Gawappamon — BT27-025.
namespace DCGO.CardEffects.BT27
{
    public class BT27_025 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternative Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.TopCard != null
                        && permanent.TopCard.EqualsTraits("DATA SQUAD");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    level: 3,
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 2,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null));
            }
            #endregion

            #region Shared On Play / When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                string effectName = "Give 1 of your Digimon battle-deletion protection; if played by an effect, also give it Blocker";
                string sharedHash = "BT27_025_OP_WD";

                bool IsOwnDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionPermanent(IsOwnDigimon);
                }

                IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
                {
                    bool playedByEffect = CardEffectCommons.CanTriggerOnPlay(hashtable, card)
                        && CardEffectCommons.IsByEffect(hashtable, null);

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                    Permanent selectedPermanent = null;

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsOwnDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage(
                        "Select 1 of your Digimon to protect from battle deletion.",
                        "The opponent is selecting a Digimon to protect from battle deletion.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    if (selectedPermanent == null) yield break;

                    yield return ContinuousController.instance.StartCoroutine(
                        CardEffectCommons.GainCanNotBeDeletedByBattle(
                            targetPermanent: selectedPermanent,
                            canNotBeDestroyedByBattleCondition: CanNotBeDestroyedByBattleCondition,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass,
                            effectName: "Can't be deleted in battle"));

                    if (playedByEffect)
                    {
                        yield return ContinuousController.instance.StartCoroutine(
                            CardEffectCommons.GainBlocker(
                                targetPermanent: selectedPermanent,
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass));
                    }

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;
                        yield break;
                    }

                    bool CanNotBeDestroyedByBattleCondition(Permanent permanent,
                        Permanent attackingPermanent, Permanent defendingPermanent, CardSource defendingCard)
                    {
                        return permanent == selectedPermanent
                            && (permanent == attackingPermanent || permanent == defendingPermanent);
                    }
                }

                #region On Play
                {
                    ActivateClass activateClass = new ActivateClass();
                    activateClass.SetUpICardEffect(effectName, CanUseCondition, card);
                    activateClass.SetUpActivateClass(CanActivateCondition,
                        hash => SharedActivateCoroutine(hash, activateClass), -1, false,
                        "[On Play] 1 of your Digimon can't be deleted in battle until your opponent's turn ends. If played by effects, it also gains <Blocker>.");
                    activateClass.SetHashString(sharedHash);
                    cardEffects.Add(activateClass);

                    bool CanUseCondition(Hashtable hashtable)
                    {
                        return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                            && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                    }
                }
                #endregion

                #region When Digivolving
                {
                    ActivateClass activateClass = new ActivateClass();
                    activateClass.SetUpICardEffect(effectName, CanUseCondition, card);
                    activateClass.SetUpActivateClass(CanActivateCondition,
                        hash => SharedActivateCoroutine(hash, activateClass), -1, false,
                        "[When Digivolving] 1 of your Digimon can't be deleted in battle until your opponent's turn ends. If played by effects, it also gains <Blocker>.");
                    activateClass.SetHashString(sharedHash);
                    cardEffects.Add(activateClass);

                    bool CanUseCondition(Hashtable hashtable)
                    {
                        return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                            && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                    }
                }
                #endregion
            }
            #endregion

            #region Inherited Evade
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.EvadeSelfEffect(
                    isInheritedEffect: true,
                    card: card,
                    condition: null));
            }
            #endregion

            return cardEffects;
        }
    }
}
