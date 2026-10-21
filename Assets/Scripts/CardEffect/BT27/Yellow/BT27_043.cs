using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Kentaurosmon (X Antibody)/Ultimate Crossbow Einherjar
namespace DCGO.CardEffects.BT27
{
    public class BT27_043 : CEntity_Effect
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
                    return permanent.TopCard.EqualsCardName("Kentaurosmon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 2, false, card, null));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Barrier
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.BarrierSelfEffect(false, card, null));
            }
            #endregion

            #region Ascension
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                cardEffects.Add(CardEffectFactory.AscensionSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Shared WD/WA
            string SharedHashString = "BT27_043_WD_WA";

            string SharedEffectName = "By trashing your top security card, this Digimon unsuspends";

            string SharedEffectDescription(string tag) => $"[{tag}] [Once Per Turn] By trashing your top security card, this Digimon unsuspends.";

            bool SharedCanActivateCondition(Hashtable hashtable, ActivateClass activateClass)
            {
                return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                    && card.Owner.SecurityCards.Count >= 1;
            }

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashSecurityAndProcessAccordingToResult(
                    player: card.Owner,
                    trashAmount: 1,
                    activateClass: activateClass,
                    fromTop: true,
                    successProcess: SuccessProcess,
                    failureProcess: null));

                IEnumerator SuccessProcess(List<CardSource> cardSources)
                {
                    Permanent thisPermanent = card.PermanentOfThisCard();

                    if (thisPermanent != null && CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(
                            permanents: new List<Permanent>() { thisPermanent },
                            cardEffect: activateClass).Unsuspend());
                    }
                }
            }

            CardEffectFactory.ActivateClassesForSharedEffects
                (ref cardEffects, timing, card,
                    SharedEffectName,
                    SharedActivateCoroutine,
                    SharedEffectDescription,
                    optional: true,
                    additionalActivateCondition: SharedCanActivateCondition,
                    maxCountPerTurn: 1,
                    hashValue: SharedHashString,
                    whenDigivolving: true,
                    whenAttacking: true);
            #endregion

            #region All Turns - Security Removed
            if (timing == EffectTiming.OnLoseSecurity)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place 1 [Kentaurosmon]/[X Antibody] from digivolution cards as bottom security, activate Option side Main", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("BT27_043_AT");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] [Once Per Turn] When your security stack is removed from, by placing 1 [Kentaurosmon] or [X Antibody] from this Digimon's digivolution cards as the bottom security card, activate 1 [Main] effect on this card's Option side.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    Permanent thisPermanent = card.PermanentOfThisCard();

                    return thisPermanent != null
                        && thisPermanent.DigivolutionCards.Contains(cardSource)
                        && (cardSource.EqualsCardName("Kentaurosmon") || cardSource.EqualsCardName("X Antibody"));
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenLoseSecurity(hashtable, player => player == card.Owner);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass)
                        && card.Owner.CanAddSecurity(activateClass)
                        && card.PermanentOfThisCard().DigivolutionCards.Count(CanSelectCardCondition) >= 1;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool placedInSecurity = false;

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectCardCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: () => false,
                        canEndNotMax: false,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 card to place as the bottom security card.",
                        isShowOpponent: true,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.DigivolutionCards,
                        customRootCardList: card.PermanentOfThisCard().DigivolutionCards,
                        canLookReverseCard: false,
                        cardEffect: activateClass);

                    selectCardEffect.SetUpCustomMessage("Select 1 card to place as the bottom security card.", "The opponent is selecting 1 card to place as the bottom security card.");

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                    IEnumerator SelectCardCoroutine(CardSource cardSource)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(cardSource, toTop: false));

                        placedInSecurity = true;
                    }

                    if (placedInSecurity)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ActivateMainOfOptionSide(card, activateClass));
                    }
                }
            }
            #endregion

            #endregion

            #region Option Effects

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("1 opponent's Digimon gets -20K DP until their turn ends, -1K more per security card", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsOptionEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Main] 1 of your opponent's Digimon gets -20000 DP until their turn ends. For each card in both players' security stacks, add 1000 to this effect's DP reduction.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to reduce its DP.", "The opponent is selecting 1 Digimon to reduce its DP.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            if (permanent != null)
                            {
                                // Counted when the effect resolves: every card in both players' security stacks adds 1000 to the reduction.
                                int securityCount = card.Owner.SecurityCards.Count + card.Owner.Enemy.SecurityCards.Count;
                                int dpReduction = 20000 + 1000 * securityCount;

                                yield return ContinuousController.instance.StartCoroutine(
                                    CardEffectCommons.ChangeDigimonDP(
                                        targetPermanent: permanent,
                                        changeValue: -dpReduction,
                                        effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                        activateClass: activateClass));
                            }
                        }
                    }
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
