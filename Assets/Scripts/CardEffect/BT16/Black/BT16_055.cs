using System;
using System.Collections;
using System.Collections.Generic;

// Namakemon
namespace DCGO.CardEffects.BT16
{
    public class BT16_055 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            # region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.CardNames.Contains("Pulsemon"))
                    {
                        return true;
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 2,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null));
            }
            #endregion

            #region Shared OP/WD
            string SharedEffectName() => "Conditionally 1 of your Digimon can't be DP reduced or De-Digivolved, Conditionally 1 of your Digimon gains <Blocker> and <Reboot>";

            string SharedEffectDescription(string tag) => $"[{tag}] If you have 3 or more security cards, 1 of your Digimon can't have its DP reduced by your opponent's effects, and isn't affected by <De-Digivolve> effects until the end of your opponent's turn. If you have 3 or fewer security cards, 1 of your Digimon gains <Blocker> and <Reboot> until the end of your opponent's turn.";

            bool SharedCanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
            }

            Permanent selectedPermanent = null;

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
            }
            
            #region Can't Be De-Digivolved

            void ActivateDeDigivolveProtection()
            {
                ImmuneFromDeDigivolveClass immuneFromDeDigivolveClass = new ImmuneFromDeDigivolveClass();
                immuneFromDeDigivolveClass.SetUpICardEffect("Isn't affected by <De-Digivolve>", CanUseDeDigivolveCondition, selectedPermanent.TopCard);
                immuneFromDeDigivolveClass.SetUpImmuneFromDeDigivolveClass(PermanentCondition: PermanentDeDigivolveCondition);
                selectedPermanent.UntilOpponentTurnEndEffects.Add((_timing) => immuneFromDeDigivolveClass);
            }

            bool CanUseDeDigivolveCondition(Hashtable hashtable1)
            {
                if (selectedPermanent.TopCard != null)
                {
                    return true;
                }

                return false;
            }

            bool PermanentDeDigivolveCondition(Permanent permanent)
            {
                if (permanent == selectedPermanent)
                {
                    return true;
                }

                return false;
            }
            #endregion

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (card.Owner.SecurityCards.Count >= 3)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

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
                            cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage(
                                "Select 1 Digimon that can't have its DP reduced by your opponent's effects and isn't affected by <De-Digivolve> effects until the end of your opponent's turn.",
                                "The opponent is selecting 1 Digimon that can't have its DP reduced by your opponent's effects and isn't affected by <De-Digivolve> effects until the end of your turn.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;

                        yield return null;
                    }

                    if (selectedPermanent != null)
                    {
                        ActivateDeDigivolveProtection();

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainImmuneFromDPMinus(
                            targetPermanent: selectedPermanent,
                            cardEffectCondition: cardEffect => CardEffectCommons.IsOpponentEffect(cardEffect, card),
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass,
                            effectName: "Can't have DP reduced"));
                    }
                }
                if (card.Owner.SecurityCards.Count <= 3)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

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
                            cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage(
                                "Select 1 Digimon that gains <Blocker> and <Reboot> until the end of your opponent's turn.",
                                "The opponent is selecting 1 Digimon that gains <Blocker> and <Reboot> until the end of your turn.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;

                        yield return null;
                    }

                    if (selectedPermanent != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainReboot(
                            targetPermanent: selectedPermanent,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass));

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainBlocker(
                            targetPermanent: selectedPermanent,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass));
                    }
                }
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("On Play"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(SharedEffectName(), CanUseCondition, card);
                activateClass.SetUpActivateClass(SharedCanActivateCondition, hash => SharedActivateCoroutine(hash, activateClass), -1, false, SharedEffectDescription("When Digivolving"));
                cardEffects.Add(activateClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }
            }
            #endregion

            #region Inherited Effect
            if (timing == EffectTiming.None)
            {
                bool condition()
                {
                    return card.PermanentOfThisCard().TopCard.HasText("Pulsemon");
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect<Func<int>>(
                    changeValue: () => 1000,
                    isInheritedEffect: true,
                    card: card,
                    condition: condition));
            }
            #endregion

            return cardEffects;
        }
    }
}
