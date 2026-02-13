using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT16
{
    public class BT16_044 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digivolve Condition

            if (timing == EffectTiming.None)
            {
                bool HasPulsmonInText(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasText("Pulsemon") && targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 4;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: HasPulsmonInText, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region On Play/When Digivolving Shared

            bool PermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                return false;
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend and/or gain memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] If you have 3 or more security cards, suspend 1 of your opponent's Digimon. If you have 3 or fewer security cards, gain 1 memory.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (card.Owner.SecurityCards.Count >= 3)
                        {
                            if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, PermanentCondition))
                            {
                                return true;
                            }
                        }

                        if (card.Owner.SecurityCards.Count <= 3)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.SecurityCards.Count >= 3)
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(PermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: PermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                            mode: SelectPermanentEffect.Mode.Tap,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to suspend and give effects.", "The opponent is selecting 1 Digimon to suspend and give effects.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                        {
                            foreach (Permanent selectedPermanent in permanents)
                            {
                                if (CardEffectCommons.HasMatchConditionPermanent(PermanentCondition))
                                {
                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCantUnsuspendNextActivePhase(
                                            targetPermanent: selectedPermanent,
                                            activateClass: activateClass
                                        ));
                                }
                            }
                            yield return null;
                        }
                    }
                    if (card.Owner.SecurityCards.Count <= 3)
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(2, activateClass));
                    }
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend and/or gain memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] If you have 3 or more security cards, suspend 1 of your opponent's Digimon. If you have 3 or fewer security cards, gain 1 memory.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (card.Owner.SecurityCards.Count >= 3)
                        {
                            if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, PermanentCondition))
                            {
                                return true;
                            }
                        }

                        if (card.Owner.SecurityCards.Count <= 3)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.SecurityCards.Count >= 3)
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(PermanentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: PermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                            mode: SelectPermanentEffect.Mode.Tap,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to suspend and give effects.", "The opponent is selecting 1 Digimon to suspend and give effects.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                        {
                            foreach (Permanent selectedPermanent in permanents)
                            {
                                if (CardEffectCommons.HasMatchConditionPermanent(PermanentCondition))
                                {
                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCantUnsuspendNextActivePhase(
                                            targetPermanent: selectedPermanent,
                                            activateClass: activateClass
                                        ));
                                }
                            }
                            yield return null;
                        }
                    }
                    if (card.Owner.SecurityCards.Count <= 3)
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(2, activateClass));
                    }
                }
            }

            #endregion

            #region Inherit

            if (timing == EffectTiming.OnEndAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Unsuspend this Digimon.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Unsuspend_BT16_044");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Attack][Once per turn] If this Digimon has [Pulsemon] in its text, by trashing the top card of your security stack, unsuspend this Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnEndAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().TopCard.HasText("Pulsemon"))
                        {
                            if (card.Owner.SecurityCards.Count >= 1)
                            {
                                if (CardEffectCommons.CanUnsuspend(card.PermanentOfThisCard()))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool PermanentHasPulsemonInText(Permanent permanent)
                {
                    if (permanent == card.PermanentOfThisCard())
                    {
                        if (card.PermanentOfThisCard().TopCard.HasText("Pulsemon"))
                        {
                            if (CardEffectCommons.CanUnsuspend(card.PermanentOfThisCard()))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                    player: card.Owner,
                    destroySecurityCount: 1,
                    cardEffect: activateClass,
                    fromTop: true).DestroySecurity());

                    if (CardEffectCommons.HasMatchConditionPermanent(PermanentHasPulsemonInText))
                    {
                        Permanent selectedPermanent = card.PermanentOfThisCard();

                        yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { selectedPermanent }, activateClass).Unsuspend());
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}