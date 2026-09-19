using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT16
{
    public class BT16_074 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Digivolve Condition

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasText("Pulsemon") && targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 4;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            #endregion

            #region When Digivolving

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 2 and trash 1 card and/or play a Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] If you have 3 or more security cards, <Draw 2>. Then trash 1 card in your hand. If you have 3 or fewer security cards, you may play 1 Digimon with 6000 DP or less and [Pulsemon] in its text from your trash without paying the cost. At the next time your opponent's turn ends, delete that Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanSelectCardCondition(CardSource card)
                {
                    if (card.HasText("Pulsemon") && card.IsDigimon && card.CardDP <= 6000)
                    {
                        return true;
                    }
                    return false;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                    {
                        return true;
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.SecurityCards.Count <= 3)
                        {
                            if (CardEffectCommons.HasMatchConditionOpponentsPermanent(card, PermanentCondition))
                            {
                                return true;
                            }
                        }

                        if (card.Owner.SecurityCards.Count >= 3)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.SecurityCards.Count >= 3)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 2, activateClass).Draw());

                        if (card.Owner.HandCards.Count >= 1)
                        {
                            int discardCount = Math.Min(1, card.Owner.HandCards.Count);

                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: (cardSource) => true,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: discardCount,
                                canNoSelect: false,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: null,
                                mode: SelectHandEffect.Mode.Discard,
                                cardEffect: activateClass);

                            yield return StartCoroutine(selectHandEffect.Activate());
                        }
                    }

                    if (card.Owner.SecurityCards.Count <= 3)
                    {
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Yes", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"No", value : false, spriteIndex: 1),
                        };

                        string selectPlayerMessage = "Will you play a Digimon with [Pulsemon] in it's text?";
                        string notSelectPlayerMessage = "The opponent is choosing whether or not to play a Digimon.";

                        GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        bool willPlay = GManager.instance.userSelectionManager.SelectedBoolValue;

                        if (willPlay)
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(PermanentCondition))
                            {
                                int maxCount = 1;

                                List<CardSource> selectedCards = new List<CardSource>();

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

                                IEnumerator SelectCardCoroutine(CardSource cardSource)
                                {
                                    selectedCards.Add(cardSource);

                                    yield return null;
                                }

                                SelectCardEffect.Root root = SelectCardEffect.Root.Trash;

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                    cardSources: selectedCards,
                                    activateClass: activateClass,
                                    payCost: false,
                                    isTapped: false,
                                    root: root,
                                    activateETB: true));

                                foreach (CardSource selectedCard in selectedCards)
                                {
                                    if (CardEffectCommons.IsExistOnBattleArea(selectedCard))
                                    {
                                        Permanent selectedPermanent = selectedCard.PermanentOfThisCard();

                                        if (selectedPermanent != null)
                                        {
                                            ActivateClass activateClass1 = new ActivateClass();
                                            activateClass1.SetUpICardEffect("Delete this Digimon", CanUseCondition1, selectedCard);
                                            activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, "");
                                            activateClass1.SetEffectSourcePermanent(selectedPermanent);
                                            card.Owner.UntilOpponentTurnEndEffects.Add(GetCardEffect);

                                            if (!selectedCard.CanNotBeAffected(activateClass))
                                            {
                                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateDebuffEffect(selectedPermanent));
                                            }

                                            bool CanUseCondition1(Hashtable hashtable1)
                                            {
                                                if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                                                {
                                                    if (GManager.instance.turnStateMachine.gameContext.TurnPlayer != selectedPermanent.TopCard.Owner)
                                                    {
                                                        return true;
                                                    }
                                                }

                                                return false;
                                            }

                                            bool CanActivateCondition1(Hashtable hashtable)
                                            {
                                                if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                                                {
                                                    if (selectedPermanent.CanBeDestroyedBySkill(activateClass1))
                                                    {
                                                        if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass1))
                                                        {
                                                            return selectedPermanent.IsDigimon;
                                                        }
                                                    }
                                                }

                                                return false;
                                            }

                                            IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                                            {
                                                yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(new List<Permanent>() { selectedPermanent }, CardEffectCommons.CardEffectHashtable(activateClass1)).Destroy());
                                            }

                                            ICardEffect GetCardEffect(EffectTiming _timing)
                                            {
                                                if (_timing == EffectTiming.OnEndTurn)
                                                {
                                                    return activateClass1;
                                                }

                                                return null;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            yield return null;
                        }
                    }
                }
            }

            #endregion

            #region Inherit

            if (timing == EffectTiming.OnEndAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Unsuspend this Digimon.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Unsuspend_BT16_074");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Attack][Once per turn] If this Digimon has [Pulsemon] in its text, by trashing the top card of your security stack, unsuspend this Digimon.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnEndAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().TopCard.HasText("Pulsemon"))
                        {
                            if (card.Owner.SecurityCards.Count >= 1)
                            {
                                if (CardEffectCommons.CanUnsuspend(card.PermanentOfThisCard()))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (permanent == card.PermanentOfThisCard())
                    {
                        if (card.PermanentOfThisCard().TopCard.HasText("Pulsemon"))
                        {
                            if (CardEffectCommons.CanUnsuspend(card.PermanentOfThisCard()))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                    player: card.Owner,
                    destroySecurityCount: 1,
                    cardEffect: activateClass,
                    fromTop: true).DestroySecurity());

                    if (CardEffectCommons.HasMatchConditionPermanent(PermanentCondition))
                    {
                        Permanent selectedPermanent = card.PermanentOfThisCard();

                        yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { selectedPermanent }, activateClass).Unsuspend());
                    }
                }
            }

            #endregion

            return cardEffects;
        }
    }
}