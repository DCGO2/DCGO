using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT13
{
    public class BT13_094 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Memory +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Start of Your Main Phase] If you have a Digimon with [Avian] or [Bird] in one of its traits, gain 1 memory.";
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
                    if (isExistOnField(card))
                    {
                        if (card.Owner.GetBattleAreaPermanents().Contains(card.PermanentOfThisCard()))
                        {
                            if (CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.IsDigimon && permanent.TopCard.HasBirdTraits))
                            {
                                if (card.Owner.CanAddMemory(activateClass))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (isExistOnField(card))
                    {
                        if (card.Owner.CanAddMemory(activateClass))
                        {
                            yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Your 1 Digimon gets effects", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] 1 of your Digimon gains \"[On Deletion] You may play 1[Biyomon] from your hand or trash without paying the cost\" until the end of your opponent's turn.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (isExistOnField(card))
                    {
                        if (card.Owner.GetBattleAreaPermanents().Contains(card.PermanentOfThisCard()))
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (isExistOnField(card))
                    {
                        if (card.Owner.GetBattleAreaPermanents().Contains(card.PermanentOfThisCard()))
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                            {
                                int maxCount = 1;

                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectPermanentCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: false,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: SelectPermanentCoroutine,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage("Selcect 1 Digimon that will get effects.", "The opponent is selecting 1 Digimon that will get effects.");

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    Permanent selectedPermanent = permanent;

                                    if (selectedPermanent != null)
                                    {
                                        CardSource _topCard = selectedPermanent.TopCard;

                                        ActivateClass activateClass1 = new ActivateClass();
                                        activateClass1.SetUpICardEffect("Play 1 [Biyomon] from trash", CanUseCondition1, selectedPermanent.TopCard);
                                        activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, true, EffectDiscription1());
                                        activateClass1.SetEffectSourcePermanent(selectedPermanent);
                                        CardEffectCommons.AddEffectToPermanent(targetPermanent: selectedPermanent, effectDuration: EffectDuration.UntilOpponentTurnEnd, card: card, cardEffect: activateClass1, timing: EffectTiming.OnDestroyedAnyone);

                                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(selectedPermanent));

                                        string EffectDiscription1()
                                        {
                                            return "[On Deletion] You may play 1[Biyomon] from your hand or trash without paying the cost";
                                        }

                                        bool CanSelectCardCondition(CardSource cardSource)
                                        {
                                            if (cardSource != null)
                                            {
                                                if (cardSource.Owner == card.Owner)
                                                {
                                                    if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass1))
                                                    {
                                                        if (cardSource.CardNames.Contains("Biyomon"))
                                                        {
                                                            return true;
                                                        }
                                                    }
                                                }
                                            }

                                            return false;
                                        }

                                        bool CanUseCondition1(Hashtable hashtable1)
                                        {
                                            if (CardEffectCommons.IsPermanentExistsOnBattleArea(selectedPermanent))
                                            {
                                                if (CardEffectCommons.CanTriggerOnPermanentDeleted(hashtable1, (permanent) => permanent == selectedPermanent))
                                                {
                                                    if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                                    {
                                                        return true;
                                                    }
                                                }
                                            }

                                            return false;
                                        }

                                        bool CanActivateCondition1(Hashtable hashtable1)
                                        {
                                            if (CardEffectCommons.IsTopCardInTrashOnDeletion(hashtable1))
                                            {
                                                if (_topCard.Owner.HandCards.Count((cardSource) => CanSelectCardCondition(cardSource)) >= 1)
                                                {
                                                    return true;
                                                }

                                                if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(_topCard, CanSelectCardCondition))
                                                {
                                                    return true;
                                                }
                                            }

                                            return false;
                                        }

                                        IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                                        {
                                            bool canSelectHand = _topCard.Owner.HandCards.Count(CanSelectCardCondition) >= 1;
                                            bool canSelectTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(_topCard, CanSelectCardCondition);

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

                                                List<CardSource> selectedCards = new List<CardSource>();

                                                IEnumerator SelectCardCoroutine(CardSource cardSource)
                                                {
                                                    selectedCards.Add(cardSource);

                                                    yield return null;
                                                }

                                                bool fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;

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

                                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass, payCost: false, isTapped: false, root: root, activateETB: true));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }

            return cardEffects;
        }
    }
}