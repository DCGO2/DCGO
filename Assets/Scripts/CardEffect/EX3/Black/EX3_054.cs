using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX3
{
    public class EX3_054 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Reduce Digivolution Cost", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetHashString("DigivolutionCost-_EX3_054");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "When you would digivolve into this card, by returning up to 5 cards with [D-Brigade] in their traits from your trash to the top of your deck, reduce the digivolution cost by 1 for each returned card.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.CardTraits.Contains("D-Brigade");
                }

                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card);
                }

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenPermanentWouldDigivolve(hashtable, PermanentCondition, CardCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, CanSelectCardCondition))
                    {
                        int maxCount = Math.Min(5, card.Owner.TrashCards.Count(CanSelectCardCondition));

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectCardCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: CanEndSelectCondition,
                            canNoSelect: () => true,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AfterSelectCardCoroutine,
                            message: "Select cards to return to the top of deck\n(cards will be placed back to the top of the deck so that cards with lower numbers are on top).",
                            maxCount: maxCount,
                            canEndNotMax: true,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Trash,
                            customRootCardList: null,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectCardEffect.SetNotAddLog();
                        selectCardEffect.SetUpCustomMessage_ShowCard("Deck Top Cards");
                        selectCardEffect.SetUpCustomMessage(
                            "Select cards to put on top of the deck.",
                            "The opponent is selecting cards to put on top of the deck.");

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        bool CanEndSelectCondition(List<CardSource> cardSources)
                        {
                            if (CardEffectCommons.HasNoElement(cardSources))
                            {
                                return false;
                            }

                            return true;
                        }

                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            if (cardSources.Count >= 1)
                            {
                                cardSources.Reverse();

                                yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryTopCards(cardSources));

                                int minusCount = cardSources.Count;

                                ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                                ChangeCostClass changeCostClass = new ChangeCostClass();
                                changeCostClass.SetUpICardEffect($"Digivolution Cost -{minusCount}", CanUseCondition1, card);
                                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                                card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(_hashtable));

                                bool CanUseCondition1(Hashtable hashtable)
                                {
                                    return true;
                                }

                                int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                                {
                                    if (CardSourceCondition(cardSource))
                                    {
                                        if (RootCondition(root))
                                        {
                                            if (PermanentsCondition(targetPermanents))
                                            {
                                                Cost -= minusCount;
                                            }
                                        }
                                    }

                                    return Cost;
                                }

                                bool PermanentsCondition(List<Permanent> targetPermanents)
                                {
                                    if (targetPermanents != null)
                                    {
                                        if (targetPermanents.Count(PermanentCondition) >= 1)
                                        {
                                            return true;
                                        }
                                    }

                                    return false;
                                }

                                bool PermanentCondition(Permanent targetPermanent)
                                {
                                    if (targetPermanent.TopCard != null)
                                    {
                                        if (targetPermanent.TopCard.Owner == card.Owner)
                                        {
                                            if (targetPermanent.TopCard.Owner.GetBattleAreaPermanents().Contains(targetPermanent))
                                            {
                                                return true;
                                            }
                                        }
                                    }

                                    return false;
                                }

                                bool CardSourceCondition(CardSource cardSource)
                                {
                                    if (cardSource != null)
                                    {
                                        if (cardSource == card)
                                        {
                                            return true;
                                        }
                                    }

                                    return false;
                                }

                                bool RootCondition(SelectCardEffect.Root root)
                                {
                                    return true;
                                }

                                bool isUpDown()
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete 1 Digimon and unsuspend this Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetHashString("Unsuspend_EX3_054");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Your Turn][Once Per Turn] When you play another Digimon with [D-Brigade] in its traits, delete 1 of your opponent's Digimon with a play cost less than or equal to the Digimon you played, and unsuspend this Digimon.";
                }

                bool PermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent != card.PermanentOfThisCard())
                        {
                            if (permanent.TopCard.CardTraits.Contains("D-Brigade"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool PermanentCondition1(Permanent permanent)
                {
                    if (permanent != null)
                    {
                        if (permanent != card.PermanentOfThisCard())
                        {
                            if (permanent.TraitsJustAfterPlayed.Contains("D-Brigade"))
                            {
                                if (permanent.PlayCostJustAfterPlayed >= 0)
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
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            if (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PermanentCondition))
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
                    List<Permanent> permanents = CardEffectCommons.GetPlayedPermanentsFromEnterFieldHashtable(
                        hashtable: _hashtable,
                        rootCondition: null);

                    if (permanents != null)
                    {
                        int maxCost = -1;

                        List<int> costs = permanents
                        .Filter(PermanentCondition1)
                        .Map(permanent => permanent.PlayCostJustAfterPlayed);

                        if (costs.Count >= 1)
                        {
                            maxCost = costs.Max();
                        }

                        bool CanSelectPermanentCondition(Permanent permanent)
                        {
                            if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                            {
                                if (permanent.TopCard.HasPlayCost)
                                {
                                    if (permanent.TopCard.GetCostItself <= maxCost)
                                    {
                                        return true;
                                    }
                                }
                            }

                            return false;
                        }

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

                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        Permanent selectedPermanent = card.PermanentOfThisCard();

                        yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { selectedPermanent }, activateClass).Unsuspend());
                    }
                }
            }

            return cardEffects;
        }
    }
}