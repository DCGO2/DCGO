using System;
using System.Collections;
using System.Collections.Generic;

// Nanomachine Break
namespace DCGO.CardEffects.BT23
{
    public class BT23_094 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Ignore Color Requirment

            if (timing == EffectTiming.None)
            {
                IgnoreColorConditionClass ignoreColorConditionClass = new IgnoreColorConditionClass();
                ignoreColorConditionClass.SetUpICardEffect("Ignore color requirements", CanUseCondition, card);
                ignoreColorConditionClass.SetUpIgnoreColorConditionClass(cardCondition: CardCondition);
                cardEffects.Add(ignoreColorConditionClass);

                bool HasAppmonTrait(Permanent permanent)
                {
                    if (CardEffectCommons.IsOwnerPermanent(permanent, card))
                    {
                        if (permanent.TopCard.HasCSTraits)
                        {
                            if (permanent.IsDigimon || permanent.IsTamer)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.HasMatchConditionPermanent(HasAppmonTrait, true);
                }

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }
            }

            #endregion

            #region Main/YT/Security Shared

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                bool IsOpponentsDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                if (CardEffectCommons.HasMatchConditionPermanent(IsOpponentsDigimon))
                {
                    Permanent selectedPermanent = null;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(IsOpponentsDigimon));

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsOpponentsDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectedPermanent,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    IEnumerator SelectedPermanent(Permanent target)
                    {
                        selectedPermanent = target;
                        yield return null;
                    }

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will gain -1 Sec Atk, cant activate [When Digivolving] and cant activate [When Attacking].", "The opponent is selecting 1 Digimon that will gain -1 Sec Atk, cant activate [When Digivolving] and cant activate [When Attacking].");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    if (selectedPermanent != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonSAttack(
                            targetPermanent: selectedPermanent,
                            changeValue: -1,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass));

                        bool CanUseConditionDebuff(Hashtable hashtableDebuff)
                        {
                            return true;
                        }

                        bool InvalidateWDCondition(ICardEffect cardEffect)
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

                        bool InvalidateWACondition(ICardEffect cardEffect)
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
                                                if (cardEffect.IsOnAttack)
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

                        DisableEffectClass invalidationClass = new DisableEffectClass();
                        invalidationClass.SetUpICardEffect("Ignore [When Digivolving] Effect", CanUseConditionDebuff, card);
                        invalidationClass.SetUpDisableEffectClass(DisableCondition: InvalidateWDCondition);
                        selectedPermanent.UntilOwnerTurnEndEffects.Add(_ => invalidationClass);

                        DisableEffectClass invalidationClass1 = new DisableEffectClass();
                        invalidationClass1.SetUpICardEffect("Ignore [When Attacking] Effect", CanUseConditionDebuff, card);
                        invalidationClass1.SetUpDisableEffectClass(DisableCondition: InvalidateWACondition);
                        selectedPermanent.UntilOwnerTurnEndEffects.Add(_ => invalidationClass1);


                    }
                }
            }

            #endregion

            #region Main

            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("1 digimon gains Sec Atk -1 and cant use [When Digivolving] & [When Attacking effects]. then place is battle area", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] Until your opponent's turn ends, give 1 of their Digimon <Security A. -1> and it can't activate [When Digivolving] or [When Attacking] effects. Then, place this card in the battle area.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(SharedActivateCoroutine(hashtable, activateClass));
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
                }
            }

            #endregion

            #region Your Turn - Delay

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("1 digimon gains Sec Atk -1 and cant use [When Digivolving] & [When Attacking effects].", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "When one of your [CS] trait Digimon attacks <Delay>. Until your opponent's turn ends, give 1 of their Digimon <Security A. -1> and it can't activate [When Digivolving] or [When Attacking] effects.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.CanDeclareOptionDelayEffect(card)
                        && CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, PermanentCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.TopCard.HasCSTraits;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() }, activateClass: activateClass, successProcess: permanents => SuccessProcess(), failureProcess: null));

                    IEnumerator SuccessProcess()
                    {
                        yield return ContinuousController.instance.StartCoroutine(SharedActivateCoroutine(hashtable, activateClass));
                    }
                }
            }

            #endregion

            #region Security

            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("1 digimon gains Sec Atk -1 and cant use [When Digivolving] & [When Attacking effects]. then place is battle area", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Security] Until your opponent's turn ends, give 1 of their Digimon <Security A. -1> and it can't activate [When Digivolving] or [When Attacking] effects. Then, place this card in the battle area.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(SharedActivateCoroutine(hashtable, activateClass));
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
                }
            }

            #endregion

            return cardEffects;
        }
    }
}