using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT13
{
    public class BT13_083 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete your 1 level 3 Digimon to get Play Cost -4", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetHashString("PlayCost-4_BT13_083");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "When you would play this card, by deleting 1 of your level 3 Digimon, reduce the play cost by 4.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.Level == 3)
                        {
                            if (permanent.TopCard.HasLevel)
                            {
                                if (!permanent.TopCard.CanNotBeAffected(activateClass))
                                {
                                    if (permanent.CanBeDestroyedBySkill(activateClass))
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
                    if (CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, cardSource => cardSource == card))
                    {
                        return true;
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        bool CanNoSelect =
                        CardEffectCommons.GetPlayCardClassFromHashtable(_hashtable) != null &&
                            card.PayingCost(
                                CardEffectCommons.GetPlayCardClassFromHashtable(_hashtable).Root,
                                null,
                                checkAvailability: false)
                            <= card.Owner.MaxMemoryCost;

                        int maxCount = 1;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
                            canNoSelect: CanNoSelect,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(targetPermanents: new List<Permanent>() { permanent }, activateClass: activateClass, successProcess: permanents => SuccessProcess(), failureProcess: null));

                            IEnumerator SuccessProcess()
                            {
                                if (card.Owner.CanReduceCost(null, card))
                                {
                                    ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);
                                }

                                ChangeCostClass changeCostClass = new ChangeCostClass();
                                changeCostClass.SetUpICardEffect("Play Cost -4", CanUseCondition1, card);
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
                                                Cost -= 4;
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

                                yield return null;
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.None)
            {
                ChangeCostClass changeCostClass = new ChangeCostClass();
                changeCostClass.SetUpICardEffect("Play Cost -4", CanUseCondition, card);
                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => true, isChangePayingCost: () => true);
                changeCostClass.SetNotShowUI(true);
                cardEffects.Add(changeCostClass);

                bool CanSelectPermanentCondition(Permanent permanent, ICardEffect activateClass)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.Level == 3)
                        {
                            if (permanent.TopCard.HasLevel)
                            {
                                if (!permanent.TopCard.CanNotBeAffected(activateClass))
                                {
                                    if (permanent.CanBeDestroyedBySkill(activateClass))
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
                    if (card.PermanentOfThisCard() == null)
                    {
                        ICardEffect activateClass = card.EffectList(EffectTiming.BeforePayCost).Find(cardEffect => cardEffect.EffectName == "Delete your 1 level 3 Digimon to get Play Cost -4");

                        if (activateClass != null)
                        {
                            if (CardEffectCommons.HasMatchConditionPermanent(
                                (permanent) => CanSelectPermanentCondition(permanent, activateClass)))
                            {
                                return true;
                            }
                        }
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
                                Cost -= 4;
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

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 2 and trash 2 cards from hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] <Draw 2>. Then, trash 2 cards in your hand.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.LibraryCards.Count >= 1)
                        {
                            return true;
                        }

                        if (card.Owner.HandCards.Count >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 2, activateClass).Draw());

                    if (card.Owner.HandCards.Count >= 1)
                    {
                        int discardCount = Math.Min(2, card.Owner.HandCards.Count);

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
            }

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.CanNotDigivolveStaticSelfEffect(
                cardCondition: null,
                isInheritedEffect: false,
                card: card,
                condition: Condition,
                effectName: "Can't digivolve")
                );
            }

            if (timing == EffectTiming.OnDestroyedAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 [Gizmon: XT] from trash", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Deletion] By returning 2 cards with [Gizmon] in their names from your trash to the bottom of the deck in any order, you may play 1 [Gizmon: XT] from your trash without paying the cost.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.ContainsCardName("Gizmon");
                }

                bool CanSelectCardCondition1(CardSource cardSource)
                {
                    if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass, root: SelectCardEffect.Root.Trash))
                    {
                        if (cardSource.CardNames.Contains("Gizmon: XT"))
                        {
                            return true;
                        }

                        if (cardSource.CardNames.Contains("Gizmon:XT"))
                        {
                            return true;
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
                    if (CardEffectCommons.CanActivateOnDeletion(card))
                    {
                        if (card.Owner.TrashCards.Count(CanSelectCardCondition) >= 2)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool returned = false;

                    if (card.Owner.TrashCards.Count(CanSelectCardCondition) >= 2)
                    {
                        int maxCount = 2;

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                        canTargetCondition: (cardSource) => CanSelectCardCondition(cardSource),
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        canNoSelect: () => false,
                        selectCardCoroutine: null,
                        afterSelectCardCoroutine: AfterSelectCardCoroutine,
                        message: "Select cards to place at the bottom of the deck\n(cards will be placed back to the bottom of the deck so that cards with lower numbers are on top).",
                        maxCount: maxCount,
                        canEndNotMax: false,
                        isShowOpponent: false,
                        mode: SelectCardEffect.Mode.Custom,
                        root: SelectCardEffect.Root.Trash,
                        customRootCardList: null,
                        canLookReverseCard: true,
                        selectPlayer: card.Owner,
                        cardEffect: activateClass);

                        selectCardEffect.SetNotShowCard();
                        selectCardEffect.SetNotAddLog();

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                        {
                            if (cardSources.Count == 2)
                            {
                                yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(cardSources));

                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(cardSources, "Deck Bottom Cards", true, true));

                                returned = true;
                            }
                        }
                    }

                    if (returned)
                    {
                        if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, (cardSource) => CanSelectCardCondition1(cardSource)))
                        {
                            int maxCount = Math.Min(1, card.Owner.TrashCards.Count((cardSource) => CanSelectCardCondition1(cardSource)));

                            List<CardSource> selectedCards = new List<CardSource>();

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                            selectCardEffect.SetUp(
                                        canTargetCondition: CanSelectCardCondition1,
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

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass, payCost: false, isTapped: false, root: SelectCardEffect.Root.Trash, activateETB: true));
                        }
                    }
                }
            }

            return cardEffects;
        }
    }
}