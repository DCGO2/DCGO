using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class BT7_015 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            int count()
            {
                int count = 0;

                foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
                {
                    count += player.TrashCards.Count((cardSource) => cardSource.IsOption);
                }

                return count;
            }

            ChangeCostClass changeCostClass = new ChangeCostClass();
            changeCostClass.SetUpICardEffect($"Reduce Play Cost", CanUseCondition, card);
            changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
            cardEffects.Add(changeCostClass);

            bool CanUseCondition(Hashtable hashtable)
            {
                if (card.Owner.HandCards.Contains(card))
                {
                    return true;
                }

                return false;
            }

            int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
            {
                if (CardSourceCondition(cardSource))
                {
                    if (RootCondition(root))
                    {
                        if (PermanentsCondition(targetPermanents))
                        {
                            Cost -= count();
                        }
                    }
                }

                return Cost;
            }

            bool PermanentsCondition(List<Permanent> targetPermanents)
            {
                if (targetPermanents == null)
                {
                    return true;
                }

                else
                {
                    if (targetPermanents.Count((targetPermanent) => targetPermanent != null) == 0)
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CardSourceCondition(CardSource cardSource)
            {
                return cardSource == card;
            }

            bool RootCondition(SelectCardEffect.Root root)
            {
                if (root == SelectCardEffect.Root.Hand)
                {
                    return true;
                }

                return false;
            }

            bool isUpDown()
            {
                return true;
            }
        }

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Return cards from trash to deck and delete 1 Digimon", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[On Play] Return all cards with [Three Musketeers] in their traits and all Option cards from both players' trashes to the bottom of their owners' decks. If 7 or more cards were returned using this effect, delete 1 of your opponent's Digimon with [Three Musketeers] in its traits or 8000 DP or less.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                if (cardSource != null)
                {
                    if (cardSource.IsOption)
                    {
                        return true;
                    }

                    if (cardSource.CardTraits.Contains("Three Musketeers"))
                    {
                        return true;
                    }

                    if (cardSource.CardTraits.Contains("ThreeMusketeers"))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (permanent != null)
                {
                    if (permanent.TopCard != null)
                    {
                        if (permanent.IsDigimon)
                        {
                            if (permanent.TopCard.Owner == card.Owner.Enemy)
                            {
                                if (permanent.TopCard.Owner.GetBattleAreaPermanents().Contains(permanent))
                                {
                                    if (permanent.DP <= card.Owner.MaxDP_DeleteEffect(8000, activateClass))
                                    {
                                        return true;
                                    }

                                    if (permanent.TopCard.CardTraits.Contains("ThreeMusketeers"))
                                    {
                                        return true;
                                    }

                                    if (permanent.TopCard.CardTraits.Contains("Three Musketeers"))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }

                return false;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (isExistOnField(card))
                {
                    if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                    {
                        int count = 0;

                        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
                        {
                            count += player.TrashCards.Count(CanSelectCardCondition);
                        }

                        if (count >= 1)
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
                    if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                    {
                        int count = 0;

                        foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
                        {
                            count += player.TrashCards.Count(CanSelectCardCondition);
                        }

                        if (count >= 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(PutLibraryBottom(card.Owner));
                            yield return ContinuousController.instance.StartCoroutine(PutLibraryBottom(card.Owner.Enemy));

                            IEnumerator PutLibraryBottom(Player player)
                            {
                                if (player.TrashCards.Count(CanSelectCardCondition) >= 1)
                                {
                                    List<CardSource> selectedCards = new List<CardSource>();

                                    foreach (CardSource cardSource in player.TrashCards)
                                    {
                                        if (CanSelectCardCondition(cardSource))
                                        {
                                            selectedCards.Add(cardSource);
                                        }
                                    }

                                    if (selectedCards.Count >= 1)
                                    {
                                        List<CardSource> libraryBottomCards = new List<CardSource>();

                                        if (selectedCards.Count == 1)
                                        {
                                            foreach (CardSource cardSource in selectedCards)
                                            {
                                                libraryBottomCards.Add(cardSource);
                                            }
                                        }

                                        else
                                        {
                                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                            selectCardEffect.SetUp(
                                                canTargetCondition: (cardSource) => true,
                                                canTargetCondition_ByPreSelecetedList: null,
                                                canEndSelectCondition: null,
                                                canNoSelect: () => false,
                                                selectCardCoroutine: null,
                                                afterSelectCardCoroutine: AfterSelectCardCoroutine1,
                                                message: "Specify the order to place the card in the deck bottom\n(cards will be placed so that cards with lower numbers are on top).",
                                                maxCount: selectedCards.Count,
                                                canEndNotMax: false,
                                                isShowOpponent: false,
                                                mode: SelectCardEffect.Mode.Custom,
                                                root: SelectCardEffect.Root.Custom,
                                                customRootCardList: selectedCards,
                                                canLookReverseCard: true,
                                                selectPlayer: card.Owner,
                                                cardEffect: activateClass);

                                            selectCardEffect.SetNotShowCard();
                                            selectCardEffect.SetNotAddLog();

                                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                                            IEnumerator AfterSelectCardCoroutine1(List<CardSource> cardSources)
                                            {
                                                foreach (CardSource cardSource in cardSources)
                                                {
                                                    libraryBottomCards.Add(cardSource);
                                                }

                                                yield return null;
                                            }
                                        }

                                        if (libraryBottomCards.Count >= 1)
                                        {
                                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(libraryBottomCards));

                                            if (!player.isYou)
                                            {
                                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(libraryBottomCards, "Deck Bottom Cards", true, true));
                                            }

                                            else
                                            {
                                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(libraryBottomCards, "Deck Bottom Cards", true, true));
                                            }
                                        }
                                    }
                                }
                            }

                            if (count >= 7)
                            {
                                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                                {
                                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                    selectPermanentEffect.SetUp(
                                        selectPlayer: card.Owner,
                                        canTargetCondition: CanSelectPermanentCondition,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        maxCount: maxCount,
                                        canNoSelect: false,
                                        canEndNotMax: false,
                                        selectPermanentCoroutine: null,
                                        afterSelectPermanentCoroutine: null,
                                        mode: SelectPermanentEffect.Mode.Destroy,
                                        cardEffect: activateClass);

                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                                }
                            }
                        }
                    }
                }
            }
        }

        return cardEffects;
    }
}
