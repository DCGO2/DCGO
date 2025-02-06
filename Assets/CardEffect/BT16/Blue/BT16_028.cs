using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT16
{
    public class BT16_028 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.CardNames.Contains("Paildramon") || targetPermanent.TopCard.CardNames.Contains("Dinobeemon");
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

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("1 of your opponent's Digimon/Tamer can't unsuspend.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] 1 of your opponent's Digimon or Tamer can't unsuspend until the end of their turn. Then, by suspending 1 of their Digimon or Tamers, unsuspend 1 of your Digimon.";
                }

                bool CanSelectOpponentsDigimon(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card))
                    {
                        return permanent.IsDigimon || permanent.IsTamer;
                    }

                    return false;
                }

                bool CanSelectOpponentsDigimonToSuspend(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card))
                    {
                        if (permanent.IsDigimon || permanent.IsTamer)
                        {
                            if (!permanent.IsSuspended)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectYourSuspendedDigimon(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.IsDigimon && permanent.IsSuspended)
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card))
                    {
                        return true;
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentsDigimon))
                    {
                        Permanent selectedPermanent = null;
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition_ByPreSelecetedList: null,
                            canTargetCondition: CanSelectOpponentsDigimon,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon/Tamer to not unsuspend.", "The opponent is selecting 1 Digimon/Tamer to not unsuspend.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotUnsuspend(
                                targetPermanent: selectedPermanent,
                                effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                activateClass: activateClass,
                                condition: null,
                                effectName: "Your Digimon can't unsuspend"));
                        }
                    }

                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentsDigimonToSuspend))
                        {
                            Permanent suspendedPermanent = null;
                            SelectPermanentEffect selectSuspendEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectSuspendEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition_ByPreSelecetedList: null,
                                canTargetCondition: CanSelectOpponentsDigimonToSuspend,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: true,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectSuspendCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Tap,
                                cardEffect: activateClass);

                            selectSuspendEffect.SetUpCustomMessage("Select 1 Digimon/Tamer to suspend.", "The opponent is selecting 1 Digimon/Tamer to suspend.");

                            yield return ContinuousController.instance.StartCoroutine(selectSuspendEffect.Activate());

                            IEnumerator SelectSuspendCoroutine(Permanent permanent)
                            {
                                suspendedPermanent = permanent;
                                yield return null;
                            }

                            if (suspendedPermanent != null)
                            {
                                SelectPermanentEffect selectUnsuspendEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectSuspendEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canTargetCondition: CanSelectYourSuspendedDigimon,
                                    canEndSelectCondition: null,
                                    maxCount: 1,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: SelectSuspendCoroutine,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.UnTap,
                                    cardEffect: activateClass);

                                selectSuspendEffect.SetUpCustomMessage("Select 1 Digimon to unsuspend.", "The opponent is selecting 1 Digimon to unsuspend.");

                                yield return ContinuousController.instance.StartCoroutine(selectSuspendEffect.Activate());
                            }
                        }
                    }
                }
            }

            #endregion

            #region All Turns - When an effect plays/digivolves

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Digivolve into [Imperialdramon: Fighter Mode]", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] When an effect plays or digivolves an opponent's Digimon, if you have a Tamer, this Digimon may digivolve into [Imperialdramon: Fighter Mode] in your hand without paying the cost.";
                }

                bool IsOpponentDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool HasTamer(Permanent permanent)
                {
                    if (permanent.IsTamer)
                        return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card);

                    return false;
                }

                bool HasFighterMode(CardSource cardSource)
                {
                    return cardSource.CardNames.Contains("Imperialdramon: Fighter Mode") || cardSource.CardNames.Contains("Imperialdramon:FighterMode");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, IsOpponentDigimon) || CardEffectCommons.CanTriggerWhenPermanentDigivolving(hashtable, IsOpponentDigimon))
                    {
                        if (CardEffectCommons.IsByEffect(hashtable, null))
                        {
                            if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, HasTamer))
                            {
                                if (card.Owner.HandCards.Count(HasFighterMode) >= 1)
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
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                            targetPermanent: card.PermanentOfThisCard(),
                            cardCondition: HasFighterMode,
                            payCost: false,
                            reduceCostTuple: null,
                            fixedCostTuple: null,
                            ignoreDigivolutionRequirementFixedCost: -1,
                            isHand: true,
                            activateClass: activateClass,
                            successProcess: null,
                            ignoreSelection: true));
                }
            }

            #endregion

            return cardEffects;
        }
    }
}