using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT13
{
    public class BT13_089 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete this Digimon to play 1 [Ravemon] from trash at the end of opponent's turn", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return " [End of Your Turn] By deleting this Digimon that has a digivolution card with [Bird] or [Avian] in one of its traits, at the end of your opponent's turn, you may play 1 [Ravemon] from your trash without paying the cost.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                        if (cardSource.IsDigimon)
                        {
                            if (cardSource.Owner == card.Owner)
                            {
                                if (cardSource.CardNames.Contains("Ravemon"))
                                {
                                    if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().DigivolutionCards.Count(cardSource => cardSource.HasBirdTraits) >= 1)
                        {
                            if (card.PermanentOfThisCard().CanBeDestroyedBySkill(activateClass))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().DigivolutionCards.Count(cardSource => cardSource.HasBirdTraits) >= 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: new List<Permanent>() { card.PermanentOfThisCard() }, activateClass: activateClass, successProcess: permanents => SuccessProcess(), failureProcess: null));

                            IEnumerator SuccessProcess()
                            {
                                ActivateClass activateClass1 = new ActivateClass();
                                activateClass1.SetUpICardEffect("Play 1 [Ravemon] from trash", CanUseCondition1, card);
                                activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, true, "");
                                CardEffectCommons.AddEffectToPlayer(effectDuration: EffectDuration.UntilOpponentTurnEnd, card: card, cardEffect: activateClass1, timing: EffectTiming.OnEndTurn);

                                bool CanUseCondition1(Hashtable hashtable)
                                {
                                    return CardEffectCommons.IsOpponentTurn(card);
                                }

                                bool CanActivateCondition1(Hashtable hashtable)
                                {
                                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                                    {
                                        return true;
                                    }

                                    return false;
                                }

                                IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                                {
                                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, (cardSource) => CanSelectCardCondition(cardSource)))
                                    {
                                        int maxCount = Math.Min(1, card.Owner.TrashCards.Count((cardSource) => CanSelectCardCondition(cardSource)));

                                        List<CardSource> selectedCards = new List<CardSource>();

                                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                        selectCardEffect.SetUp(
                                                    canTargetCondition: CanSelectCardCondition,
                                                    canTargetCondition_ByPreSelecetedList: null,
                                                    canEndSelectCondition: null,
                                                    canNoSelect: () => true,
                                                    selectCardCoroutine: SelectCardCoroutine,
                                                    afterSelectCardCoroutine: null,
                                                    message: "Select 1 [Ravemon] to play.",
                                                    maxCount: maxCount,
                                                    canEndNotMax: false,
                                                    isShowOpponent: true,
                                                    mode: SelectCardEffect.Mode.Custom,
                                                    root: SelectCardEffect.Root.Trash,
                                                    customRootCardList: null,
                                                    canLookReverseCard: true,
                                                    selectPlayer: card.Owner,
                                                    cardEffect: activateClass1);

                                        selectCardEffect.SetUpCustomMessage("Select 1 [Ravemon] to play.", "The opponent is selecting 1 [Ravemon] to play.");
                                        selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                                        yield return StartCoroutine(selectCardEffect.Activate());

                                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                                        {
                                            selectedCards.Add(cardSource);

                                            yield return null;
                                        }

                                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass, payCost: false, isTapped: false, root: SelectCardEffect.Root.Trash, activateETB: true));
                                    }
                                }

                                yield return null;
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 [Falcomon] or [Keenan Crier] from hand or trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Deletion] You may play 1 [Falcomon] or [Keenan Crier] from your hand or trash without paying the cost.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                        if (cardSource.Owner == card.Owner)
                        {
                            if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                            {
                                if (cardSource.CardNames.Contains("Falcomon"))
                                {
                                    return true;
                                }

                                if (cardSource.CardNames.Contains("Keenan Crier"))
                                {
                                    return true;
                                }

                                if (cardSource.CardNames.Contains("KeenanCrier"))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnTrash(card))
                    {
                        if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                        {
                            return true;
                        }

                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool canSelectHand = card.Owner.HandCards.Count(CanSelectCardCondition) >= 1;
                    bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition);

                    if (canSelectHand || canSelectTrash)
                    {
                        if (canSelectHand && canSelectTrash)
                        {
                            List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"From hand", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"From trash", value : false, spriteIndex: 1),
                        };

                            string selectPlayerMessage = "From which area do you play a card?";
                            string notSelectPlayerMessage = "The opponent is choosing from which area to play a card.";

                            GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                        }
                        else
                        {
                            GManager.instance.userSelectionManager.SetBool(canSelectHand);
                        }

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                        List<CardSource> selectedCards = new List<CardSource>();

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        if (fromHand)
                        {
                            int maxCount = 1;

                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: true,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                mode: SelectHandEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectHandEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                            selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                            yield return StartCoroutine(selectHandEffect.Activate());
                        }
                        else
                        {
                            int maxCount = 1;

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => true,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 card to play.",
                                maxCount: maxCount,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                            selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                        }

                        SelectCardEffect.Root root = SelectCardEffect.Root.Hand;

                        if (!fromHand)
                        {
                            root = SelectCardEffect.Root.Trash;
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                            cardSources: selectedCards,
                            activateClass: activateClass,
                            payCost: false,
                            isTapped: false,
                            root: root,
                            activateETB: true));
                    }
                }
            }

            return cardEffects;
        }
    }
}