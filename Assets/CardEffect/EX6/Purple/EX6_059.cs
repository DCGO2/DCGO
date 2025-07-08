using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

namespace DCGO.CardEffects.EX6
{
    public class EX6_059 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Scapegoat
            if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
            { 
                cardEffects.Add(CardEffectFactory.ScapegoatSelfEffect(isInheritedEffect: false, card: card, condition: null, effectName: "<Scapegoat>", effectDiscription: null));
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 card in your opponent's hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Trash 1 card in your opponent's hand without looking.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable,card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if(CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if(card.Owner.Enemy.HandCards.Count >= 1)
                        {
                            return true;
                        }
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.isYou)
                    {
                        foreach (CardSource cardSource in card.Owner.Enemy.HandCards)
                        {
                            cardSource.SetReverse();
                        }
                    }

                    card.Owner.Enemy.HandCards = RandomUtility.ShuffledDeckCards(card.Owner.Enemy.HandCards);

                    List<CardSource> selectedCards = new List<CardSource>();

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: (cardSource) => true,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => false,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 card in your opponent's hand to trash.",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: false,
                        mode: SelectCardEffect.Mode.Discard,
                        root: SelectCardEffect.Root.Custom,
                        customRootCardList: card.Owner.Enemy.HandCards,
                        canLookReverseCard: false,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetNotShowCard();
                    selectCardEffect.SetUseFaceDown();

                    if (card.Owner.isYou)
                    {
                        selectCardEffect.SetNotAddLog();
                    }

                    selectCardEffect.SetUpCustomMessage(
                    "Select 1 card to trash.",
                    "The opponent is selecting 1 card to trash.");

                    yield return StartCoroutine(selectCardEffect.Activate());
                }
            }
            #endregion

            #region When Digivolving
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Trash 1 card in your opponent's hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Trash 1 card in your opponent's hand without looking.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (card.Owner.Enemy.HandCards.Count >= 1)
                        {
                            return true;
                        }
                    }
                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (card.Owner.isYou)
                    {
                        foreach (CardSource cardSource in card.Owner.Enemy.HandCards)
                        {
                            cardSource.SetReverse();
                        }
                    }

                    card.Owner.Enemy.HandCards = RandomUtility.ShuffledDeckCards(card.Owner.Enemy.HandCards);

                    List<CardSource> selectedCards = new List<CardSource>();

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: (cardSource) => true,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => false,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: null,
                        message: "Select 1 card in your opponent's hand to trash.",
                        maxCount: 1,
                        canEndNotMax: false,
                        isShowOpponent: false,
                        mode: SelectCardEffect.Mode.Discard,
                        root: SelectCardEffect.Root.Custom,
                        customRootCardList: card.Owner.Enemy.HandCards,
                        canLookReverseCard: false,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                    selectCardEffect.SetNotShowCard();
                    selectCardEffect.SetUseFaceDown();

                    if (card.Owner.isYou)
                    {
                        selectCardEffect.SetNotAddLog();
                    }

                    selectCardEffect.SetUpCustomMessage(
                    "Select 1 card to trash.",
                    "The opponent is selecting 1 card to trash.");

                    yield return StartCoroutine(selectCardEffect.Activate());
                }
            }
            #endregion

            #region All Turns
            if (timing == EffectTiming.OnDiscardHand)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 purple card with a play cost of 10 or less from your trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
                activateClass.SetHashString("PlayFromTrash_059");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] [Once Per Turn] When a card is trashed from your opponent's hand, you may play 1 purple card with a play cost of 10 or less from your trash without paying the cost. For each card in your opponent's hand, reduce this effect's play cost maximum by 1.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
                    {
                        if (CardEffectCommons.CanTriggerOnTrashHand(hashtable, cardEffect => true, cardSource => cardSource.Owner == card.Owner.Enemy))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectCardInTrash(CardSource cardSource)
                {
                    int maxCost = 10 - card.Owner.Enemy.HandCards.Count();

                    if(cardSource.IsDigimon || cardSource.IsTamer)
                    {
                        if (cardSource.CardColors.Contains(CardColor.Purple))
                        {
                            if (cardSource.GetCostItself <= maxCost)
                            {
                                return true;
                            }
                        }
                    }                    
           
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaDigimon(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardInTrash))
                    {
                        int maxCount = Math.Min(1, card.Owner.TrashCards.Count(CanSelectCardInTrash));

                        List<CardSource> selectedCards = new List<CardSource>();

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectCardInTrash,
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

                        yield return StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                cardSources: selectedCards,
                                activateClass: activateClass,
                                payCost: false,
                                isTapped: false,
                                root: SelectCardEffect.Root.Trash,
                                activateETB: true));
                    }
                }
            }
            #endregion

            return cardEffects;
        }
    }
}