using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT12
{
    public class BT12_031 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.None)
            {
                bool PermanentCondition(Permanent targetPermanent)
                {
                    return targetPermanent.TopCard.ContainsCardName("Imperialdramon: Dragon Mode") || targetPermanent.TopCard.ContainsCardName("Imperialdramon:DragonMode") ||
                        targetPermanent.TopCard.ContainsCardName("Imperialdramon Dragon Mode") || targetPermanent.TopCard.ContainsCardName("ImperialdramonDragonMode");
                }

                cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 2, ignoreDigivolutionRequirement: false, card: card, condition: null));
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Suspend Digimon without digivolution cards and return suspended Digimon to the bottom of deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[When Digivolving] Suspend all of your opponent's Digimon with no digivolution cards. Then, return 1 of your opponent's suspended Digimon to its owner's hand. By returning 1 [Imperialdramon: Dragon Mode] card from this Digimon's digivolution cards to its owner's hand, place all of your opponent's suspended Digimon at the bottom of their owners' decks instead.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.HasNoDigivolutionCards)
                        {
                            if (!permanent.TopCard.CanNotBeAffected(activateClass))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                        if (cardSource.Owner == card.Owner)
                        {
                            if (cardSource.CardNames.Contains("Imperialdramon: Dragon Mode") || cardSource.CardNames.Contains("Imperialdramon:DragonMode") ||
                                cardSource.CardNames.Contains("Imperialdramon Dragon Mode") || cardSource.CardNames.Contains("ImperialdramonDragonMode"))
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanSelectPermanentCondition1(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.IsSuspended)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanSelectPermanentCondition2(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card))
                    {
                        if (permanent.IsSuspended)
                        {
                            if (!permanent.TopCard.CanNotBeAffected(activateClass))
                            {
                                if (!permanent.CannotReturnToLibrary(activateClass))
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
                    return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
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
                    if (isExistOnField(card))
                    {
                        if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                        {
                            Hashtable hashtable = new Hashtable();
                            hashtable.Add("CardEffect", activateClass);

                            if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                            {
                                List<Permanent> tappedPermanents = new List<Permanent>();

                                foreach (Permanent permanent in card.Owner.Enemy.GetBattleAreaDigimons())
                                {
                                    if (CanSelectPermanentCondition(permanent))
                                    {
                                        tappedPermanents.Add(permanent);
                                    }
                                }

                                yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(tappedPermanents, hashtable).Tap());
                            }

                            bool returned = false;

                            if (isExistOnField(card))
                            {
                                Permanent selectedPermanent = card.PermanentOfThisCard();

                                if (selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition) >= 1)
                                {
                                    int maxCount = Math.Min(1, selectedPermanent.DigivolutionCards.Count(CanSelectCardCondition));

                                    SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                    selectCardEffect.SetUp(
                                                canTargetCondition: CanSelectCardCondition,
                                                canTargetCondition_ByPreSelecetedList: null,
                                                canEndSelectCondition: null,
                                                canNoSelect: () => true,
                                                selectCardCoroutine: null,
                                                afterSelectCardCoroutine: AfterSelectCardCoroutine,
                                                message: "Select 1 card to add to your hand.",
                                                maxCount: maxCount,
                                                canEndNotMax: false,
                                                isShowOpponent: true,
                                                mode: SelectCardEffect.Mode.AddHand,
                                                root: SelectCardEffect.Root.Custom,
                                                customRootCardList: selectedPermanent.DigivolutionCards,
                                                canLookReverseCard: true,
                                                selectPlayer: card.Owner,
                                                cardEffect: activateClass);

                                    yield return StartCoroutine(selectCardEffect.Activate());

                                    IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                                    {
                                        if (cardSources.Count((cardSource) => cardSource.Owner.HandCards.Contains(cardSource)) >= 1)
                                        {
                                            returned = true;
                                        }

                                        yield return null;
                                    }
                                }
                            }

                            if (!returned)
                            {
                                if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition1))
                                {
                                    int maxCount = Math.Min(1, CardEffectCommons.MatchConditionPermanentCount(CanSelectPermanentCondition1));

                                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                                    selectPermanentEffect.SetUp(
                                        selectPlayer: card.Owner,
                                        canTargetCondition: CanSelectPermanentCondition1,
                                        canTargetCondition_ByPreSelecetedList: null,
                                        canEndSelectCondition: null,
                                        maxCount: maxCount,
                                        canNoSelect: false,
                                        canEndNotMax: false,
                                        selectPermanentCoroutine: null,
                                        afterSelectPermanentCoroutine: null,
                                        mode: SelectPermanentEffect.Mode.Bounce,
                                        cardEffect: activateClass);

                                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());
                                }
                            }
                            else
                            {
                                List<Permanent> selectedPermanents = new List<Permanent>();

                                foreach (Permanent permanent in card.Owner.Enemy.GetBattleAreaDigimons())
                                {
                                    if (CanSelectPermanentCondition2(permanent))
                                    {
                                        selectedPermanents.Add(permanent);
                                    }
                                }

                                if (selectedPermanents.Count >= 1)
                                {
                                    if (selectedPermanents.Count >= 1)
                                    {
                                        if (selectedPermanents.Count == 1)
                                        {
                                            yield return ContinuousController.instance.StartCoroutine(new DeckBottomBounceClass(selectedPermanents, hashtable).DeckBounce());
                                        }
                                        else
                                        {
                                            List<CardSource> cardSources = new List<CardSource>();

                                            foreach (Permanent permanent in selectedPermanents)
                                            {
                                                cardSources.Add(permanent.TopCard);
                                            }

                                            List<SkillInfo> skillInfos = new List<SkillInfo>();

                                            foreach (CardSource cardSource in cardSources)
                                            {
                                                ICardEffect cardEffect = new ChangeBaseDPClass();
                                                cardEffect.SetUpICardEffect(" ", null, cardSource);

                                                skillInfos.Add(new SkillInfo(cardEffect, null, EffectTiming.None));
                                            }

                                            List<CardSource> selectedCards = new List<CardSource>();

                                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                            selectCardEffect.SetUp(
                                                canTargetCondition: (cardSource) => true,
                                                canTargetCondition_ByPreSelecetedList: null,
                                                canEndSelectCondition: null,
                                                canNoSelect: () => false,
                                                selectCardCoroutine: null,
                                                afterSelectCardCoroutine: AfterSelectCardCoroutine1,
                                                message: "Specify the order to place the card at the bottom of the deck\n(cards will be placed back to the bottom of the deck so that cards with lower numbers are on top).",
                                                maxCount: cardSources.Count,
                                                canEndNotMax: false,
                                                isShowOpponent: false,
                                                mode: SelectCardEffect.Mode.Custom,
                                                root: SelectCardEffect.Root.Custom,
                                                customRootCardList: cardSources,
                                                canLookReverseCard: true,
                                                selectPlayer: card.Owner,
                                                cardEffect: activateClass);

                                            selectCardEffect.SetNotShowCard();
                                            selectCardEffect.SetNotAddLog();
                                            selectCardEffect.SetUpSkillInfos(skillInfos);

                                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                                            IEnumerator AfterSelectCardCoroutine1(List<CardSource> cardSources)
                                            {
                                                if (cardSources.Count >= 1)
                                                {
                                                    foreach (CardSource cardSource in cardSources)
                                                    {
                                                        selectedCards.Add(cardSource);
                                                    }

                                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(cardSources, "Deck Bottom Cards", true, true));
                                                }
                                            }

                                            if (selectedCards.Count >= 1)
                                            {
                                                List<Permanent> libraryPermanets = new List<Permanent>();

                                                foreach (CardSource cardSource in selectedCards)
                                                {
                                                    libraryPermanets.Add(cardSource.PermanentOfThisCard());
                                                }

                                                if (libraryPermanets.Count >= 1)
                                                {
                                                    DeckBottomBounceClass putLibraryBottomPermanent = new DeckBottomBounceClass(libraryPermanets, hashtable);

                                                    putLibraryBottomPermanent.SetNotShowCards();

                                                    yield return ContinuousController.instance.StartCoroutine(putLibraryBottomPermanent.DeckBounce());
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (timing == EffectTiming.None)
            {
                int count()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        return card.PermanentOfThisCard().DigivolutionCardsColors.Count;
                    }

                    return 0;
                }

                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (count() >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect<Func<int>>(changeValue: () => 1000 * count(), isInheritedEffect: false, card: card, condition: Condition));
            }

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().DigivolutionCardsColors.Count >= 2)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.ChangeSelfSAttackStaticEffect(changeValue: 1, isInheritedEffect: false, card: card, condition: Condition));
            }

            if (timing == EffectTiming.None)
            {
                bool Condition()
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.PermanentOfThisCard().DigivolutionCardsColors.Count >= 2)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: Condition));
            }

            return cardEffects;
        }
    }
}