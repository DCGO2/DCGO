using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT20
{
    public class BT20_098 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Main Effect

            if (timing == EffectTiming.OptionSkill)
            {
                int maxLevel = 9;

                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[Main] By returning 9 levels' total worth of Digimon cards from your opponent's trash to the bottom of the deck, you may play 1 [Ghost] trait Digimon card of each returned card's level from your trash without paying the costs. Then, the Digimon this effect played gain <Rush> and <Blocker> until the end of your opponent's turn.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                bool CanSelectReturnCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon && cardSource.HasLevel && cardSource.Level <= maxLevel;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionOpponentsCardInTrash(card, CanSelectReturnCardCondition))
                    {
                        List<CardSource> selectedCardsToReturn = new();

                        int maxCount = Math.Min(3,
                            CardEffectCommons.MatchConditionOpponentsCardCountInTrash(card, CanSelectReturnCardCondition));

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                            canTargetCondition: CanSelectReturnCardCondition,
                            canTargetCondition_ByPreSelecetedList: CanTargetConditionByPreSelectedList,
                            canEndSelectCondition: CanEndSelectCondition,
                            canNoSelect: () => true,
                            selectCardCoroutine: SelectCardCoroutine1,
                            afterSelectCardCoroutine: null,
                            message:
                            "Select cards to place at the bottom of the deck\n(cards will be placed back to the bottom of the deck so that cards with lower numbers are on top).",
                            maxCount: maxCount,
                            canEndNotMax: true,
                            isShowOpponent: true,
                            mode: SelectCardEffect.Mode.Custom,
                            root: SelectCardEffect.Root.Custom,
                            customRootCardList: card.Owner.Enemy.TrashCards,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                        IEnumerator SelectCardCoroutine1(CardSource cardSource)
                        {
                            selectedCardsToReturn.Add(cardSource);

                            yield return null;
                        }

                        bool CanEndSelectCondition(List<CardSource> cardSources)
                        {
                            if (cardSources.Count <= 0)
                            {
                                return false;
                            }

                            int sumLevel = cardSources.Sum(source => source.Level);

                            if (sumLevel != maxLevel)
                            {
                                return false;
                            }

                            return true;
                        }

                        bool CanTargetConditionByPreSelectedList(List<CardSource> cardSources, CardSource cardSource)
                        {
                            int sumLevel = cardSources.Sum(source => source.Level);

                            sumLevel += cardSource.Level;

                            if (sumLevel > maxLevel)
                            {
                                return false;
                            }

                            return true;
                        }

                        if (selectedCardsToReturn.Count >= 1)
                        {
                            // Return opponent's cards from trash to the bottom of their deck

                            yield return ContinuousController.instance.StartCoroutine(
                                CardObjectController.AddLibraryBottomCards(selectedCardsToReturn));

                            // For each returned card, choose a card from your trash with the same level, then play them

                            List<CardSource> selectedCardsToPlay = new List<CardSource>();

                            IEnumerator SelectCardCoroutine2(CardSource cardSource)
                            {
                                selectedCardsToPlay.Add(cardSource);

                                yield return null;
                            }

                            foreach (CardSource returnedCard in selectedCardsToReturn)
                            {
                                bool CanSelectPlayCardCondition(CardSource cardSource)
                                {
                                    return cardSource.IsDigimon && cardSource.EqualsTraits("Ghost") &&
                                           cardSource.HasLevel && cardSource.Level == returnedCard.Level &&
                                           !selectedCardsToPlay.Contains(cardSource) &&
                                           CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false,
                                               cardEffect: activateClass);
                                }

                                bool CanNoSelect()
                                {
                                    return selectedCardsToPlay.Count == 0;
                                }

                                bool RemoveOptionsAlreadySelected(List<CardSource> cardSources, CardSource cardSource)
                                {
                                    if (cardSources.Contains(cardSource))
                                        return false;

                                    return true;
                                }

                                selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectPlayCardCondition,
                                    canTargetCondition_ByPreSelecetedList: RemoveOptionsAlreadySelected,
                                    canEndSelectCondition: null,
                                    canNoSelect: CanNoSelect,
                                    selectCardCoroutine: SelectCardCoroutine2,
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

                                selectCardEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                                selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());
                            }

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                                cardSources: selectedCardsToPlay,
                                activateClass: activateClass,
                                payCost: false,
                                isTapped: false,
                                root: SelectCardEffect.Root.Trash,
                                activateETB: true));

                            // Give all played cards <Rush> and <Blocker>

                            foreach (CardSource playedCard in selectedCardsToPlay)
                            {
                                Permanent permanent = playedCard.PermanentOfThisCard();

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainRush(
                                    targetPermanent: permanent,
                                    effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                    activateClass: activateClass));

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainBlocker(
                                    targetPermanent: permanent,
                                    effectDuration: EffectDuration.UntilOpponentTurnEnd,
                                    activateClass: activateClass));
                            }
                        }
                    }
                }
            }

            #endregion

            #region Security Effect

            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect($"Play a Digimon from trash without paying the cost", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDescription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return
                        "[Security] You may play 1 level 5 or lower Digimon card with the [Ghost] trait from your trash without paying the cost.";
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return cardSource.IsDigimon && cardSource.EqualsTraits("Ghost") &&
                           cardSource.HasLevel && cardSource.Level <= 5 &&
                           CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                    selectCardEffect.SetUp(
                        canTargetCondition: CanSelectCardCondition,
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

                    selectCardEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                    selectCardEffect.SetUpCustomMessage_ShowCard("Played Card");

                    yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

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

            #endregion

            return cardEffects;
        }
    }
}