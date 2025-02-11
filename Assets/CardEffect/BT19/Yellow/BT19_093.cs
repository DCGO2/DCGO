using System.Collections;
using System.Collections.Generic;
using System;

namespace DCGO.CardEffects.BT19
{
    public class BT19_093 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Ignore Color Requirements
            if (timing == EffectTiming.None)
            {
                IgnoreColorConditionClass ignoreColorConditionClass = new IgnoreColorConditionClass();
                ignoreColorConditionClass.SetUpICardEffect("Ignore color requirements", CanUseCondition, card);
                ignoreColorConditionClass.SetUpIgnoreColorConditionClass(cardCondition: CardCondition);

                cardEffects.Add(ignoreColorConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return !CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.TopCard.EqualsCardName("Queen Device"));
                }
                
                bool CardCondition(CardSource cardSource)
                {
                    return (cardSource == card);
                }
            }
            #endregion

            #region When Trashed from battle area
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Give -3000 DP and Cannot activate When Digivolving effects.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "When this card is trashed in your battle area, until the end of your opponent's turn, 1 of your opponent's Digimon gets -3000 DP and that Digimon's [When Digivolving] effects don't activate.";
                }

                bool HasDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanActivateOnDeletion(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(HasDigimon));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: HasDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get -3000 DP and cannot activate When Digivolving effects.", "The opponent is selecting 1 Digimon.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                            targetPermanent: permanent,
                            changeValue: -3000,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass));

                        Permanent selectedPermanent = permanent;

                        if (selectedPermanent != null)
                        {
                            DisableEffectClass invalidationClass = new DisableEffectClass();
                            invalidationClass.SetUpICardEffect("Ignore [When Digivolving] Effect", CanUseCondition1, card);
                            invalidationClass.SetUpDisableEffectClass(DisableCondition: InvalidateCondition);
                            selectedPermanent.UntilOwnerTurnEndEffects.Add((_timing) => invalidationClass);

                            bool CanUseCondition1(Hashtable hashtable)
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
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Give -3000 DP and Cannot activate When Digivolving effects.", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Main] Until the end of your opponent's turn, 1 of your opponent's Digimon gets -3000 DP and that Digimon's [When Digivolving] effects don't activate. Then, place this card in the battle area.";
                }

                bool HasDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(HasDigimon));

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: HasDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get -3000 DP and cannot activate When Digivolving effects.", "The opponent is selecting 1 Digimon.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                            targetPermanent: permanent,
                            changeValue: -3000,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass));

                        Permanent selectedPermanent = permanent;

                        if (selectedPermanent != null)
                        {
                            DisableEffectClass invalidationClass = new DisableEffectClass();
                            invalidationClass.SetUpICardEffect("Ignore [When Digivolving] Effect", CanUseCondition1, card);
                            invalidationClass.SetUpDisableEffectClass(DisableCondition: InvalidateCondition);
                            selectedPermanent.UntilOwnerTurnEndEffects.Add((_timing) => invalidationClass);

                            bool CanUseCondition1(Hashtable hashtable)
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
                    
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
                }
            }
            #endregion

            #region Security Effect
            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Security Attack -2 for 2 of your opponents Digimon, then add this to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Security] 2 of your opponent's Digimon gain <Security A. -2> for the turn. Then, add this card to the hand.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        int maxCount = Math.Min(2, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

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

                        selectPermanentEffect.SetUpCustomMessage("Select 2 Digimon to give Security Attack -2.", "The opponent is selecting 2 Digimon.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonSAttack(
                                targetPermanent: permanent,
                                changeValue: -2,
                                effectDuration: EffectDuration.UntilEachTurnEnd,
                                activateClass: activateClass));
                            
                        }
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.AddThisCardToHand(card, activateClass));
                }
            }
            #endregion

            return cardEffects;
        }
    }
}