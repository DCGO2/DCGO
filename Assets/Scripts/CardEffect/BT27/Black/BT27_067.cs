using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Artiomon / Doom Buster
namespace DCGO.CardEffects.BT27
{
    public class BT27_067 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digimon Effects
            #region Alternative Digivolution Condition
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasGlowingDawnTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 3, false, card, null, level: 5));
            }
            #endregion

            #region Engage
            if (timing == EffectTiming.OnEndTurn)
            {
                cardEffects.Add(CardEffectFactory.EngageSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Raid
            if (timing == EffectTiming.OnAllyAttack)
            {
                cardEffects.Add(CardEffectFactory.RaidSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Piercing
            if (timing == EffectTiming.OnDetermineDoSecurityCheck)
            {
                cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region All Turns OPT - Would leave the battle area
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 2 face-down cards under Tamers, this Digimon doesn't leave", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("BT27_067_Leave");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] [Once Per Turn] When this Digimon would leave the battle area, by trashing 2 bottom face-down cards from under any of your Tamers, it doesn't leave.";
                }

                bool IsThisPermanent(Permanent permanent)
                {
                    return permanent != null && permanent == card.PermanentOfThisCard();
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, IsThisPermanent);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimonActivate(card, activateClass)
                        && (CardEffectCommons.HasMatchConditionPermanent(TamerWith2OrMoreFaceDownSources)
                            || CardEffectCommons.MatchConditionPermanentCount(TamerWithOneFaceDownSource) >= 2);
                }

                bool TamerWithOneFaceDownSource(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                        && permanent.DigivolutionCards.Any(FaceDownCards)
                        && !permanent.ImmuneFromStackTrashing(activateClass);

                bool TamerWith2OrMoreFaceDownSources(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                        && permanent.DigivolutionCards.Count(FaceDownCards) >= 2
                        && !permanent.ImmuneFromStackTrashing(activateClass);

                bool FaceDownCards(CardSource cardSource) => cardSource.IsFaceDown;

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    Permanent thisPermanent = card.PermanentOfThisCard();

                    bool paid = false;
                    if (CardEffectCommons.HasMatchConditionPermanent(TamerWith2OrMoreFaceDownSources)
                                            || CardEffectCommons.MatchConditionPermanentCount(TamerWithOneFaceDownSource) >= 2)
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                        int maxCount = Math.Min(2, CardEffectCommons.MatchConditionPermanentCount(TamerWithOneFaceDownSource));

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: TamerWithOneFaceDownSource,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: CanEndSelectCondition,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: true,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select all Tamer(s) to trash bottom face-down cards from.", "The opponent is selecting Tamer(s) to trash bottom face-down cards from.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        bool CanEndSelectCondition(List<Permanent> permanents)
                        {
                            return permanents.Count == 2
                                || (permanents.Count > 0 && permanents[0].DigivolutionCards.Count(FaceDownCards) >= 2);
                        }

                        IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                        {
                            if (permanents.Count == 1)
                            {
                                paid = true;
                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: permanents[0], trashCount: 2, isFromTop: false, activateClass: activateClass, cardCondition: FaceDownCards));
                            }
                            else if (permanents.Count == 2)
                            {
                                paid = true;
                                foreach (Permanent selectedPermanent in permanents)
                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(targetPermanent: selectedPermanent, trashCount: 1, isFromTop: false, activateClass: activateClass, cardCondition: FaceDownCards));
                            }
                        }
                    }

                    if (paid && thisPermanent != null)
                    {
                        thisPermanent.willBeRemoveField = false;
                        thisPermanent.HideDeleteEffect();
                        thisPermanent.HideHandBounceEffect();
                        thisPermanent.HideDeckBounceEffect();
                    }
                }
            }
            #endregion

            #region All Turns OPT - Activate Option side Main
            if (timing == EffectTiming.OnAttackTargetChanged || timing == EffectTiming.OnDigivolutionCardDiscarded)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Activate [Main] effect on this card's Option side", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("BT27_067_ActivateOption");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] [Once Per Turn] When attack targets change or effects trash cards from under your Tamers, you may activate 1 [Main] effect on this card's Option side.";

                bool OwnTamerCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && ((timing == EffectTiming.OnAttackTargetChanged
                            && CardEffectCommons.CanTriggerOnAttackTargetSwitch(hashtable, card))
                        || (timing == EffectTiming.OnDigivolutionCardDiscarded
                            && CardEffectCommons.CanTriggerOnTrashDigivolutionCard(hashtable, OwnTamerCondition, cardEffect => cardEffect != null, _ => true)));

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ActivateMainOfOptionSide(card, activateClass));
                }
            }
            #endregion
            #endregion

            #region Option Effects
            #region Use Req
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.UseRequirements(card, cardSource => cardSource.HasGlowingDawnTraits));
            }
            #endregion

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("<De-Digivolve 2> 1 opponent's Digimon, 1 of your Digimon unsuspends and gets +5000 DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsOptionEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Main] <De-Digivolve 2> 1 of your opponent's Digimon. Then, 1 of your Digimon unsuspends and gets +5000 DP until your turn ends.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                bool CanSelectOpponentDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanSelectOwnDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    #region De-Digivolve 2
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOpponentDigimon))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();
                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectOpponentDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Degenerate,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetDegenerationCount(2);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 opponent's Digimon to De-Digivolve", "The opponent is selecting 1 Digimon to De-Digivolve");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                    }
                    #endregion

                    #region Unsuspend + 5000 DP
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectOwnDigimon))
                    {
                        SelectPermanentEffect selectOwn = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectOwn.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectOwnDigimon,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectOwnCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectOwn.SetUpCustomMessage("Select 1 Digimon to unsuspend and get +5000 DP.", "The opponent is selecting 1 Digimon to unsuspend and get +5000 DP.");

                        IEnumerator SelectOwnCoroutine(Permanent permanent)
                        {
                            if (permanent != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(
                                    permanents: new List<Permanent>() { permanent },
                                    cardEffect: activateClass).Unsuspend());

                                yield return ContinuousController.instance.StartCoroutine(
                                    CardEffectCommons.ChangeDigimonDP(
                                        targetPermanent: permanent,
                                        changeValue: 5000,
                                        effectDuration: EffectDuration.UntilOwnerTurnEnd,
                                        activateClass: activateClass));
                            }
                        }

                        yield return ContinuousController.instance.StartCoroutine(selectOwn.Activate());
                    }
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
