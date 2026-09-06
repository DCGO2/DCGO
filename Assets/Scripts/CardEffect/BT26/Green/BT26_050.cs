using System;
using System.Collections;
using System.Collections.Generic;

// Rosemon: Burst Mode/Aguichant Lèvres
namespace DCGO.CardEffects
{
    public class BT26_050 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digimon Effects
            #region Alt Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.TopCard.HasDataSquadTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 5, false, card, null, level: 6));
            }
            #endregion

            #region Burst Digivolution
            if (timing == EffectTiming.None)
            {
                AddBurstDigivolutionConditionClass addBurstDigivolutionConditionClass = new AddBurstDigivolutionConditionClass();
                addBurstDigivolutionConditionClass.SetUpICardEffect($"Burst Digivolution", CanUseCondition, card);
                addBurstDigivolutionConditionClass.SetUpAddBurstDigivolutionConditionClass(getBurstDigivolutionCondition: GetBurstDigivolution);
                addBurstDigivolutionConditionClass.SetNotShowUI(true);
                cardEffects.Add(addBurstDigivolutionConditionClass);

                bool CanUseCondition(Hashtable hashtable) => true;

                BurstDigivolutionCondition GetBurstDigivolution(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        bool tamerCondition(Permanent permanent)
                        {
                            return permanent != null
                                && permanent.TopCard != null
                                && permanent.TopCard.Owner == card.Owner
                                && permanent.TopCard.Owner.GetBattleAreaPermanents().Contains(permanent)
                                && !permanent.CannotReturnToHand(null)
                                && permanent.TopCard.EqualsCardName("Yoshino Fujieda");
                        }

                        bool digimonCondition(Permanent permanent)
                        {
                            return permanent != null
                                && permanent.TopCard != null
                                && permanent.TopCard.Owner == card.Owner
                                && permanent.TopCard.Owner.GetFieldPermanents().Contains(permanent)
                                && !card.CanNotEvolve(permanent)
                                && permanent.TopCard.EqualsCardName("Rosemon");
                        }

                        BurstDigivolutionCondition burstDigivolutionCondition = new BurstDigivolutionCondition(
                            tamerCondition: tamerCondition,
                            selectTamerMessage: "1 [Yoshino Fujieda]",
                            digimonCondition: digimonCondition,
                            selectDigimonMessage: "1 [Rosemon]",
                            cost: 0);

                        return burstDigivolutionCondition;
                    }

                    return null;
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend 2 opponent's Digimon or Tamers, then 2 opponent Digimon or Tamers can't unsuspend until their turn ends", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDiscription() => "You may suspend 2 Digimon or Tamers. Then, 2 of your opponent's Digimon or Tamers can't unsuspend until their turn ends.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable) => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);

                bool CanSelectPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                    && (permanent.IsDigimon || permanent.IsTamer);

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    int maxCount = Math.Min(2, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                    if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Tap,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage($"Select {maxCount} Digimon or Tamer(s) to suspend.", $"The opponent is selecting {maxCount} Digimon or Tamer(s) to suspend.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }

                    if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, CanSelectPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage($"Select {maxCount}  Digimon or Tamer(s) to not unsuspend until their turn ends.", $"The opponent is selecting {maxCount} Digimon or Tamer(s) to not unsuspend until their turn ends.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator AfterSelectPermanentCoroutine(List<Permanent> selectedPermanents)
                        {
                            foreach (Permanent permanent in selectedPermanents)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotUnsuspend(
                                    targetPermanent: permanent,
                                    effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                    activateClass: activateClass,
                                    condition: null,
                                    effectName: "Cannot Unsuspend"));
                            }

                            yield return null;
                        }
                    }
                }
            }
            #endregion

            #region WD/WA Shared
            string SharedEffectName = "By bottom decking 1 other suspended digimon, trash opponent top security card.";

            string SharedEffectDescription(string tag) => $"[{tag}] By returning 1 other suspended Digimon to the bottom of the deck, trash your opponent's top security card.";

            bool AdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass)
            {
                return CardEffectCommons.HasMatchConditionPermanent(CanSelectReturnPermanentCondition);
            }

            bool CanSelectReturnPermanentCondition(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnBattleAreaDigimon(permanent)
                    && permanent.IsSuspended
                    && permanent != card.PermanentOfThisCard();
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                Permanent selectedPermanent = null;

                #region Select Suspended Digimon
                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                selectPermanentEffect.SetUp(
                    selectPlayer: card.Owner,
                    canTargetCondition: CanSelectReturnPermanentCondition,
                    canTargetCondition_ByPreSelecetedList: null,
                    canEndSelectCondition: null,
                    maxCount: 1,
                    canNoSelect: true,
                    canEndNotMax: false,
                    selectPermanentCoroutine: SelectPermanentCoroutine,
                    afterSelectPermanentCoroutine: null,
                    mode: SelectPermanentEffect.Mode.Custom,
                    cardEffect: activateClass);


                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                {
                    selectedPermanent = permanent;
                    yield return null;
                }

                selectPermanentEffect.SetUpCustomMessage("Select 1 other Digimon to bottom deck.", "The opponent is selecting 1 other Digimon to bottom deck.");
                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                #endregion

                if (selectedPermanent != null)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeckBouncePeremanentAndProcessAccordingToResult(
                        targetPermanents: new List<Permanent> { selectedPermanent },
                        activateClass: activateClass,
                        successProcess: SuccessProcess(),
                        failureProcess: null));

                    IEnumerator SuccessProcess()
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(card.Owner.Enemy, 1, activateClass, true).DestroySecurity());
                    }
                }
            }

            CardEffectFactory.ActivateClassesForSharedEffects
            (ref cardEffects, timing, card,
                SharedEffectName,
                SharedActivateCoroutine,
                SharedEffectDescription,
                additionalActivateCondition: AdditionalActivateCondition,
                optional: false,
                isSkippable: true,
                whenDigivolving: true,
                whenAttacking: true);
            #endregion
            #endregion

            #region Option Effects
            #region Ignore Colour Requirement
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.UseRequirements(card, CardCondition));

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource.HasDataSquadTraits;
                }
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend 2 opponent's Digimon or Tamers, then opponent suspended Digimon or Tamers can't unsuspend until their turn ends", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                    => "[Main] Suspend 2 of your opponent's Digimon or Tamers. Then, until their turn ends, none of their suspended Digimon or Tamers can digivolve or unsuspend.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

                bool CanSelectSuspendOpponentPermamentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                    && (permanent.IsDigimon || permanent.IsTamer);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    #region Suspend 2 Opponent's Digimon or Tamers
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectSuspendOpponentPermamentCondition))
                    {
                        int maxCount = Math.Min(2, CardEffectCommons.MatchConditionPermanentCount(CanSelectSuspendOpponentPermamentCondition));
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectSuspendOpponentPermamentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Tap,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage($"Select { maxCount } Digimon or Tamer(s) to suspend.", $"The opponent is selecting { maxCount } Digimon or Tamer(s) to suspend.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                    #endregion

                    bool CanNotUnSuspendOrDigivolveCondition(Permanent permanent)
                    {
                        return CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(permanent, card)
                            && (permanent.IsDigimon || permanent.IsTamer)
                            && permanent.IsSuspended;
                    }

                    #region Digivolve prevention
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotDigivolvePlayerEffect(
                        permanentCondition: CanNotUnSuspendOrDigivolveCondition,
                        cardCondition: null,
                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                        activateClass: activateClass,
                        isOnlyActivePhase: false,
                        effectName: "Cannot Digivolve"));
                    #endregion

                    #region Stun Opponent's Digimon or Tamers
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotUnsuspendPlayerEffect(
                        permanentCondition: CanNotUnSuspendOrDigivolveCondition,
                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                        activateClass: activateClass,
                        isOnlyActivePhase: false,
                        effectName: "Cannot Unsuspend"));
                    #endregion
                }
            }
            #endregion

            #region Arts Digivolution
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ArtsDigivolveEffect(card));
            }
            #endregion
            #endregion

            return cardEffects;
        }
    }
}
