using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// e-Pulse
namespace DCGO.CardEffects.ST23
{
    public class ST23_15 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Ignore Color Requirement
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.UseRequirements(card, PermanentCondition));

                bool PermanentCondition(Permanent permanent)
                {
                    return (permanent.IsDigimon || permanent.IsTamer)
                        && permanent.TopCard.EqualsTraits("BEATBREAK");
                }
            }
            #endregion       

            #region Main
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 [BEATBREAK] card with a play cost of 4 or less from hand or trash for free, place this card in battle area", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Main] You may play 1 [BEATBREAK] trait card with a play cost of 4 or less from your hand or trash without paying the cost. Then, place this card in the battle area.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool CanSelectPlayCondition(CardSource cardSource)
                    {
                        return (cardSource.IsDigimon
                                || cardSource.IsTamer)
                            && cardSource.HasPlayCost
                            && cardSource.EqualsTraits("BEATBREAK")
                            && cardSource.GetCostItself <= 4
                            && CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass);
                    }

                    bool canSelectHand = CardEffectCommons.HasMatchConditionOwnersHand(card, CanSelectPlayCondition);
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectPlayCondition);

                    if (canSelectHand || canSelectTrash)
                    {
                        #region Setup Location Selection
                        List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>();
                        if (canSelectHand)
                        {
                            selectionElements.Add(new(message: $"From hand", value: 1, spriteIndex: 0));
                        }
                        if (canSelectTrash)
                        {
                            selectionElements.Add(new(message: $"From trash", value: 2, spriteIndex: 0));
                        }
                        selectionElements.Add(new(message: $"Do not play", value: 3, spriteIndex: 1));

                        string selectPlayerMessage = "From which area do you select a card?";
                        string notSelectPlayerMessage = "The opponent is choosing from which area to select a card.";

                        GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                        #endregion

                        bool doPlay = GManager.instance.userSelectionManager.SelectedIntValue != 3;
                        bool fromHand = GManager.instance.userSelectionManager.SelectedIntValue == 1;

                        if (doPlay)
                        {
                            #region Hand/Trash Card Selection & Play
                            if (fromHand)
                            {
                                SelectHandEffect selectHandEffect2 = GManager.instance.GetComponent<SelectHandEffect>();

                                selectHandEffect2.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectPlayCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: 1,
                                    canNoSelect: true,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    selectCardCoroutine: null,
                                    afterSelectCardCoroutine: null,
                                    mode: SelectHandEffect.Mode.PlayForFree,
                                    cardEffect: activateClass);

                                yield return StartCoroutine(selectHandEffect2.Activate());
                            }
                            else
                            {
                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectPlayCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: null,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 card to play.",
                                    maxCount: 1,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.PlayForFree,
                                    root: SelectCardEffect.Root.Trash,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                                yield return StartCoroutine(selectCardEffect.Activate());
                            }
                            #endregion
                        }
                    }

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlaceDelayOptionCards(card: card, cardEffect: activateClass));
                }                
            }
            #endregion

            #region Security
            if (timing == EffectTiming.SecuritySkill)
            {
                CardEffectCommons.AddActivateMainOptionSecurityEffect(
                    card: card,
                    cardEffects: ref cardEffects,
                    effectName: "[Security] You may play 1 [DATA SQUAD] trait card with a play cost of 4 or less from your hand or trash without paying the cost. Then, place this card in the battle area.");
            }
            #endregion

            #region Start of Your Main Phase
            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place this card under 1 [BEATBREAK] tamer face down to <Draw 1> and gain 1 memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[Start of Your Main Phase] By placing this card from the battle area face down under any of your [BEATBREAK] trait Tamers, <Draw 1> and gain 1 memory.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.IsOwnerTurn(card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleArea(card)
                        && CardEffectCommons.HasMatchConditionPermanent(CanSelectPermamentCondition);
                }

                bool CanSelectPermamentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaTamer(permanent, card)
                        && permanent.TopCard.EqualsTraits("BEATBREAK");
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermamentCondition))
                    {
                        Permanent selectedPermanent = null;

                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermamentCondition));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermamentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: true,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Tamer that will get a digivolution card.", "The opponent is selecting 1 Tamer that will get a digivolution card.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;

                            yield return null;
                        }

                        if (selectedPermanent != null && !selectedPermanent.IsToken)
                        {
                            yield return ContinuousController.instance.StartCoroutine(card.Owner.brainStormObject.CloseBrainstrorm(card));

                            yield return ContinuousController.instance.StartCoroutine(selectedPermanent.AddDigivolutionCardsBottom(new List<CardSource>() { card }, activateClass, false, true));

                            yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());

                            yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
