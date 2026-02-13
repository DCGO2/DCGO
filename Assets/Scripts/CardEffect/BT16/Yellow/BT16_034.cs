using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT16
{
    public class BT16_034 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digivolve Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasText("Pulsemon") && targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 4;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Minus DP and/or Security -2 to one of your opponent's Digimon.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetHashString("OnPlay_BT16_034");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] If you have 3 or more security cards, 1 of your opponent's Digimon gets -4000 DP until the end of thier turn. If you have 3 or fewer security cards, 1 of your opponent's Digimon gains [Security A-2] until the end of their turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        return true;
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
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
                            if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, PermanentCondition))
                            {
                                return true;
                            }
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
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get DP -4000.", "The opponent is selecting 1 Digimon that will get DP -4000.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: permanent, changeValue: -4000, effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));
                        }
                    }

                    if (card.Owner.SecurityCards.Count <= 3)
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
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get Security Attack -2", "The opponent is selecting 1 Digimon that will get Security Attack -2.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonSAttack(targetPermanent: permanent, changeValue: -2, effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));
                        }
                    }
                }
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Minus DP and/or Security -2 to one of your opponent's Digimon.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetHashString("Digivolving_BT16_034");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] If you have 3 or more security cards, 1 of your opponent's Digimon gets -4000 DP until the end of thier turn. If you have 3 or fewer security cards, 1 of your opponent's Digimon gains [Security A-2] until the end of their turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        return true;
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
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
                            if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, PermanentCondition))
                            {
                                return true;
                            }
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
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get DP -4000.", "The opponent is selecting 1 Digimon that will get DP -4000.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: permanent, changeValue: -4000, effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));
                        }
                    }

                    if (card.Owner.SecurityCards.Count <= 3)
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
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get Security Attack -2", "The opponent is selecting 1 Digimon that will get Security Attack -2.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonSAttack(targetPermanent: permanent, changeValue: -2, effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));
                        }
                    }
                }
            }

            #endregion

            #region Inherit

            if (timing == EffectTiming.OnEndAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Unsuspend this Digimon by trashing the top card of your security.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Inherit_BT16_034");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Attack] [Once per turn] If this Digimon has [Pulsemon] in its text, by trashing the top card of your security stack, unsuspend this Digimon.";
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

                bool PermanentCondition(Permanent permanent)
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

                    if (CardEffectCommons.HasMatchConditionPermanent(PermanentCondition))
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