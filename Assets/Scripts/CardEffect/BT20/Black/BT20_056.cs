using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT20
{
    public class BT20_056 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Barrier

            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            {
                cardEffects.Add(CardEffectFactory.BarrierSelfEffect(
                    isInheritedEffect: false,
                    card: card,
                    condition: null));
            }

            #endregion

            #region On Play

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("<Recovery +1 (Deck)>", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[On Play] <Recovery +1 (Deck)>. Then, if during an attack, 1 of your Digimon in the breeding area may digivolve into a level 6 or lower [Chronicle] trait Digimon card in the hand or trash without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanSelectDigivolveCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon && cardSource.HasLevel && cardSource.Level <= 6 &&
                           cardSource.EqualsTraits("Chronicle") &&
                           cardSource.CanPlayCardTargetFrame(
                               card.Owner.GetBreedingAreaPermanents()[0].PermanentFrame,
                               false, activateClass, isBreedingArea: true);
                }

                bool IsAttackingCondition()
                {
                    return GManager.instance.attackProcess.IsAttacking &&
                           card.Owner.GetBreedingAreaPermanents().Count > 0 &&
                           (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectDigivolveCardCondition) ||
                            CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectDigivolveCardCondition));
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           (card.Owner.LibraryCards.Count >= 1 || IsAttackingCondition());
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.LibraryCards.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IRecovery(card.Owner, 1, activateClass).Recovery());
                    }

                    if (IsAttackingCondition())
                    {
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new(message: "Digivolve in the breeding area", value: true, spriteIndex: 0),
                            new(message: "Do not digivolve", value: false, spriteIndex: 1),
                        };

                        string selectPlayerMessage = "Digivolve in the breeding area?";
                        string notSelectPlayerMessage = "The opponent is choosing whether to digivolve in the breeding area or not.";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements,
                            selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage,
                            notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager
                            .WaitForEndSelect());

                        if (GManager.instance.userSelectionManager.SelectedBoolValue)
                        {
                            bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectDigivolveCardCondition);
                            bool canSelectTrash =
                                CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectDigivolveCardCondition);

                            if (canSelectHand || canSelectTrash)
                            {
                                if (canSelectHand && canSelectTrash)
                                {
                                    selectionElements = new List<SelectionElement<bool>>()
                                    {
                                        new(message: "From hand", value: true, spriteIndex: 0),
                                        new(message: "From trash", value: false, spriteIndex: 1),
                                    };

                                    selectPlayerMessage = "From which area do you play a card?";
                                    notSelectPlayerMessage = "The opponent is choosing from which area to play a card.";

                                    GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements,
                                        selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage,
                                        notSelectPlayerMessage: notSelectPlayerMessage);
                                }
                                else
                                {
                                    GManager.instance.userSelectionManager.SetBool(canSelectHand);
                                }

                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager
                                    .WaitForEndSelect());

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                    targetPermanent: card.Owner.GetBreedingAreaPermanents()[0],
                                    cardCondition: CanSelectDigivolveCardCondition,
                                    payCost: false,
                                    reduceCostTuple: null,
                                    fixedCostTuple: null,
                                    ignoreDigivolutionRequirementFixedCost: -1,
                                    isHand: GManager.instance.userSelectionManager.SelectedBoolValue,
                                    activateClass: activateClass,
                                    successProcess: null));
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
                activateClass.SetUpICardEffect("<Recovery +1 (Deck)>", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[When Digivolving] <Recovery +1 (Deck)>. Then, if during an attack, 1 of your Digimon in the breeding area may digivolve into a level 6 or lower [Chronicle] trait Digimon card in the hand or trash without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanSelectDigivolveCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon && cardSource.HasLevel && cardSource.Level <= 6 &&
                           cardSource.EqualsTraits("Chronicle") &&
                           cardSource.CanPlayCardTargetFrame(
                               card.Owner.GetBreedingAreaPermanents()[0].PermanentFrame,
                               false, activateClass, isBreedingArea: true);
                }

                bool IsAttackingCondition()
                {
                    return GManager.instance.attackProcess.IsAttacking &&
                           card.Owner.GetBreedingAreaPermanents().Count > 0 &&
                           (CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectDigivolveCardCondition) ||
                            CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectDigivolveCardCondition));
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           (card.Owner.LibraryCards.Count >= 1 || IsAttackingCondition());
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.LibraryCards.Count >= 1)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new IRecovery(card.Owner, 1, activateClass).Recovery());
                    }

                    if (IsAttackingCondition())
                    {
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new(message: "Digivolve in the breeding area", value: true, spriteIndex: 0),
                            new(message: "Do not digivolve", value: false, spriteIndex: 1),
                        };

                        string selectPlayerMessage = "Digivolve in the breeding area?";
                        string notSelectPlayerMessage = "The opponent is choosing whether to digivolve in the breeding area or not.";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements,
                            selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage,
                            notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager
                            .WaitForEndSelect());

                        if (GManager.instance.userSelectionManager.SelectedBoolValue)
                        {
                            bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectDigivolveCardCondition);
                            bool canSelectTrash =
                                CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectDigivolveCardCondition);

                            if (canSelectHand || canSelectTrash)
                            {
                                if (canSelectHand && canSelectTrash)
                                {
                                    selectionElements = new List<SelectionElement<bool>>()
                                    {
                                        new(message: "From hand", value: true, spriteIndex: 0),
                                        new(message: "From trash", value: false, spriteIndex: 1),
                                    };

                                    selectPlayerMessage = "From which area do you play a card?";
                                    notSelectPlayerMessage = "The opponent is choosing from which area to play a card.";

                                    GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements,
                                        selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage,
                                        notSelectPlayerMessage: notSelectPlayerMessage);
                                }
                                else
                                {
                                    GManager.instance.userSelectionManager.SetBool(canSelectHand);
                                }

                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager
                                    .WaitForEndSelect());

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                                    targetPermanent: card.Owner.GetBreedingAreaPermanents()[0],
                                    cardCondition: CanSelectDigivolveCardCondition,
                                    payCost: false,
                                    reduceCostTuple: null,
                                    fixedCostTuple: null,
                                    ignoreDigivolutionRequirementFixedCost: -1,
                                    isHand: GManager.instance.userSelectionManager.SelectedBoolValue,
                                    activateClass: activateClass,
                                    successProcess: null));
                            }
                        }
                    }
                }
            }

            #endregion

            #region All Turns

            if (timing == EffectTiming.OnLoseSecurity)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("1 of your opponent's Digimon gets -8000 DP", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetHashString("RemovedSec_BT20_056");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[All Turns] (Once Per Turn) When security stacks are removed from, 1 of your opponent's Digimon gets -8000 DP for the turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenLoseSecurity(hashtable, _ => true);
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    Permanent selectedPermanent = null;

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

                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will get DP -8000.",
                        "The opponent is selecting 1 Digimon that will get DP -8000.");

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
                            changeValue: -8000,
                            effectDuration: EffectDuration.UntilEachTurnEnd,
                            activateClass: activateClass));
                    }
                }
            }

            #endregion

            #region All Turns - ESS

            if (timing == EffectTiming.WhenRemoveField)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash your top security card to prevent this Digimon from leaving the battle area",
                    CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDescription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("TrashSecurityToStay_BT20_056");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[All Turns] (Once Per Turn) When this Digimon would leave the battle are other than by your effects, if this Digimon is [Alphamon: Ouryuken], by trashing your top security card, it doesn't leave.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           CardEffectCommons.CanTriggerWhenRemoveField(hashtable, card) &&
                           !CardEffectCommons.IsByEffect(hashtable, cardEffect => CardEffectCommons.IsOwnerEffect(cardEffect, card));
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card) &&
                           card.PermanentOfThisCard().TopCard.EqualsCardName("Alphamon: Ouryuken") &&
                           card.Owner.SecurityCards.Count > 0;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                        player: card.Owner,
                        destroySecurityCount: 1,
                        cardEffect: activateClass,
                        fromTop: true).DestroySecurity());
                    
                    Permanent thisCardPermanent = card.PermanentOfThisCard();

                    thisCardPermanent.willBeRemoveField = false;

                    thisCardPermanent.HideDeleteEffect();
                    thisCardPermanent.HideHandBounceEffect();
                    thisCardPermanent.HideDeckBounceEffect();
                    thisCardPermanent.HideWillRemoveFieldEffect();

                    yield return null;
                }
            }

            #endregion

            return cardEffects;
        }
    }
}