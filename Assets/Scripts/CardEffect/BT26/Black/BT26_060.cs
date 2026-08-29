using System;
using System.Collections;
using System.Collections.Generic;

// Chronomon: Destroy Mode
namespace DCGO.CardEffects.BT26
{
    public class BT26_060 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Alternate Digivolution Requirement - Lv.6 w/[Chronomon] in text
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.HasText("Chronomon");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 5, ignoreDigivolutionRequirement: false, card: card, condition: null, level: 6));
            }
            #endregion

            #region Alternate Digivolution Requirement - [Giant Slayer]
            if (timing == EffectTiming.None)
            {
                static bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsCardName("Giant Slayer");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 5, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }
            #endregion

            #region Security A. +1
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Reboot
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.RebootSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Blocker
            if (timing == EffectTiming.None)
            {
                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
            }
            #endregion

            #region Succession
            if (timing == EffectTiming.None)
            {
                bool CardCondition(CardSource cardSource)
                    => cardSource.HasLevel && cardSource.Level == 6 && cardSource.ContainsCardName("Chronomon");

                cardEffects.Add(CardEffectFactory.SuccessionSelfEffect(isInheritedEffect: false, card: card, condition: null, cardCondition: CardCondition));
            }
            #endregion

            #region Shared On Play / When Digivolving

            string SharedEffectName() => "Return the top 5 stacked cards of 3 opponent's Digimon to the top of their deck";

            string SharedEffectDescription(string tag)
                => $"[{tag}] Return the top 5 stacked cards of 3 of your opponent's Digimon to the top of the deck.";

            bool CanSelectTargetCondition(Permanent permanent)
                => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

            IEnumerator SharedActivateCoroutine(Hashtable hashtable, ActivateClass activateClass)
            {
                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectTargetCondition))
                {
                    int maxCount = Math.Min(3, CardEffectCommons.MatchConditionPermanentCount(CanSelectTargetCondition));

                    List<Permanent> selectedPermanents = new List<Permanent>();

                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectTargetCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: maxCount,
                        canNoSelect: false,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        selectedPermanents.Add(permanent);
                        yield return null;
                    }

                    selectPermanentEffect.SetUpCustomMessage($"Select {maxCount} Digimon to return the top 5 stacked cards from.", "The opponent is selecting Digimon to return the top 5 stacked cards from.");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    foreach (Permanent selectedPermanent in selectedPermanents)
                    {
                        if (selectedPermanent == null) continue;
                        if (selectedPermanent.TopCard == null) continue;
                        if (selectedPermanent.ImmuneFromStackReturnToLibrary(activateClass)) continue;
                        if (selectedPermanent.TopCard.CanNotBeAffected(activateClass)) continue;

                        int returnCount = Math.Min(selectedPermanent.StackCards.Count, 5);

                        if (returnCount <= 0) continue;

                        List<CardSource> cardsToReturn = selectedPermanent.StackCards.GetRange(0, returnCount);

                        if (returnCount == 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryTopCards(cardsToReturn, cardEffect: activateClass));
                            continue;
                        }

                        List<CardSource> drawOrderedCards = new List<CardSource>();

                        SelectCardEffect selectOrderEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectOrderEffect.SetUp(
                            canTargetCondition: (cardSource) => true,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            canNoSelect: () => false,
                            selectCardCoroutine: null,
                            afterSelectCardCoroutine: AfterSelectOrderCoroutine,
                            message: $"Select the order to return these {returnCount} cards to the top of the opponent's deck (card #1 is the one they'll draw first, #2 second, and so on).",
                            maxCount: returnCount,
                            canEndNotMax: false,
                            isShowOpponent: false,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Custom,
                            customRootCardList: cardsToReturn,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        selectOrderEffect.SetUpCustomMessage_ShowCard("Returned Cards");

                        yield return ContinuousController.instance.StartCoroutine(selectOrderEffect.Activate());

                        IEnumerator AfterSelectOrderCoroutine(List<CardSource> cardSources)
                        {
                            drawOrderedCards = cardSources.Clone();
                            yield return null;
                        }

                        drawOrderedCards.Reverse();

                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryTopCards(drawOrderedCards, cardEffect: activateClass));
                    }
                }
            }

            #endregion

            #region On Play / When Digivolving
            CardEffectFactory.ActivateClassesForSharedEffects(
                ref cardEffects, timing, card,
                SharedEffectName(),
                SharedActivateCoroutine,
                SharedEffectDescription,
                optional: false,
                additionalActivateCondition: (hash, ac) => CardEffectCommons.HasMatchConditionPermanent(CanSelectTargetCondition),
                onPlay: true,
                whenDigivolving: true);
            #endregion

            #region All Turns - Reactive Delete
            if (timing == EffectTiming.OnAddLibraryAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("May delete 1 opponent's Digimon", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, false, EffectDescription());
                activateClass.SetIsSkippable(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                    => "[All Turns] [Once Per Turn] When your effects add to decks, you may delete 1 of your opponent's Digimon.";

                bool CanSelectDeleteTargetCondition(Permanent permanent)
                    => CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);

                bool CardEffectCondition(ICardEffect cardEffect)
                    => CardEffectCommons.IsOwnerEffect(cardEffect, card);

                bool CanUseCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnAddLibrary(hashtable, null, CardEffectCondition);

                bool CanActivateCondition(Hashtable hashtable)
                    => CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass)
                        && CardEffectCommons.HasMatchConditionPermanent(CanSelectDeleteTargetCondition);

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    bool isUsed = false;

                    SelectPermanentEffect selectDeleteEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectDeleteEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectDeleteTargetCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: SelectPermanentCoroutine,
                        afterSelectPermanentCoroutine: null,
                        mode: SelectPermanentEffect.Mode.Destroy,
                        cardEffect: activateClass);

                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                    {
                        if (permanent != null) isUsed = true;
                        yield return null;
                    }

                    selectDeleteEffect.SetUpCustomMessage("Select 1 Digimon to delete.", "The opponent is selecting 1 Digimon to delete.");

                    yield return ContinuousController.instance.StartCoroutine(selectDeleteEffect.Activate());

                    if (!isUsed) activateClass.RemoveUse();
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
