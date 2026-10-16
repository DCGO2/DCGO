using System.Collections;
using System.Collections.Generic;
using System.Linq;

//Craniamon (X Antibody)
namespace DCGO.CardEffects.BT27
{
    public class BT27_070 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digimon Side
            #region Alt Digivolution
            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return permanent.TopCard.EqualsCardName("Craniamon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(PermanentCondition, 2, false, card, null));
            }
            #endregion

            #region Reboot
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By suspending, they don't leave", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] When any of your Digimon would leave the battle area, by suspending this Digimon, they don't leave.";
                }

                bool RemovedDigimon(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenPermanentRemoveField(hashtable, RemovedDigimon);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.CanActivateSuspendCostEffect(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent thisPermament = card.PermanentOfThisCard();

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SuspendPeremanentAndProcessAccordingToResult(
                        targetPermanents: new List<Permanent>() { thisPermament },
                        activateClass: activateClass,
                        successProcess: SuccessProcess,
                        failureProcess: null));

                    IEnumerator SuccessProcess(List<Permanent> _permanents)
                    {
                        List<Permanent> protectedPermanents = CardEffectCommons.GetPermanentsFromHashtable(hashtable)
                            .Filter(RemovedDigimon);

                        foreach (Permanent permanent in protectedPermanents)
                        {
                            permanent.willBeRemoveField = false;
                            permanent.HideHandBounceEffect();
                            permanent.HideDeckBounceEffect();
                            permanent.HideDeleteEffect();
                            permanent.HideWillRemoveFieldEffect();
                        }

                        yield return null;
                    }
                }
            }
            #endregion

            #region All Turns - OPT
            if (timing == EffectTiming.OnTappedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete opponent's Digimon, Then activate [Main]", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetHashString("BT27_070_Delete");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[All Turns] [Once Per Turn] When this Digimon suspends, you may delete all of your opponent's lowest DP Digimon. Then, by trashing 1 [Craniamon] or [X Antibody] from this Digimon's digivolution cards, activate 1 [Main] effect on this card's Option side.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerWhenSelfPermanentSuspends(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && (CardEffectCommons.HasMatchConditionPermanent(PermanentCondition)
                            || card.PermanentOfThisCard().DigivolutionCards.Any(CanSelectSource));
                }

                bool PermanentCondition(Permanent permanent) => CardEffectCommons.IsMinDP(permanent, card.Owner.Enemy);

                bool CanSelectSource(CardSource cardSource)
                {
                    return (cardSource.EqualsCardName("Craniamon") || cardSource.HasXAntiBodyName) &&
                            !cardSource.CanNotTrashFromDigivolutionCards(activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(PermanentCondition))
                    {
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Delete", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"Do not delete", value : false, spriteIndex: 1),
                        };

                        string selectPlayerMessage = "Will you delete opponent's lowest DP?";
                        string notSelectPlayerMessage = "The opponent is choosing whether to delete your lowest DP.";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        if (GManager.instance.userSelectionManager.SelectedBoolValue)
                        {
                            List<Permanent> destroyTargetPermanents = card.Owner.Enemy.GetBattleAreaDigimons().Filter(PermanentCondition);
                            yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(destroyTargetPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).Destroy());
                        }
                    }

                    if (card.PermanentOfThisCard().DigivolutionCards.Any(CanSelectSource))
                    {
                        Permanent selectedPermanment = card.PermanentOfThisCard();
                        CardSource selectedCard = null;
                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectSource,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 digivolution card to discard.",
                            maxCount: 1,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Custom,
                            customRootCardList: selectedPermanment.DigivolutionCards,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: null);

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;
                            yield return null;
                        }

                        selectCardEffect.SetUpCustomMessage("Select 1 digivolution card to discard.", "The opponent is selecting 1 digivolution card to discard.");
                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        if (selectedCard != null) yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.TrashDigivolutionCardsAndProcessAccordingToResult(
                            targetPermanent: selectedPermanment,
                            targetDigivolutionCards: new List<CardSource>() { selectedCard },
                            activateClass: activateClass,
                            successProcess: SuccessProcess,
                            failureProcess: null));

                        IEnumerator SuccessProcess(List<CardSource> cardSources)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ActivateMainOfOptionSide(card, activateClass));
                        }
                    }
                }
            }
            #endregion
            #endregion

            #region Option Side
            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("As an option effect, 1 of your Digimon is immune to digimon and gets + 5k DP, then unsuspend", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsOptionEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription() => $"[Main] Until your opponent's turn ends, 1 of your Digimon with <Blocker> isn't affected by their Digimon effects and gets +5000 DP. Then, 1 of your Digimon may unsuspend.";

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);

                bool CanSelectImmunityPermanentCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card) &&
                       permanent.HasBlocker;

                bool CanSelectUnsuspendCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectImmunityPermanentCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectImmunityPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get Digimon effect immunity and DP +5000.", "The opponent is selecting 1 Digimon that will get Digimon effect immunity and DP +5000.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            #region Give Digimon Effect Immunity
                            permanent.UntilOpponentTurnEndEffects.Add((_timing) => PermanentEffectFactory.DigimonEffectImmunity(permanent));
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(permanent));
                            #endregion

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: permanent, changeValue: 5000, effectDuration: EffectDuration.UntilOpponentTurnEnd, activateClass: activateClass));
                        }
                    }

                    if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, CanSelectUnsuspendCondition))
                    {
                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectUnsuspendCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: null,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.UnTap,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will unsuspend.", "The opponent is selecting 1 Digimon that will unsuspend.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
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
