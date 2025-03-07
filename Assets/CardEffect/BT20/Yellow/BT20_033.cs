using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT20
{
    public class BT20_033 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement

            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 4 && targetPermanent.TopCard.EqualsTraits("ACCEL");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("1 Opponent's Digimon gets can't activate [When Digivolving] and -3000 DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Until the end of your opponent's turn, 1 of their Digimon can't activate [When Digivolving] effects and gets -3000 DP.";
                }

                bool IsOpponentsDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.HasMatchConditionPermanent(IsOpponentsDigimon);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = null;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsOpponentsDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectedPermanent,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will gain effect and DP -3000.", "The opponent is selecting 1 Digimon that will gain effect and DP -3000.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectedPermanent(Permanent target)
                    {
                        selectedPermanent = target;
                        yield return null;
                    }

                    if (selectedPermanent != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: selectedPermanent, changeValue: -3000, effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));

                        DisableEffectClass invalidationClass = new DisableEffectClass();
                        invalidationClass.SetUpICardEffect("Ignore [When Digivolving] Effect", CanUseConditionDebuff, card);
                        invalidationClass.SetUpDisableEffectClass(DisableCondition: InvalidateCondition);
                        selectedPermanent.UntilOwnerTurnEndEffects.Add(_ => invalidationClass);

                        bool CanUseConditionDebuff(Hashtable hashtableDebuff)
                        {
                            return true;
                        }

                        bool InvalidateCondition(ICardEffect cardEffect)
                        {
                            if (selectedPermanent.TopCard != null)
                            {
                                if (cardEffect != null)
                                {
                                    if (cardEffect.EffectSourceCard != null)
                                    {
                                        if (isExistOnField(cardEffect.EffectSourceCard))
                                        {
                                            if (cardEffect.EffectSourceCard.PermanentOfThisCard() == selectedPermanent)
                                            {
                                                if (cardEffect.IsWhenDigivolving)
                                                {
                                                    if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                                    {
                                                        return true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            return false;
                        }
                    }
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("1 Opponent's Digimon gets can't activate [When Digivolving] and -3000 DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Until the end of your opponent's turn, 1 of their Digimon can't activate [When Digivolving] effects and gets -3000 DP.";
                }

                bool IsOpponentsDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.HasMatchConditionPermanent(IsOpponentsDigimon);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = null;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsOpponentsDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectedPermanent,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will gain effect and DP -3000.", "The opponent is selecting 1 Digimon that will gain effect and DP -3000.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectedPermanent(Permanent target)
                    {
                        selectedPermanent = target;
                        yield return null;
                    }

                    if (selectedPermanent != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: selectedPermanent, changeValue: -3000, effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));

                        DisableEffectClass invalidationClass = new DisableEffectClass();
                        invalidationClass.SetUpICardEffect("Ignore [When Digivolving] Effect", CanUseConditionDebuff, card);
                        invalidationClass.SetUpDisableEffectClass(DisableCondition: InvalidateCondition);
                        selectedPermanent.UntilOwnerTurnEndEffects.Add(_ => invalidationClass);

                        bool CanUseConditionDebuff(Hashtable hashtableDebuff)
                        {
                            return true;
                        }

                        bool InvalidateCondition(ICardEffect cardEffect)
                        {
                            if (selectedPermanent.TopCard != null)
                            {
                                if (cardEffect != null)
                                {
                                    if (cardEffect.EffectSourceCard != null)
                                    {
                                        if (isExistOnField(cardEffect.EffectSourceCard))
                                        {
                                            if (cardEffect.EffectSourceCard.PermanentOfThisCard() == selectedPermanent)
                                            {
                                                if (cardEffect.IsWhenDigivolving)
                                                {
                                                    if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                                    {
                                                        return true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            return false;
                        }
                    }
                }
            }
            #endregion

            #region Redirect - ESS
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("You may change the attack target to this Digimon.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("Redirect_BT20-033");
                activateClass.SetIsInheritedEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Opponent's Turn] [Once Per Turn] When any of your opponent's Digimon attack, you may change the attack target to this Digimon.";
                }

                bool IsOpponentDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool IsThisDigimon(Permanent permanent)
                {
                    return permanent == card.PermanentOfThisCard();
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsOpponentTurn(card) &&
                           CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, IsOpponentDigimon);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsThisDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to attack.",
                        "The opponent is selecting 1 Digimon to attack.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.attackProcess.SwitchDefender(
                            activateClass,
                            false,
                            permanent));
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}