using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Shoutmon X7: Superior Mode
namespace DCGO.CardEffects.BT12
{
    public class BT12_112 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region When Would Be Played
            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play Cost -1 and select trash cards for a DigiXros", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDescription());
                activateClass.SetHashString("BT12_112_WBP");
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "When this card would be played, by placing 1 of your [Shoutmon] as this Digimon's bottom digivolution card, reduce the play cost by 1 and cards in your trash can also be placed for its DigiXros.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card)
                        && permanent.TopCard.EqualsCardName("Shoutmon")
                        && permanent.CanSelectBySkill(activateClass)
                        && !permanent.TopCard.CanNotBeAffected(activateClass)
                        && !permanent.IsToken;
                }

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, CardCondition);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                    selectPermanentEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: CanSelectPermanentCondition,
                        canTargetCondition_ByPreSelecetedList: null,
                        canEndSelectCondition: null,
                        maxCount: 1,
                        canNoSelect: true,
                        canEndNotMax: false,
                        selectPermanentCoroutine: null,
                        afterSelectPermanentCoroutine: AfterSelectPermanentCoroutine,
                        mode: SelectPermanentEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectPermanentEffect.SetUpCustomMessage("Select 1 [Shoutmon].", "The opponent is selecting 1 [Shoutmon].");

                    yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                    IEnumerator AfterSelectPermanentCoroutine(List<Permanent> permanents)
                    {
                        yield return null;

                        if (permanents.Count >= 1)
                        {
                            Permanent selectedPermanent = permanents[0];

                            if (selectedPermanent != null
                            && selectedPermanent.TopCard != null
                            && !selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                            {
                                // Code to trash selectedPermanent's digivolution sources without triggering effects similar to by rules

                                GManager.instance.GetComponent<SelectDigiXrosClass>().AddDigivolutionCardInfos(new AddDigivolutionCardsInfo(activateClass, new List<CardSource>() { selectedPermanent.TopCard }));

                                if (card.Owner.CanReduceCost(null, card))
                                {
                                    ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);
                                }

                                ChangeCostClass changeCostClass = new ChangeCostClass();
                                changeCostClass.SetUpICardEffect("Play Cost -1", CanUseCondition1, card);
                                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                                card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ShowReducedCost(_hashtable));

                                bool CanUseCondition1(Hashtable hashtable)
                                {
                                    return true;
                                }

                                int ChangeCost(CardSource cardSource, int Cost, SelectCardEffect.Root root, List<Permanent> targetPermanents)
                                {
                                    if (CardSourceCondition(cardSource)
                                    && RootCondition(root)
                                    && PermanentsCondition(targetPermanents))
                                    {
                                        Cost -= 1;
                                    }
                                     
                                    return Cost;
                                }

                                bool PermanentsCondition(List<Permanent> targetPermanents)
                                {
                                    return targetPermanents == null
                                            || targetPermanents.Count((targetPermanent) => targetPermanent != null) == 0;
                                }

                                bool CardSourceCondition(CardSource cardSource)
                                {
                                    return cardSource == card;
                                }

                                bool RootCondition(SelectCardEffect.Root root)
                                {
                                    return true;
                                }

                                bool isUpDown()
                                {
                                    return true;
                                }

                                #region can select digixros cards from trash
                                AddMaxTrashCountDigiXrosClass addMaxTrashCountDigiXrosClass = new AddMaxTrashCountDigiXrosClass();
                                addMaxTrashCountDigiXrosClass.SetUpICardEffect("Can select DigiXros cards from trash", CanUseCondition1, card);
                                addMaxTrashCountDigiXrosClass.SetUpAddMaxTrashCountDigiXrosClass(getMaxTrashCount: GetCount);
                                Func<EffectTiming, ICardEffect> getCardEffect = GetCardEffect;
                                card.Owner.UntilCalculateFixedCostEffect.Add(getCardEffect);

                                ICardEffect GetCardEffect(EffectTiming _timing)
                                {
                                    if (_timing == EffectTiming.None)
                                    {
                                        return addMaxTrashCountDigiXrosClass;
                                    }

                                    return null;
                                }

                                int GetCount(CardSource cardSource)
                                {
                                    return 100;
                                }
                                #endregion
                            }
                        }
                    }
                }
            }
            #endregion

            #region On Play
            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return all digivolution cards of 1 Digimon and the Digimon to the bottom of deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDescription());
                cardEffects.Add(activateClass);

                string EffectDescription()
                {
                    return "[On Play] Return all of the digivolution cards of 1 of your opponent's Digimon to the bottom of the deck, and return that Digimon to the bottom of the deck.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaTrigger(card, activateClass)
                        && CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.IsExistOnBattleAreaActivate(card, activateClass);
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        Permanent selectedPermanent = null;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: 1,
                            canNoSelect: false,
                            canEndNotMax: false,
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to put it's sources on bottom of the deck and then that Digimon.", "The opponent is selecting 1 Digimon to put it's sources on bottom of the deck and then that Digimon.");
                        yield return ContinuousController.instance.StartCoroutine(selectPermanentEffect.Activate());

                        IEnumerator SelectPermanentCoroutine(Permanent permanent)
                        {
                            selectedPermanent = permanent;
                            yield return null;
                        }

                        if (selectedPermanent != null)
                        {
                            List<CardSource> libraryCards = new List<CardSource>();

                            if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                            {
                                if (selectedPermanent.DigivolutionCards.Count >= 1)
                                {
                                    if (selectedPermanent.DigivolutionCards.Count == 1)
                                    {
                                        libraryCards = selectedPermanent.DigivolutionCards.Clone();
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
                                            message: "Specify the order to place the card at the bottom of the deck\n(cards will be placed back to the bottom of the deck so that cards with lower numbers are on top).",
                                            maxCount: selectedPermanent.DigivolutionCards.Count,
                                            canEndNotMax: false,
                                            isShowOpponent: false,
                                            mode: SelectCardEffect.Mode.Custom,
                                            root: SelectCardEffect.Root.Custom,
                                            customRootCardList: selectedPermanent.DigivolutionCards,
                                            canLookReverseCard: true,
                                            selectPlayer: card.Owner,
                                            cardEffect: activateClass);

                                        selectCardEffect.SetNotShowCard();
                                        selectCardEffect.SetNotAddLog();
                                        selectCardEffect.SetUseFaceDown();

                                        yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                                        IEnumerator AfterSelectCardCoroutine1(List<CardSource> cardSources)
                                        {
                                            libraryCards = cardSources.Clone();

                                            yield return null;
                                        }
                                    }

                                    if (libraryCards.Count >= 1)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(libraryCards, cardEffect: activateClass));

                                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect2(libraryCards, "Deck Bottom Cards", true, true));
                                    }
                                }

                                if (selectedPermanent.TopCard != null)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(new DeckBottomBounceClass(new List<Permanent>() { selectedPermanent }, CardEffectCommons.CardEffectHashtable(activateClass)).DeckBounce());
                                }
                            }                            
                        }
                    }
                }
            }
            #endregion

            #region Your Turn
            if (timing == EffectTiming.None)
            {
                DisableEffectClass invalidationClass = new DisableEffectClass();
                invalidationClass.SetUpICardEffect("Ignore Security Effect", CanUseCondition, card);
                invalidationClass.SetUpDisableEffectClass(DisableCondition: InvalidateCondition);

                cardEffects.Add(invalidationClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                bool InvalidateCondition(ICardEffect cardEffect)
                {
                    return isExistOnField(card)
                        && card.Owner.GetBattleAreaPermanents().Contains(card.PermanentOfThisCard())
                        && CardEffectCommons.IsOwnerTurn(card)
                        && cardEffect != null
                        && cardEffect.EffectSourceCard != null
                        && cardEffect.EffectSourceCard.IsOption
                        && cardEffect.IsSecurityEffect;
                }
            }
            #endregion

            #region DigiXros
            if (timing == EffectTiming.None)
            {
                AddDigiXrosConditionClass addDigiXrosConditionClass = new AddDigiXrosConditionClass();
                addDigiXrosConditionClass.SetUpICardEffect($"DigiXros", CanUseCondition, card);
                addDigiXrosConditionClass.SetUpAddDigiXrosConditionClass(getDigiXrosCondition: GetDigiXros);
                addDigiXrosConditionClass.SetNotShowUI(true);
                cardEffects.Add(addDigiXrosConditionClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    return true;
                }

                DigiXrosCondition GetDigiXros(CardSource cardSource)
                {
                    if (cardSource == card)
                    {
                        DigiXrosConditionElement element = new DigiXrosConditionElement(CanSelectCardCondition, "1 Digimon card with [Xros Heart] or [Blue Flare] trait", true);

                        bool CanSelectCardCondition(CardSource cardSource)
                        {
                            return cardSource != null
                                && cardSource.Owner == card.Owner
                                && cardSource.IsDigimon
                                && (cardSource.CardTraits.Contains("Xros Heart")
                                    || cardSource.CardTraits.Contains("Blue Flare"));
                        }

                        List<DigiXrosConditionElement> elements = new List<DigiXrosConditionElement>();

                        for (int i = 0; i < 50; i++)
                        {
                            elements.Add(element);
                        }

                        bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                        {
                            List<string> cardIDs = new List<string>();

                            foreach (CardSource cardSource1 in cardSources)
                            {
                                if (!cardIDs.Contains(cardSource1.CardID))
                                {
                                    cardIDs.Add(cardSource1.CardID);
                                }
                            }

                            if (cardIDs.Contains(cardSource.CardID))
                            {
                                return false;
                            }

                            return true;
                        }

                        DigiXrosCondition digiXrosCondition = new DigiXrosCondition(elements, CanTargetCondition_ByPreSelecetedList, 1);

                        return digiXrosCondition;
                    }

                    return null;
                }
            }
            #endregion

            return cardEffects;
        }
    }
}
