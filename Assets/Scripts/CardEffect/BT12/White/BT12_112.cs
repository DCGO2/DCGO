using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DCGO.CardEffects.BT12
{
    public class BT12_112 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.BeforePayCost)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Play Cost -1 and select trash cards for a DigiXros", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                activateClass.SetHashString("PlayCost-1_BT12_112");
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "When you would play this card from your hand, by placing 1 of your [Shoutmon] as a digivolution card under this Digimon, reduce its play cost by 1 and place the cards in your trash as digivolution cards for a DigiXros.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                    {
                        if (permanent.TopCard.CardNames.Contains("Shoutmon"))
                        {
                            if (permanent.CanSelectBySkill(activateClass))
                            {
                                if (!permanent.TopCard.CanNotBeAffected(activateClass))
                                {
                                    if (!permanent.IsToken)
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }

                    return false;
                }

                bool CardCondition(CardSource cardSource)
                {
                    return cardSource == card;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, CardCondition))
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
                        int maxCount = 1;

                        SelectPermanentEffect selectPermanentEffect = GManager.instance.GetComponent<SelectPermanentEffect>();

                        selectPermanentEffect.SetUp(
                            selectPlayer: card.Owner,
                            canTargetCondition: CanSelectPermanentCondition,
                            canTargetCondition_ByPreSelecetedList: null,
                            canEndSelectCondition: null,
                            maxCount: maxCount,
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

                                if (selectedPermanent != null)
                                {
                                    if (selectedPermanent.TopCard != null)
                                    {
                                        if (!selectedPermanent.TopCard.CanNotBeAffected(activateClass))
                                        {
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
                                                if (CardSourceCondition(cardSource))
                                                {
                                                    if (RootCondition(root))
                                                    {
                                                        if (PermanentsCondition(targetPermanents))
                                                        {
                                                            Cost -= 1;
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
                    }
                }
            }

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Return all digivolution cards of 1 Digimon and the Digimon to the bottom of deck", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Return all of the digivolution cards under 1 of your opponent's Digimon to their owner's deck in any order, and return that Digimon to the bottom of its owner's deck.";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnOpponentBattleAreaDigimon(permanent, card);
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (CardEffectCommons.HasMatchConditionPermanent(CanSelectPermanentCondition))
                    {
                        Permanent selectedPermanent = null;

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
                            selectPermanentCoroutine: SelectPermanentCoroutine,
                            afterSelectPermanentCoroutine: null,
                            mode: SelectPermanentEffect.Mode.Custom,
                            cardEffect: activateClass);

                        selectPermanentEffect.SetUpCustomMessage("Select 1 Digimon to put on bottom of the deck.", "The opponent is selecting 1 Digimon to put on bottom of the deck.");
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
                                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddLibraryBottomCards(libraryCards));

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
                    if (isExistOnField(card))
                    {
                        if (card.Owner.GetBattleAreaPermanents().Contains(card.PermanentOfThisCard()))
                        {
                            if (CardEffectCommons.IsOwnerTurn(card))
                            {
                                if (cardEffect != null)
                                {
                                    if (cardEffect.EffectSourceCard != null)
                                    {
                                        if (cardEffect.EffectSourceCard.IsOption)
                                        {
                                            if (cardEffect.IsSecurityEffect)
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    return false;
                }
            }

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
                        DigiXrosConditionElement element = new DigiXrosConditionElement(CanSelectCardCondition, "1 Digimon card with [Xros Heart] or [Blue Flare] trait");

                        bool CanSelectCardCondition(CardSource cardSource)
                        {
                            if (cardSource != null)
                            {
                                if (cardSource.Owner == card.Owner)
                                {
                                    if (cardSource.IsDigimon)
                                    {
                                        if (cardSource.CardTraits.Contains("Xros Heart"))
                                        {
                                            return true;
                                        }

                                        if (cardSource.CardTraits.Contains("XrosHeart"))
                                        {
                                            return true;
                                        }

                                        if (cardSource.CardTraits.Contains("Blue Flare"))
                                        {
                                            return true;
                                        }

                                        if (cardSource.CardTraits.Contains("BlueFlare"))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }

                            return false;
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

            return cardEffects;
        }
    }
}