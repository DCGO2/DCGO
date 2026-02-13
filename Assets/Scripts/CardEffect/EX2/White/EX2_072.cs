using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.EX2
{
    public class EX2_072 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                IgnoreColorConditionClass ignoreColorConditionClass = new IgnoreColorConditionClass();
                ignoreColorConditionClass.SetUpICardEffect("Ignore color requirements", CanUseCondition, card);
                ignoreColorConditionClass.SetUpIgnoreColorConditionClass(cardCondition: CardCondition);

                cardEffects.Add(ignoreColorConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (card.Owner.GetBattleAreaPermanents().Count((permanet) => permanet.IsTamer) >= 1)
                    {
                        return true;
                    }

                    return false;
                }

                bool CardCondition(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        return true;
                    }

                    return false;
                }
            }
            if (timing == EffectTiming.OptionSkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Main] Reveal the top 5 cards of your deck. You may digivolve 1 of your Digimon into 1 non-white Digimon card among them without paying its memory cost. If you don't, add 1 Digimon card among them to your hand. Place the remaining cards at the bottom of your deck in any order.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (!cardSource.CardColors.Contains(CardColor.White))
                        {
                            foreach (Permanent permanent in card.Owner.GetBattleAreaDigimons())
                            {
                                if (cardSource.CanPlayCardTargetFrame(permanent.PermanentFrame, false, activateClass))
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition1(CardSource cardSource)
                {
                    return cardSource.IsDigimon;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.LibraryCards.Count >= 1)
                    {
                        bool digivolved = false;

                        List<CardSource> selectedCards = new List<CardSource>();

                        RevealLibraryClass revealLibrary = new RevealLibraryClass(card.Owner, 5);

                        yield return ContinuousController.instance.StartCoroutine(revealLibrary.RevealLibrary());

                        List<CardSource> libraryCards = new List<CardSource>();

                        int maxCount = Math.Min(1, revealLibrary.RevealedCards.Count(CanSelectCardCondition));

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUseFaceDown();

                        if (maxCount >= 1)
                        {
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
                            root: SelectCardEffect.Root.Custom,
                            customRootCardList: revealLibrary.RevealedCards,
                            canLookReverseCard: true,
                            selectPlayer: card.Owner,
                            cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                selectedCards.Add(cardSource);
                                yield return null;
                            }
                        }

                        if (selectedCards.Count >= 1)
                        {
                            CardSource selectedCard = selectedCards[0];

                            if (selectedCard != null)
                            {
                                List<Permanent> candidatePermanents = new List<Permanent>();

                                foreach (Permanent permanent in card.Owner.GetBattleAreaDigimons())
                                {
                                    if (selectedCard.CanPlayCardTargetFrame(permanent.PermanentFrame, false, activateClass))
                                    {
                                        candidatePermanents.Add(permanent);
                                    }
                                }

                                bool CanSelectPermanentCondition(Permanent permanent)
                                {
                                    if (permanent != null)
                                    {
                                        if (candidatePermanents.Contains(permanent))
                                        {
                                            return true;
                                        }
                                    }

                                    return false;
                                }

                                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                                {
                                    Permanent selectedPermanent = null;

                                    maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition));

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

                                    selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon that will Digivolve.", "The opponent is selecting 1 Digimon that will Digivolve.");

                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                                    IEnumerator SelectPermanentCoroutine(Permanent permanent)
                                    {
                                        selectedPermanent = permanent;

                                        yield return null;
                                    }

                                    if (selectedPermanent != null)
                                    {
                                        if (selectedCard != null)
                                        {
                                            if (selectedCard.CanPlayCardTargetFrame(selectedPermanent.PermanentFrame, false, activateClass))
                                            {
                                                digivolved = true;

                                                foreach (CardSource cardSource in revealLibrary.RevealedCards)
                                                {
                                                    if (!selectedCards.Contains(cardSource))
                                                    {
                                                        libraryCards.Add(cardSource);
                                                    }
                                                }

                                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ReturnRevealedCardsToLibraryBottom(libraryCards, activateClass));

                                                yield return ContinuousController.instance.StartCoroutine(new PlayCardClass(
                                                    cardSources: new List<CardSource>() { selectedCard },
                                                    hashtable: CardEffectCommons.CardEffectHashtable(activateClass),
                                                    payCost: false,
                                                    targetPermanent: selectedPermanent,
                                                    isTapped: false,
                                                    root: SelectCardEffect.Root.Library,
                                                    activateETB: true).PlayCard());
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (!digivolved)
                        {
                            maxCount = Math.Min(1, revealLibrary.RevealedCards.Count(CanSelectCardCondition1));

                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectCardCondition1,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => false,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: AfterSelectCardCoroutine,
                                message: "Select 1 card to add to your hand.",
                                maxCount: maxCount,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.AddHand,
                                root: SelectCardEffect.Root.Custom,
                                customRootCardList: revealLibrary.RevealedCards,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                            IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                            {
                                foreach (CardSource cardSource in revealLibrary.RevealedCards)
                                {
                                    if (!cardSources.Contains(cardSource))
                                    {
                                        libraryCards.Add(cardSource);
                                    }
                                }

                                yield return null;
                            }

                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ReturnRevealedCardsToLibraryBottom(libraryCards, activateClass));
                        }
                    }
                }
            }

            if (timing == EffectTiming.SecuritySkill)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect($"Play 1 Tamer from hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
                activateClass.SetIsSecurityEffect(true);
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Security] You may play 1 Tamer card from your hand without paying its memory cost.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource.IsTamer)
                    {
                        if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1)
                    {
                        List<CardSource> selectedCards = new List<CardSource>();

                        int maxCount = 1;

                        SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                        selectHandEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectCardCondition,
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

                        selectHandEffect.SetUpCustomMessage("Select 1 card to play.", "The opponent is selecting 1 card to play.");
                        selectHandEffect.SetUpCustomMessage_ShowCard("Played Card");

                        yield return StartCoroutine(selectHandEffect.Activate());

                        IEnumerator SelectCardCoroutine(CardSource cardSource)
                        {
                            selectedCards.Add(cardSource);

                            yield return null;
                        }

                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(cardSources: selectedCards, activateClass: activateClass, payCost: false, isTapped: false, root: SelectCardEffect.Root.Hand, activateETB: true));
                    }
                }
            }

            return cardEffects;
        }
    }
}