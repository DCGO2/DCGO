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

            #region Shared helper: trash N bottom face-down cards from under your Tamers
            bool CanSelectTamerWithFaceDown(Permanent permanent)
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                    && permanent.DigivolutionCards.Any(c => c.IsFlipped);
            }

            int FaceDownCountUnderTamers()
            {
                return card.Owner.GetBattleAreaPermanents()
                    .Where(p => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(p, card))
                    .Sum(p => p.DigivolutionCards.Count(c => c.IsFlipped));
            }

            // Trashes `count` bottom face-down cards, one Tamer pick per card. onResult(true) only if all were trashed.
            IEnumerator PayFaceDownCost(ActivateClass activateClass, int count, Action<bool> onResult)
            {
                int trashedCount = 0;

                for (int i = 0; i < count; i++)
                {
                    if (!CardEffectCommons.HasMatchConditionPermanent(CanSelectTamerWithFaceDown))
                    {
                        break;
                    }

                    bool picked = false;
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectTamerWithFaceDown,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: i == 0,
                        canEndNotMax: i == 0,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage(
                        "Select 1 Tamer to trash its bottom face-down card.",
                        "The opponent is selecting 1 Tamer to trash its bottom face-down card.");

                    IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                    {
                        if (permanents.Count >= 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(
                                CardEffectCommons.TrashDigivolutionCardsFromTopOrBottom(
                                    targetPermanent: permanents[0],
                                    trashCount: 1,
                                    isFromTop: false,
                                    activateClass: activateClass,
                                    cardCondition: c => c.IsFlipped));

                            picked = true;
                        }
                    }

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    if (!picked)
                    {
                        break;
                    }

                    trashedCount++;
                }

                onResult(trashedCount >= count);
            }
            #endregion

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
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, IsThisPermanent);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && FaceDownCountUnderTamers() >= 2;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    Permanent thisPermanent = card.PermanentOfThisCard();

                    bool paid = false;
                    yield return ContinuousController.instance.StartCoroutine(PayFaceDownCost(activateClass, 2, result => paid = result));

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
            string OptionTriggerName = "Activate [Main] effect on this card's Option side";
            string OptionTriggerHash = "BT27_067_ActivateOption";
            string OptionTriggerDescription = "[All Turns] [Once Per Turn] When attack targets change or effects trash cards from under your Tamers, you may activate 1 [Main] effect on this card's Option side.";

            ActivateClass MakeOptionTrigger(Func<Hashtable, bool> canUse)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(OptionTriggerName, canUse, card);
                activateClass.SetUpActivateClass(CanActivateOptionCondition, ActivateOptionCoroutine, 1, true, OptionTriggerDescription);
                activateClass.SetHashString(OptionTriggerHash);

                bool CanActivateOptionCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateOptionCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ActivateMainOfOptionSide(card, activateClass));
                }

                return activateClass;
            }

            if (timing == EffectTiming.OnAttackTargetChanged)
            {
                cardEffects.Add(MakeOptionTrigger(hashtable =>
                    CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                    && CardEffectCommons.CanTriggerOnPermanentAttackTargetSwitch(hashtable, permanent => true)));
            }

            if (timing == EffectTiming.OnDigivolutionCardDiscarded)
            {
                cardEffects.Add(MakeOptionTrigger(hashtable =>
                    CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                    && CardEffectCommons.CanTriggerOnTrashDigivolutionCard(
                        hashtable,
                        permanent => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card),
                        cardEffect => cardEffect != null,
                        cardSource => true)));
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
                            selectPermanentCoroutine: SelectOpponentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to <De-Digivolve 2>.", "The opponent is selecting 1 Digimon to <De-Digivolve 2>.");

                        IEnumerator SelectOpponentCoroutine(Permanent permanent)
                        {
                            if (permanent != null)
                            {
                                yield return ContinuousController.instance.StartCoroutine(
                                    new IDegeneration(permanent, 2, activateClass).Degeneration());
                            }
                        }

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
