using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.BT17
{
    public class BT17_059 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("By placing [Doomsday Clock] in sources, Play 2 Diaboromon tokens", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] By placing 1 [Doomsday Clock] from your hand or trash as this Digimon's bottom digivolution card, you may play 2 [Diaboromon] (Digimon | 14 Cost | Level 6 | White | Mega | Unknown | Unidentified | DP3000) Tokens without paying its cost.";
                }

                bool HasDoomsdayClock(CardSource source)
                {
                    return source.EqualsCardName("Doomsday Clock");
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                        return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersHand(card, HasDoomsdayClock) || CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, HasDoomsdayClock))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    bool validCardInTrash = CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, HasDoomsdayClock);
                    bool validCardInHand = CardEffectCommons.HasMatchConditionOwnersHand(card, HasDoomsdayClock);
                    bool useTrash = false;
                    bool cardAdded = false;

                    if (validCardInTrash || validCardInHand)
                    {
                        #region Selecting Hand or Trash
                        if (validCardInTrash && validCardInHand)
                        {
                            List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                                {
                                    new SelectionElement<int>(message: $"Trash", value : 0, spriteIndex: 0),
                                    new SelectionElement<int>(message: $"Hand", value : 1, spriteIndex: 0),
                                    new SelectionElement<int>(message: $"No Selection", value : 2, spriteIndex: 1),
                                };

                            string selectPlayerMessage = "Will you use a card from hand or trash?";
                            string notSelectPlayerMessage = "The opponent is choosing whether use a card from hand or trash.";

                            GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                        }
                        else if (validCardInTrash && !validCardInHand)
                        {
                            GManager.instance.userSelectionManager.SetInt(0);
                        }
                        else if (!validCardInTrash && validCardInHand)
                        {
                            GManager.instance.userSelectionManager.SetInt(1);
                        }
                        else
                        {
                            GManager.instance.userSelectionManager.SetInt(2);
                        }

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        useTrash = (GManager.instance.userSelectionManager.SelectedIntValue == 0);
                        #endregion

                        if (GManager.instance.userSelectionManager.SelectedIntValue != 2)
                        {
                            #region Card Selection
                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                if(cardSource != null)
                                {
                                    
                                    yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(
                                                new List<CardSource>() { cardSource },
                                                activateClass));
                                    
                                    cardAdded = true;
                                }
                            }

                            if (!useTrash)
                            {
                                SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                                selectHandEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: HasDoomsdayClock,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: 1,
                                    canNoSelect: true,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    mode: SelectHandEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectHandEffect.SetUpCustomMessage(
                        "Select 1 card to place at the bottom of digivolution cards.",
                        "The opponent is selecting 1 card to place at the bottom of digivolution cards.");
                                selectHandEffect.SetUpCustomMessage_ShowCard("Place bottom digivolution card");

                                yield return StartCoroutine(selectHandEffect.Activate());
                            }

                            else
                            {
                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                    canTargetCondition: HasDoomsdayClock,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    canNoSelect: () => true,
                                    selectCardCoroutine: SelectCardCoroutine,
                                    afterSelectCardCoroutine: null,
                                    message: "Select 1 card to play.",
                                    maxCount: 1,
                                    canEndNotMax: false,
                                    isShowOpponent: true,
                                    mode: SelectCardEffect.Mode.Custom,
                                    root: SelectCardEffect.Root.Trash,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                                selectCardEffect.SetUpCustomMessage(
                        "Select 1 card to place at the bottom of digivolution cards.",
                        "The opponent is selecting 1 card to place at the bottom of digivolution cards.");
                                selectCardEffect.SetUpCustomMessage_ShowCard("Place bottom digivolution card");

                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                            }
                            #endregion
                        }
                    }

                    if (cardAdded)
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayDiaboromonToken(activateClass, 2));
                    }
                }
            }
            #endregion

            #region Opponets Turn
            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Switch attack target to a [Diaboromon]", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("AttackSwitch_BT17_059");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Opponent's Turn] [Once Per Turn] When one of your opponent's Digimon attacks, you may switch the attack target to 1 of your Digimon with [Diaboromon] in its name.";
                }

                bool AttackingPermanent(Permanent permanent)
                {
                    return CardEffectCommons.IsOpponentPermanent(permanent, card);
                }

                bool DefendingPermanent(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                        return permanent.TopCard.ContainsCardName("Diaboromon");

                    return false;
                }


                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if(CardEffectCommons.CanTriggerOnPermanentAttack(hashtable, AttackingPermanent))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (GManager.instance.attackProcess.AttackingPermanent != null)
                        {
                            if (GManager.instance.attackProcess.AttackingPermanent.CanSwitchAttackTarget)
                            {
                                if (CardEffectCommons.HasMatchConditionPermanent(DefendingPermanent))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(DefendingPermanent))
                    {
                        int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(DefendingPermanent));

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: DefendingPermanent,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that opponent's Digimon attacks to.", "The opponent is selecting 1 Digimon that opponent's Digimon attacks to.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.attackProcess.SwitchDefender(
                                activateClass,
                                false,
                                permanent));
                        }
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
