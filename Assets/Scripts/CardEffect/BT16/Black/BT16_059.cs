using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT16
{
    public class BT16_059 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            # region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    if (targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 4)
                    {
                        if (targetPermanent.TopCard.HasText("Pulsemon"))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                    permanentCondition: PermanentCondition,
                    digivolutionCost: 3,
                    ignoreDigivolutionRequirement: false,
                    card: card,
                    condition: null));
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("De-Digivolve 1 digimon, then delete 1 digimon ", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());

                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] If you have 3 or more security cards, <De-Digivolve 1> 1 of your opponent's Digimon. If you have 3 or fewer security cards, delete 1 of your opponent's Digimon with a play cost of 6 or less.";
                }

                bool CanSelectDeletePermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.HasPlayCost && permanent.TopCard.GetCostItself <= 6)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectDeDigivolvePermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    if (card.Owner.SecurityCards.Count >= 3)
                    {
                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectDeDigivolvePermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: AfterSelectDedigivolveCoroutine,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator AfterSelectDedigivolveCoroutine(List<Permanent> permanents)
                        {
                            foreach (Permanent permanent in permanents)
                            {
                                yield return ContinuousController.instance.StartCoroutine(new IDegeneration(permanent, 1, activateClass).Degeneration());
                            }
                        }
                    }

                    if (card.Owner.SecurityCards.Count <= 3)
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectDeletePermanentCondition))
                        {
                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectDeletePermanentCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Destroy,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }
                    }
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("De-Digivolve 1 digimon, then delete 1 digimon ", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());

                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] If you have 3 or more security cards, <De-Digivolve 1> 1 of your opponent's Digimon. If you have 3 or fewer security cards, delete 1 of your opponent's Digimon with a play cost of 6 or less.";
                }

                bool CanSelectDeletePermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.TopCard.HasPlayCost && permanent.TopCard.GetCostItself <= 6)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectDeDigivolvePermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    if (card.Owner.SecurityCards.Count >= 3)
                    {
                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectDeDigivolvePermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: AfterSelectDedigivolveCoroutine,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator AfterSelectDedigivolveCoroutine(List<Permanent> permanents)
                        {
                            foreach (Permanent permanent in permanents)
                            {
                                yield return ContinuousController.instance.StartCoroutine(new IDegeneration(permanent, 1, activateClass).Degeneration());
                            }
                        }
                    }

                    if (card.Owner.SecurityCards.Count <= 3)
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectDeletePermanentCondition))
                        {
                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectDeletePermanentCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Destroy,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }
                    }
                }
            }
            #endregion

            #region End Of Attack - ESS
            if (timing == EffectTiming.OnEndAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Unsuspend this Digimon by trashing the top card of your security.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Inherit_BT16_059");
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