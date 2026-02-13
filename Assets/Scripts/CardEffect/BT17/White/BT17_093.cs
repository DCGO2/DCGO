using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

namespace DCGO.CardEffects.BT17
{
    public class BT17_093 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region All Turns - When you Hatch

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Gain Memory", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[All Turns] When your breeding area is hatched in, by suspending this Tamer, gain 1 memory.";
                }

                bool IsDigiEggHatch(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBreedingArea(permanent, card))
                    {
                        return permanent.TopCard.IsDigiEgg;
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnField(card))
                    {
                        if (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, IsDigiEggHatch))
                            return true;
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnField(card))
                    {
                        if (CardEffectCommons.CanActivateSuspendCostEffect(card))
                            return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(new List<Permanent>() { card.PermanentOfThisCard() }, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());

                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }

            #endregion

            #region End of Your Turn

            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return this card to bottom of deck, draw 1. Then play a Tamer", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[End of Your Turn] By returning this Tamer to the bottom of the deck, <Draw 1>. Then, you may play 1 Tamer card with [Tai Kamiya] or [Kari Kamiya] in its name from your hand without paying the cost.";
                }

                bool HasProperTamer(CardSource source)
                {
                    if (source.IsTamer)
                    {
                        if (source.ContainsCardName("Tai Kamiya") || source.ContainsCardName("Kari Kamiya"))
                        {
                            return CardEffectCommons.CanPlayAsNewPermanent(source, false, activateClass);
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                        return isExistOnField(card);

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return isExistOnField(card);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                        {
                            new SelectionElement<bool>(message: $"Yes", value : true, spriteIndex: 0),
                            new SelectionElement<bool>(message: $"No", value : false, spriteIndex: 1),
                        };

                    string selectPlayerMessage = "Will you return this tamer to bottom of deck?";
                    string notSelectPlayerMessage = "The opponent is choosing whether or not to return tamer to bottom of the deck.";

                    GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);

                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                    bool willReturn = GManager.instance.userSelectionManager.SelectedBoolValue;

                    if (willReturn)
                    {
                        yield return ContinuousController.instance.StartCoroutine(new DeckBottomBounceClass(new List<Permanent> { card.PermanentOfThisCard() }, hashtable).DeckBounce());

                        yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());

                        if (CardEffectCommons.HasMatchConditionOwnersHand(card, HasProperTamer))
                        {
                            int maxCount = Math.Min(1, card.Owner.HandCards.Count(HasProperTamer));

                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: HasProperTamer,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                maxCount: maxCount,
                                canNoSelect: true,
                                canEndNotMax: false,
                                isShowOpponent: false,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: SelectCardCoroutine,
                                mode: SelectHandEffect.Mode.Custom,
                                cardEffect: activateClass);

                            selectHandEffect.SetUpCustomMessage("Select Tamer to play.", "The opponent is selecting a Tamer to play.");

                            yield return StartCoroutine(selectHandEffect.Activate());

                            IEnumerator SelectCardCoroutine(List<CardSource> selectedCards)
                            {
                                if (selectedCards.Count > 0)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                        cardSources: selectedCards,
                                        activateClass: activateClass,
                                        payCost: false,
                                        isTapped: false,
                                        root: SelectCardEffect.Root.Hand,
                                        activateETB: true));
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            #region Security Effect

            if (timing == EffectTiming.SecuritySkill)
            {
                cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
            }

            #endregion

            return cardEffects;
        }
    }
}