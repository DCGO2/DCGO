using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT12
{
    public class BT12_081 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.CardNames.Contains("Psychemon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 5, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasSaveText && (targetPermanent.TopCard.CardColors.Contains(CardColor.Yellow) || targetPermanent.TopCard.CardColors.Contains(CardColor.Green) || targetPermanent.TopCard.CardColors.Contains(CardColor.Purple)) && targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 4;
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play 1 digivolution card or this Digimon digivolves to [Quartzmon]", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] You may play 1 level 4 or lower Digimon card with <Save> from under one of your Tamers without paying its cost. If there are 4 or more digivolution cards under this Digimon, it can be digivolved into a [Quartzmon] in your hand or under one of your Tamers with the digivolution cost reduced by 3 instead.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent.IsTamer)
                        {
                            if (permanent.DigivolutionCards.Count(CanSelectCardCondition) >= 1)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.Level <= 4)
                        {
                            if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                            {
                                if (cardSource.HasLevel)
                                {
                                    if (cardSource.HasSaveText)
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
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().DigivolutionCards.Count >= 4)
                        {
                            bool CanSelectQuartzmonCardCondition(CardSource cardSource)
                            {
                                if (cardSource.CardNames.Contains("Quartzmon"))
                                {
                                    if (cardSource.CanPlayCardTargetFrame(card.PermanentOfThisCard().PermanentFrame, PayCost: true, cardEffect: activateClass))
                                    {
                                        return true;
                                    }
                                }

                                return false;
                            }

                            bool CanSelectQuartzmonTamerCondition(Permanent permanent)
                            {
                                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                                {
                                    if (permanent.IsTamer)
                                    {
                                        if (permanent.DigivolutionCards.Some(CanSelectQuartzmonCardCondition))
                                        {
                                            return true;
                                        }
                                    }
                                }

                                return false;
                            }

                            if (card.Owner.HandCards.Count >= 1)
                            {
                                return true;
                            }

                            if (card.Owner.GetBattleAreaPermanents().Some(CanSelectQuartzmonTamerCondition))
                            {
                                return true;
                            }
                        }
                        else
                        {
                            if (card.Owner.GetBattleAreaPermanents().Some(CanSelectPermanentCondition))
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
                        bool canEvo = card.PermanentOfThisCard().DigivolutionCards.Count >= 4;

                        if (canEvo)
                        {
                            List<SelectionElement<int>> selectionElements = new List<SelectionElement<int>>()
                            {
                                new SelectionElement<int>(message: $"Play From Sources", value : 1, spriteIndex: 0),
                                new SelectionElement<int>(message: $"Digivolve to [Quartzmon]", value : 0, spriteIndex: 1),
                            };

                            string selectPlayerMessage = "Which effect would you like to do?";
                            string notSelectPlayerMessage = "The opponent is choosing effect to do.";

                            GManager.instance.userSelectionManager.SetIntSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                        }
                        else
                        {
                            GManager.instance.userSelectionManager.SetInt(1);
                        }

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());
                        
                        if (GManager.instance.userSelectionManager.SelectedIntValue == 0)
                        {
                            #region Reduce Cost

                            ChangeCostClass changeCostClass = new ChangeCostClass();
                            changeCostClass.SetUpICardEffect("Digivolution Cost -3", CanUseCondition1, card);
                            changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                            Func<EffectTiming, ICardEffect> getCardEffect = GetCardEffect;
                            card.Owner.UntilCalculateFixedCostEffect.Add(getCardEffect);

                            ICardEffect GetCardEffect(EffectTiming _timing)
                            {
                                if (_timing == EffectTiming.None)
                                {
                                    return changeCostClass;
                                }

                                return null;
                            }

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
                                            Cost -= 3;
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
                                return targetPermanent == card.PermanentOfThisCard();
                            }

                            bool CardSourceCondition(CardSource cardSource)
                            {
                                return cardSource.Owner == card.Owner;
                            }

                            bool RootCondition(SelectCardEffect.Root root)
                            {
                                return true;
                            }

                            bool isUpDown()
                            {
                                return true;
                            }

                            #endregion

                            bool CanSelectQuartzmonCardCondition(CardSource cardSource)
                            {
                                if (cardSource.CardNames.Contains("Quartzmon"))
                                {
                                    SelectCardEffect.Root root = CardEffectCommons.HasMatchConditionOwnersPermanent(
                                        card, (permanent) => permanent.DigivolutionCards.Contains(cardSource))
                                        ? SelectCardEffect.Root.DigivolutionCards
                                        : SelectCardEffect.Root.Hand;

                                    if (cardSource.CanPlayCardTargetFrame(
                                        card.PermanentOfThisCard().PermanentFrame,
                                        PayCost: true,
                                        cardEffect: activateClass,
                                        root: root))
                                    {
                                        return true;
                                    }
                                }

                                return false;
                            }

                            bool CanSelectQuartzmonTamerCondition(Permanent permanent)
                            {
                                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                                {
                                    if (permanent.IsTamer)
                                    {
                                        if (permanent.DigivolutionCards.Some(CanSelectQuartzmonCardCondition))
                                        {
                                            return true;
                                        }
                                    }
                                }

                                return false;
                            }

                            bool canSelectHand = card.Owner.HandCards.Some(CanSelectQuartzmonCardCondition);
                            bool canSelectTamer = CardEffectCommons.HasMatchConditionPermanent(CanSelectQuartzmonTamerCondition);

                            if (canSelectHand || canSelectTamer)
                            {
                                if (canSelectHand && canSelectTamer)
                                {
                                    List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>()
                                    {
                                        new SelectionElement<bool>(message: $"From hand", value : true, spriteIndex: 0),
                                        new SelectionElement<bool>(message: $"Under Tamer", value : false, spriteIndex: 1),
                                    };

                                    string selectPlayerMessage = "From which area do you select 1 [Quartzmon]?";
                                    string notSelectPlayerMessage = "The opponent is selecting from which area to select 1 [Quartzmon].";

                                    GManager.instance.userSelectionManager.SetBoolSelection(selectionElements: selectionElements, selectPlayer: card.Owner, selectPlayerMessage: selectPlayerMessage, notSelectPlayerMessage: notSelectPlayerMessage);
                                }
                                else
                                {
                                    GManager.instance.userSelectionManager.SetBool(canSelectHand);
                                }

                                yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                                bool fromHand = GManager.instance.userSelectionManager.SelectedBoolValue;

                                if (fromHand)
                                {
                                    if (card.Owner.HandCards.Count(CanSelectQuartzmonCardCondition) >= 1)
                                    {
                                        List<CardSource> selectedCards = new List<CardSource>();

                                        int maxCount = 1;

                                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                                        selectHandEffect.SetUp(
                                            selectPlayer: card.Owner,
                                            canTargetCondition: CanSelectQuartzmonCardCondition,
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

                                        selectHandEffect.SetUpCustomMessage("Select 1 [Quartzmon] to digivolve.", "The opponent is selecting 1 [Quartzmon] to digivolve.");
                                        selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                                        yield return StartCoroutine(selectHandEffect.Activate());

                                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                                        {
                                            selectedCards.Add(cardSource);

                                            yield return null;
                                        }

                                        yield return ContinuousController.instance.StartCoroutine(new PlayCardClass(
                                                        cardSources: selectedCards,
                                                        hashtable: CardEffectCommons.CardEffectHashtable(activateClass),
                                                        payCost: true,
                                                        targetPermanent: card.PermanentOfThisCard(),
                                                        isTapped: false,
                                                        root: SelectCardEffect.Root.Hand,
                                                        activateETB: true).PlayCard());
                                    }
                                }
                                else
                                {
                                    if (card.Owner.GetBattleAreaPermanents().Some(CanSelectQuartzmonTamerCondition))
                                    {
                                        Permanent selectedPermanent = null;

                                        int maxCount = Math.Min(1, card.Owner.GetBattleAreaPermanents().Count(CanSelectQuartzmonTamerCondition));

                                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                        selectPermanentEffect.SetUp(
                                            selectPlayer: card.Owner,
                                            canTargetCondition: CanSelectQuartzmonTamerCondition,
                                            canTargetCondition_ByPreSelecetedList: null,
                                            canEndSelectCondition: null,
                                            maxCount: maxCount,
                                            canNoSelect: true,
                                            canEndNotMax: false,
                                            selectPermanentCoroutine: SelectPermanentCoroutine,
                                            afterSelectPermanentCoroutine: null,
                                            mode: SelectPermanentEffect.Mode.Custom,
                                            cardEffect: activateClass);

                                        selectPermanentEffect.SetUpCustomMessage("Select 1 Tamer.", "The opponent is selecting 1 Tamer.");

                                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                        {
                                            selectedPermanent = permanent;

                                            yield return null;
                                        }

                                        if (selectedPermanent != null)
                                        {
                                            if (selectedPermanent.DigivolutionCards.Some((cardSource) => CanSelectQuartzmonCardCondition(cardSource)))
                                            {
                                                maxCount = Math.Min(1, selectedPermanent.DigivolutionCards.Count((cardSource) => CanSelectQuartzmonCardCondition(cardSource)));

                                                List<CardSource> selectedCards = new List<CardSource>();

                                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                                selectCardEffect.SetUp(
                                                            canTargetCondition: CanSelectQuartzmonCardCondition,
                                                            canTargetCondition_ByPreSelecetedList: null,
                                                            canEndSelectCondition: null,
                                                            canNoSelect: () => false,
                                                            selectCardCoroutine: SelectCardCoroutine,
                                                            afterSelectCardCoroutine: null,
                                                            message: "Select 1 [Quartzmon] to digivolve from digivolution cards.",
                                                            maxCount: maxCount,
                                                            canEndNotMax: false,
                                                            isShowOpponent: true,
                                                            mode: SelectCardEffect.Mode.Custom,
                                                            root: SelectCardEffect.Root.Custom,
                                                            customRootCardList: selectedPermanent.DigivolutionCards,
                                                            canLookReverseCard: true,
                                                            selectPlayer: card.Owner,
                                                            cardEffect: activateClass);

                                                selectCardEffect.SetUpCustomMessage(
                                                    "Select 1 [Quartzmon] to digivolve from digivolution cards.",
                                                    "The opponent is selecting 1 [Quartzmon] to digivolve from digivolution cards.");

                                                selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                                                yield return StartCoroutine(selectCardEffect.Activate());

                                                IEnumerator SelectCardCoroutine(CardSource cardSource)
                                                {
                                                    selectedCards.Add(cardSource);
                                                    cardSource.willBeRemoveSources = true;

                                                    yield return null;
                                                }

                                                yield return ContinuousController.instance.StartCoroutine(new PlayCardClass(
                                                                cardSources: selectedCards,
                                                                hashtable: CardEffectCommons.CardEffectHashtable(activateClass),
                                                                payCost: true,
                                                                targetPermanent: card.PermanentOfThisCard(),
                                                                isTapped: false,
                                                                root: SelectCardEffect.Root.DigivolutionCards,
                                                                activateETB: true).PlayCard());
                                            }
                                        }
                                    }
                                }
                            }

                            #region End Cost Reduction

                            card.Owner.UntilCalculateFixedCostEffect.Remove(getCardEffect);

                            #endregion
                        }
                        else
                        {
                            if (card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1)
                            {
                                int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

                                SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                selectPermanentEffect.SetUp(
                                    selectPlayer: card.Owner,
                                    canTargetCondition: CanSelectPermanentCondition,
                                    canTargetCondition_ByPreSelecetedList: null,
                                    canEndSelectCondition: null,
                                    maxCount: maxCount,
                                    canNoSelect: true,
                                    canEndNotMax: false,
                                    selectPermanentCoroutine: SelectPermanentCoroutine,
                                    afterSelectPermanentCoroutine: null,
                                    mode: SelectPermanentEffect.Mode.Custom,
                                    cardEffect: activateClass);

                                selectPermanentEffect.SetUpCustomMessage("Select 1 Tamer which has digivolution cards.", "The opponent is selecting 1 Tamer which has digivolution cards.");

                                yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                {
                                    Permanent selectedPermanent = permanent;

                                    if (selectedPermanent != null)
                                    {
                                        if (selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition) >= 1)
                                        {
                                            maxCount = Math.Min(1, selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition));

                                            List<CardSource> selectedCards = new List<CardSource>();

                                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                            selectCardEffect.SetUp(
                                                        canTargetCondition: CanSelectCardCondition,
                                                        canTargetCondition_ByPreSelecetedList: null,
                                                        canEndSelectCondition: null,
                                                        canNoSelect: () => true,
                                                        selectCardCoroutine: SelectCardCoroutine,
                                                        afterSelectCardCoroutine: null,
                                                        message: "Select 1 digivolution card to play.",
                                                        maxCount: maxCount,
                                                        canEndNotMax: false,
                                                        isShowOpponent: true,
                                                        mode: SelectCardEffect.Mode.Custom,
                                                        root: SelectCardEffect.Root.Custom,
                                                        customRootCardList: selectedPermanent.DigivolutionCards,
                                                        canLookReverseCard: true,
                                                        selectPlayer: card.Owner,
                                                        cardEffect: activateClass);

                                            selectCardEffect.SetUpCustomMessage("Select 1 digivolution card to play.", "The opponent is selecting 1 digivolution card to play.");
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
                                                root: SelectCardEffect.Root.DigivolutionCards,
                                                activateETB: true));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.OnAllyAttack)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Draw 1", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDiscription());
                activateClass.SetIsInheritedEffect(true);
                activateClass.SetHashString("Draw1_BT12_081");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Attacking][Once Per Turn] If this Digimon has <Save> in its text, <Draw 1>. (Draw 1 card from your deck.)";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().TopCard.HasSaveText)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    yield return ContinuousController.instance.StartCoroutine(new DrawClass(card.Owner, 1, activateClass).Draw());
                }
            }

            return cardEffects;
        }
    }
}