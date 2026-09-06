using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Sirenmon
namespace DCGO.CardEffects.BT25
{
    public class BT25_039 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasTSTraits;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(level: 4, permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region End of your Turn - Security
            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new();
                activateClass.SetUpICardEffect("May play [Ceresmon] for -7, then may place this under played card", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "(Security) [End of Your Turn] You may play 1 [Ceresmon] from your hand with the cost reduced by 7. If this effect played, you may place this card as the played Digimon's bottom digivolution card.";

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistInSecurity(card, false)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistInSecurity(card, false)
                        && CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectCardCondition);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.HasPlayCost
                        && cardSource.EqualsCardName("Ceresmon")
                        && CardEffectCommons.CanPlayAsNewPermanent(cardSource, true, activateClass, fixedCost: cardSource.GetCostItself - 7);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    CardSource selectedCard = null;

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayByEffect(
                        canTargetCondition: CanSelectCardCondition,
                        SelectCardEffect.Root.Hand,
                        activateClass,
                        payCost: true,
                        reduceCostTuple: (7, null),
                        afterSelectCardCoroutine: AfterSelectCardCoroutine
                    ));

                    IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                    {
                        if (cardSources.Count > 0)
                            selectedCard = cardSources[0];

                        yield return null;
                    }

                    if (selectedCard != null)
                    {
                        Permanent playedPermanent = selectedCard.PermanentOfThisCard();

                        if (playedPermanent != null)
                        {
                            string selectPlayerMessage = "Will you place Sirenmon under Ceresmon?";
                            string notSelectPlayerMessage = "The opponent is choosing if they will place Sirenmon under Ceresmon.";

                            List<SelectionElement<bool>> command_SelectCommands = new List<SelectionElement<bool>>()
                            {
                                new SelectionElement<bool>(message: $"Yes", value: true, spriteIndex: 0),
                                new SelectionElement<bool>(message: $"No", value: false, spriteIndex: 1),
                            };

                            GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: command_SelectCommands, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                            if (GManager.instance.userSelectionManager.SelectedBoolValue)
                            {
                                yield return ContinuousController.instance.StartCoroutine(
                                    playedPermanent.AddDigivolutionCardsBottom(new List<CardSource> { card }, activateClass));

                                yield return ContinuousController.instance.StartCoroutine(new IReduceSecurity(
                                    player: card.Owner,
                                    refSkillInfos: ref ContinuousController.instance.nullSkillInfos,
                                    activateClass).ReduceSecurity());
                            }
                        }
                    }
                }
            }
            #endregion

            #region All turns
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By deleting this, they don't leave", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] When any of your other [Shaman] or [Iliad] trait Digimon or Tamers would leave the battle area other than by your effects, by deleting this Digimon, they don't leave.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, PermanentCondition)
                        && !CardEffectCommons.IsByEffect(hashtable, cardEffect => CardEffectCommons.IsOwnerEffect(cardEffect, card));
                        
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return permanent != card.PermanentOfThisCard()
                        && CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                        && (permanent.IsDigimon || permanent.IsTamer)
                        && (permanent.TopCard.HasShamanTraits || permanent.TopCard.HasIliadTraits);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() }, activateClass: activateClass, successProcess: permanents => SuccessProcess(), failureProcess: null));

                    IEnumerator SuccessProcess()
                    {
                        List<Permanent> protectedPermanents = CardEffectCommons.GetPermanentsFromHashtable(hashtable)
                                    .Filter(PermanentCondition);

                        foreach (Permanent permanent in protectedPermanents)
                        {
                            permanent.willBeRemoveField = false;
                            permanent.HideDeleteEffect();
                            permanent.HideHandBounceEffect();
                            permanent.HideDeckBounceEffect();
                            permanent.HideWillRemoveFieldEffect();
                        }

                        yield return null;
                    }
                }
            }
            #endregion

            #region On Deletion
            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                cardEffects.Add(CardEffectFactory.OnDeletionClass(
                    card,
                    "Place this card face up as the bottom security card",
                    ActivateCoroutine,
                    "[On Deletion] You may place this card face up as the bottom security card.",
                    optional: true,
                    additionalActivateCondition: AdditionalActivateCondition
                ));

                bool AdditionalActivateCondition(Hashtable hashtable, ActivateClass activateClass) => card.Owner.CanAddSecurity(activateClass);

                IEnumerator ActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
                {
                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(card, toTop: false, faceUp: true));
                }
            }
            #endregion

            #region Opponent's Turn - ESS
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("You may change the attack target to 1 of your suspended Digimon.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Redirect_BT25_039");
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[Opponent's Turn] [Once Per Turn] When one of your opponent's Digimon attacks, you may change the attack target to 1 of your suspended Digimon.";

                bool IsOpponentDigimon(Permanent permanent) => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.IsOpponentTurn(card)
                        && CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, IsOpponentDigimon);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card)
                        && CardEffectCommons.HasMatchConditionOwnersPermanent(card, IsSuspendedDigimon);
                }

                bool IsSuspendedDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card)
                        && permanent.IsSuspended;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = null;

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: IsSuspendedDigimon,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to change the attack target to.", "The opponent is selecting 1 Digimon to change the attack target to.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanent = permanent;
                        yield return null;
                    }

                    if (selectedPermanent != null)
                    {
                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.attackProcess.SwitchDefender(
                                activateClass,
                                false,
                                selectedPermanent));
                    }
                    else activateClass.RemoveUse();
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
