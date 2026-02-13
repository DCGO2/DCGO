using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT14
{
    public class BT14_033 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("This Digimon digivolves into Digimon card in security", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Start of Your Main Phase] Search your security stack. This Digimon may digivolve into a yellow Digimon card with the [Vaccine] trait among them without paying the cost. Then, shuffle your security stack. If digivolved by this effect, you may place 1 yellow card with the [Vaccine] trait from your hand at the bottom of your security stack.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.CardTraits.Contains("Vaccine"))
                    {
                        if (cardSource.CardColors.Contains(CardColor.Yellow))
                        {
                            if (cardSource.IsDigimon)
                            {
                                if (CardEffectCommons.IsExistOnBattleArea(card))
                                {
                                    if (cardSource.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame, false, activateClass))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition1(CardSource cardSource)
                {
                    if (cardSource.CardTraits.Contains("Vaccine"))
                    {
                        if (cardSource.CardColors.Contains(CardColor.Yellow))
                        {
                            return true;
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
                        if (card.Owner.SecurityCards.Count >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.SecurityCards.Count >= 1)
                    {
                        int maxCount = Math.Min(1, card.Owner.SecurityCards.Count);

                        CardSource selectedCard = null;

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine,
                            afterSelectCardCoroutine: null,
                            message: "Select 1 card to digivolve.",
                            maxCount: maxCount,
                            canEndNotMax: false,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Security,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCard = cardSource;

                            yield return null;
                        }

                        ContinuousController.instance.PlaySE(GManager.instance.ShuffleSE);

                        card.Owner.SecurityCards = RandomUtility.ShuffledDeckCards(card.Owner.SecurityCards);

                        if (selectedCard != null)
                        {
                            if (CardEffectCommons.IsExistOnBattleArea(card))
                            {
                                GManager.instance.turnStateMachine.gameContext.IsSecurityLooking = true;

                                if (selectedCard.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame, PayCost: false, activateClass, root: SelectCardEffect.Root.Security))
                                {
                                    PlayCardClass playCardClass = new PlayCardClass(
                                        cardSources: new List<CardSource>() { selectedCard },
                                        hashtable: CardEffectCommons.CardEffectHashtable(activateClass),
                                        payCost: false,
                                        targetPermanent: card.PermanentOfThisCard(),
                                        isTapped: false,
                                        root: SelectCardEffect.Root.Security,
                                        activateETB: true);

                                    yield return ContinuousController.instance.StartCoroutine(playCardClass.PlayCard());
                                }

                                GManager.instance.turnStateMachine.gameContext.IsSecurityLooking = false;

                                if (CardEffectCommons.IsDigivolvedByTheEffect(card.PermanentOfThisCard(), selectedCard, activateClass))
                                {
                                    if (card.Owner.CanAddSecurity(activateClass))
                                    {
                                        if (card.Owner.HandCards.Count(CanSelectCardCondition1) >= 1)
                                        {
                                            List<CardSource> selectedCards = new List<CardSource>();

                                            maxCount = 1;

                                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                                            selectHandEffect.SetUp(
                                                selectPlayer: card.Owner,
                                                canTargetCondition: CanSelectCardCondition1,
                                                canTargetCondition_ByPreSelecetedList: null,
                                                canEndSelectCondition: null,
                                                maxCount: maxCount,
                                                canNoSelect: true,
                                                canEndNotMax: false,
                                                isShowOpponent: true,
                                                selectCardCoroutine: SelectCardCoroutine1,
                                                afterSelectCardCoroutine: null,
                                                mode: SelectHandEffect.Mode.Custom,
                                                cardEffect: activateClass);

                                            selectHandEffect.SetUpCustomMessage(
                                                "Select 1 card to place at the bottom of security.",
                                                "The opponent is selecting 1 card to place at the bottom of security.");
                                            selectHandEffect.SetUpCustomMessage_ShowCard("Security Bottom Card");

                                            yield return StartCoroutine(selectHandEffect.Activate());

                                            IEnumerator SelectCardCoroutine1(CardSource cardSource)
                                            {
                                                selectedCards.Add(cardSource);

                                                yield return null;
                                            }

                                            foreach (CardSource cardSource in selectedCards)
                                            {
                                                yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddSecurityCard(cardSource, toTop: false));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnAddSecurity)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Memory +1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Memory+1_BT14_033");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn][Once Per Turn] When a card is added to your security stack, gain 1 memory.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerWhenAddSecurity(hashtable, player => player == card.Owner))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(card.Owner.AddMemory(1, activateClass));
                }
            }

            return cardEffects;
        }
    }
}