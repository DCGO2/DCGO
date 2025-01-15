using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.LM
{
    public class LM_024 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasText("Pulsemon") && targetPermanent.TopCard.HasLevel &&
                           targetPermanent.TopCard.Level == 5;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition,
                    digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region Ace - Blast Digivolve

            if (timing == EffectTiming.OnCounterTiming)
            {
                cardEffects.Add(CardEffectFactory.BlastDigivolveEffect(card: card, condition: null));
            }

            #endregion

            #region On Play/When Digivolving Shared

            bool PermanentSuspendCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent) &&
                       permanent.CanSuspend;
            }

            bool PermanentGetDPCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
            }

            bool PermanentBottomDeckCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card) &&
                       permanent.IsSuspended;
            }

            bool CanActivateConditionShared(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                {
                    if (card.Owner.SecurityCards.Count >= 3)
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(PermanentSuspendCondition))
                        {
                            return true;
                        }

                        if (CardEffectCommons.HasMatchConditionPermanent(PermanentGetDPCondition))
                        {
                            return true;
                        }
                    }

                    if (card.Owner.SecurityCards.Count <= 3)
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(PermanentBottomDeckCondition))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend & get +3000 DP and/or Bottom deck opponent's digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[On Play] If you have 3 or more security cards, suspend 1 Digimon, and 1 of your Digimon gets +3000 DP until the end of your opponent's turn. If you have 3 or fewer security cards, return 1 of your opponent’s suspended Digimon to the bottom of the deck.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.SecurityCards.Count >= 3)
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(PermanentSuspendCondition))
                        {
                            Permanent selectedPermanent = null;

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: PermanentSuspendCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                selectedPermanent = permanent;

                                yield return null;
                            }

                            if (selectedPermanent != null &&
                                selectedPermanent.TopCard &&
                                !selectedPermanent.TopCard.CanNotBeAffected(activateClass) &&
                                !selectedPermanent.IsSuspended && selectedPermanent.CanSuspend)
                            {
                                yield return ContinuousController.instance.StartCoroutine(
                                    new SuspendPermanentsClass(new List<Permanent>() { selectedPermanent },
                                        CardEffectCommons.CardEffectHashtable(activateClass)).Tap());
                            }
                        }

                        if (CardEffectCommons.HasMatchConditionPermanent(PermanentGetDPCondition))
                        {
                            Permanent selectedPermanent = null;

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: PermanentGetDPCondition,
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
                                "Select 1 of your Digimon that will get DP +3000.",
                                "The opponent is selecting 1 of their Digimon that will get DP +3000.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                selectedPermanent = permanent;

                                yield return null;
                            }

                            if (selectedPermanent != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                    targetPermanent: selectedPermanent,
                                    changeValue: 3000,
                                    effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                    activateClass: activateClass));
                            }
                        }
                    }

                    if (card.Owner.SecurityCards.Count <= 3)
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(PermanentBottomDeckCondition))
                        {
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition_ByPreSelecetedList: null,
                                canTargetCondition: PermanentBottomDeckCondition,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 suspended Digimon to bottom deck.",
                                "The opponent is selecting 1 suspended Digimon to bottom deck.");

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
                activateClass.SetUpICardEffect("Suspend & get +3000 DP and/or Bottom deck opponent's digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateConditionShared, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Digivolving] If you have 3 or more security cards, suspend 1 Digimon, and 1 of your Digimon gets +3000 DP until the end of your opponent's turn. If you have 3 or fewer security cards, return 1 of your opponent’s suspended Digimon to the bottom of the deck.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.SecurityCards.Count >= 3)
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(PermanentSuspendCondition))
                        {
                            Permanent selectedPermanent = null;

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: PermanentSuspendCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: SelectPermanentCoroutine,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.Custom,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                selectedPermanent = permanent;

                                yield return null;
                            }

                            if (selectedPermanent != null &&
                                selectedPermanent.TopCard &&
                                !selectedPermanent.TopCard.CanNotBeAffected(activateClass) &&
                                !selectedPermanent.IsSuspended && selectedPermanent.CanSuspend)
                            {
                                yield return ContinuousController.instance.StartCoroutine(
                                    new SuspendPermanentsClass(new List<Permanent>() { selectedPermanent },
                                        CardEffectCommons.CardEffectHashtable(activateClass)).Tap());
                            }
                        }

                        if (CardEffectCommons.HasMatchConditionPermanent(PermanentGetDPCondition))
                        {
                            Permanent selectedPermanent = null;

                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: PermanentGetDPCondition,
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
                                "Select 1 of your Digimon that will get DP +3000.",
                                "The opponent is selecting 1 of their Digimon that will get DP +3000.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                            IEnumerator SelectPermanentCoroutine(Permanent permanent)
                            {
                                selectedPermanent = permanent;

                                yield return null;
                            }

                            if (selectedPermanent != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                                    targetPermanent: selectedPermanent,
                                    changeValue: 3000,
                                    effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                    activateClass: activateClass));
                            }
                        }
                    }

                    if (card.Owner.SecurityCards.Count <= 3)
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(PermanentBottomDeckCondition))
                        {
                            SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                            selectPermanentEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition_ByPreSelecetedList: null,
                                canTargetCondition: PermanentBottomDeckCondition,
                                canEndSelectCondition: null,
                                maxCount: 1,
                                canNoSelect: false,
                                canEndNotMax: false,
                                selectPermanentCoroutine: null,
                                afterSelectPermanentCoroutine: null,
                                mode: SelectPermanentEffect.Mode.PutLibraryBottom,
                                cardEffect: activateClass);

                            selectPermanentEffect.SetUpCustomMessage("Select 1 suspended Digimon to bottom deck.",
                                "The opponent is selecting 1 suspended Digimon to bottom deck.");

                            yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                        }
                    }
                }
            }

            #endregion

            #region All Turns

            if (timing == EffectTiming.None)
            {
                CanNotAffectedClass canNotAffectedClass = new CanNotAffectedClass();
                canNotAffectedClass.SetUpICardEffect("Isn't affected by opponent's Digimon's effects", CanUseCondition, card);
                canNotAffectedClass.SetUpCanNotAffectedClass(CardCondition: CardCondition, SkillCondition: SkillCondition);
                cardEffects.Add(canNotAffectedClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           card.PermanentOfThisCard().IsSuspended;
                }

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource == card &&
                           CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           cardSource == card.PermanentOfThisCard().TopCard;
                }

                bool SkillCondition(ICardEffect cardEffect)
                {
                    return CardEffectCommons.IsOpponentEffect(cardEffect, card) && cardEffect.IsDigimonEffect;
                }
            }

            #endregion

            return cardEffects;
        }
    }
}