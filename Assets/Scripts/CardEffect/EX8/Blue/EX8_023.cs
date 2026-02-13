using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX8
{
    public class EX8_023 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.EqualsTraits("Ice-Snow") && targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 4;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Iceclad

            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.IcecladSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }

            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash any 2 sources of opponents digimon. Then, 1 of your opponent's sourceless Digimon can't suspend or activate [When Digivolving]", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Trash any 2 digivolution cards from your opponent's Digimon. Then, 1 of your opponent's Digimon with no digivolution cards can't suspend or activate [When Digivolving] effects until the end of their turn.";
                }

                bool OpponentsDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return !cardSource.CanNotTrashFromDigivolutionCards(activateClass);
                }

                bool OpponentsDigimonWithoutSources(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) &&
                           permanent.DigivolutionCards.Count == 0;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.HasMatchConditionPermanent(OpponentsDigimon);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashDigivolutionCards(
                        permanentCondition: OpponentsDigimon,
                        cardCondition: CanSelectCardCondition,
                        maxCount: 2,
                        canNoTrash: false,
                        isFromOnly1Permanent: false,
                        activateClass: activateClass
                    ));

                    if (CardEffectCommons.HasMatchConditionPermanent(OpponentsDigimonWithoutSources))
                    {
                        Permanent selectedPermanent = null;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: OpponentsDigimonWithoutSources,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will gain effects.", "The opponent is selecting 1 Digimon that will gain effects.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            if (permanent != null)
                                selectedPermanent = permanent;

                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            CanNotSuspendClass canNotSuspendClass = new CanNotSuspendClass();
                            canNotSuspendClass.SetUpICardEffect("Can't Suspend", CanUseCondition1, card);
                            canNotSuspendClass.SetUpCanNotSuspendClass(PermanentCondition: PermanentCondition);
                            selectedPermanent.UntilOwnerTurnEndEffects.Add((_timing) => canNotSuspendClass);

                            DisableEffectClass invalidationClass = new DisableEffectClass();
                            invalidationClass.SetUpICardEffect("Ignore [When Digivolving] Effect", CanUseCondition1, card);
                            invalidationClass.SetUpDisableEffectClass(DisableCondition: InvalidateCondition);
                            selectedPermanent.UntilOwnerTurnEndEffects.Add((_timing) => invalidationClass);

                            if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                            {
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));
                            }

                            bool CanUseCondition1(Hashtable hashtable)
                            {
                                if (selectedPermanent.TopCard != null)
                                {
                                    if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                    {
                                        return true;
                                    }
                                }

                                return false;
                            }

                            bool PermanentCondition(Permanent permanent)
                            {
                                if (permanent == selectedPermanent)
                                {
                                    return true;
                                }

                                return false;
                            }

                            bool InvalidateCondition(ICardEffect cardEffect)
                            {
                                if (cardEffect != null)
                                {
                                    if (cardEffect is ActivateICardEffect)
                                    {
                                        if (cardEffect.EffectSourceCard != null)
                                        {
                                            if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(cardEffect.EffectSourceCard.PermanentOfThisCard(), card))
                                            {
                                                if(cardEffect.EffectSourceCard.PermanentOfThisCard() == selectedPermanent)
                                                {
                                                    if (!cardEffect.EffectSourceCard.PermanentOfThisCard().TopCard.CanNotBeAffected(invalidationClass))
                                                    {
                                                        if (cardEffect.IsWhenDigivolving)
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

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash bottom 2 sources of 1 opponents digimon. Then, 1 of your opponent's Digimon can't suspend or activate [When Digivolving]", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Trash any 2 digivolution cards from your opponent's Digimon. Then, 1 of your opponent's Digimon with no digivolution cards can't suspend or activate [When Digivolving] effects until the end of their turn.";
                }

                bool OpponentsDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return !cardSource.CanNotTrashFromDigivolutionCards(activateClass);
                }

                bool OpponentsDigimonWithoutSources(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) &&
                           permanent.DigivolutionCards.Count == 0;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card) &&
                           CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.HasMatchConditionPermanent(OpponentsDigimon);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SelectTrashDigivolutionCards(
                        permanentCondition: OpponentsDigimon,
                        cardCondition: CanSelectCardCondition,
                        maxCount: 2,
                        canNoTrash: false,
                        isFromOnly1Permanent: false,
                        activateClass: activateClass
                    ));

                    if (CardEffectCommons.HasMatchConditionPermanent(OpponentsDigimonWithoutSources))
                    {
                        Permanent selectedPermanent = null;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: OpponentsDigimonWithoutSources,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will gain effects.", "The opponent is selecting 1 Digimon that will will gain effects.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            if (permanent != null)
                                selectedPermanent = permanent;

                            yield return null;
                        }

                        if(selectedPermanent != null)
                        {
                            CanNotSuspendClass canNotSuspendClass = new CanNotSuspendClass();
                            canNotSuspendClass.SetUpICardEffect("Can't Suspend", CanUseCondition1, card);
                            canNotSuspendClass.SetUpCanNotSuspendClass(PermanentCondition: PermanentCondition);
                            selectedPermanent.UntilOwnerTurnEndEffects.Add((_timing) => canNotSuspendClass);

                            DisableEffectClass invalidationClass = new DisableEffectClass();
                            invalidationClass.SetUpICardEffect("Ignore [When Digivolving] Effect", CanUseCondition, card);
                            invalidationClass.SetUpDisableEffectClass(DisableCondition: InvalidateCondition);
                            selectedPermanent.UntilOwnerTurnEndEffects.Add((_timing) => invalidationClass);

                            if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                            {
                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));
                            }

                            bool CanUseCondition1(Hashtable hashtable)
                            {
                                if (selectedPermanent.TopCard != null)
                                {
                                    if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                    {
                                        return true;
                                    }
                                }

                                return false;
                            }

                            bool PermanentCondition(Permanent permanent)
                            {
                                if (permanent == selectedPermanent)
                                {
                                    return true;
                                }

                                return false;
                            }

                            bool InvalidateCondition(ICardEffect cardEffect)
                            {
                                if (cardEffect != null)
                                {
                                    if (cardEffect is ActivateICardEffect)
                                    {
                                        if (cardEffect.EffectSourceCard != null)
                                        {
                                            if (CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(cardEffect.EffectSourceCard.PermanentOfThisCard(), card))
                                            {
                                                if (cardEffect.EffectSourceCard.PermanentOfThisCard() == selectedPermanent)
                                                {
                                                    if (!cardEffect.EffectSourceCard.PermanentOfThisCard().TopCard.CanNotBeAffected(invalidationClass))
                                                    {
                                                        if (cardEffect.IsWhenDigivolving)
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

            #region Your Turn
            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (card.PermanentOfThisCard().TopCard.EqualsTraits("Ice-Snow"))
                            {
                                if (!CardEffectCommons.HasMatchConditionOpponentsPermanent(card, (permanent) =>
                                permanent.IsDigimon && !permanent.HasNoDigivolutionCards))
                                {
                                    return true;
                                }
                            }                            
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(1, true, card, Condition));
            }

            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (card.PermanentOfThisCard().TopCard.EqualsTraits("Ice-Snow"))
                            {
                                if (!CardEffectCommons.HasMatchConditionOpponentsPermanent(card, (permanent) =>
                                permanent.IsDigimon && !permanent.HasNoDigivolutionCards))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: true, card: card, condition: Condition));
            }
            #endregion

            return cardEffects;
        }
    }
}
