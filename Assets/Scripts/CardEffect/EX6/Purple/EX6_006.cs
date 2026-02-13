using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;


namespace DCGO.CardEffects.EX6
{
    public class EX6_006 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            #region Inherit
            ActivateClass activateClass2 = new ActivateClass();
            activateClass2.SetUpICardEffect("Reduce play cost by 3, if this Digimon has 5 or more different names in source, reduce by 4 instead.", CanUseCondition2, card);
            activateClass2.SetUpActivateClass(CanActivateCondition2, ActivateCoroutine2, 1, true, EffectDiscription2());
            activateClass2.SetHashString("Reduce_EX6_006");
            activateClass2.SetIsInheritedEffect(true);

            string EffectDiscription2()
            {
                return "[Breeding][Your Turn][Once Per Turn] When one of your Digimon with the [Seven Great Demon Lords] trait would be played, you may reduce the play cost by 3. If this Digimon has 5 or more cards with different names in its digivolution cards, reduce the cost by 4 instead.";
            }

            bool CardCondition(CardSource cardSource)
            {
                if (cardSource.Owner == card.Owner)
                {
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.CardTraits.Contains("Seven Great Demon Lords") || cardSource.CardTraits.Contains("SevenGreatDemonLords"))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool CanUseCondition2(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBreedingArea(card))
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        if (CardEffectCommons.CanTriggerWhenPermanentWouldPlay(hashtable, CardCondition))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool CanActivateCondition2(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBreedingArea(card))
                {
                    int count()
                    {
                        List<CardSource> DigivolutionCards = card.PermanentOfThisCard().DigivolutionCards;

                        bool end = false;

                        while (true)
                        {
                            if (DigivolutionCards.Count == 0)
                            {
                                end = true;
                            }

                            if (end)
                            {
                                break;
                            }

                            for (int i = 0; i < DigivolutionCards.Count; i++)
                            {
                                CardSource searchTargetCard = DigivolutionCards[i];

                                if (DigivolutionCards.Some(cardSource => cardSource != searchTargetCard && cardSource.HasSameCardName(searchTargetCard)))
                                {
                                    DigivolutionCards.Remove(searchTargetCard);
                                    break;
                                }

                                if (i == DigivolutionCards.Count - 1)
                                {
                                    end = true;
                                }
                            }
                        }
                        return DigivolutionCards.Count;
                    }


                    int reduceCount = 3;

                    if (count() >= 5)
                    {
                        reduceCount = 4;
                    }


                    if (reduceCount >= 1)
                    {
                        PlayCardClass playCardClass = CardEffectCommons.GetPlayCardClassFromHashtable(hashtable);

                        if (playCardClass != null)
                        {
                            if (playCardClass.PayCost)
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine2(Hashtable _hashtable)
            {
                if (CardEffectCommons.IsExistOnBreedingArea(card))
                {
                    int count()
                    {
                        List<CardSource> DigivolutionCards = card.PermanentOfThisCard().DigivolutionCards;

                        bool end = false;

                        while (true)
                        {
                            if (DigivolutionCards.Count == 0)
                            {
                                end = true;
                            }

                            if (end)
                            {
                                break;
                            }

                            for (int i = 0; i < DigivolutionCards.Count; i++)
                            {
                                CardSource searchTargetCard = DigivolutionCards[i];

                                if (DigivolutionCards.Some(cardSource => cardSource != searchTargetCard && cardSource.HasSameCardName(searchTargetCard)))
                                {
                                    DigivolutionCards.Remove(searchTargetCard);
                                    break;
                                }

                                if (i == DigivolutionCards.Count - 1)
                                {
                                    end = true;
                                }
                            }
                        }
                        return DigivolutionCards.Count;
                    }

                    int reduceCount = 3;

                    if(count() >= 5)
                    {
                        List<SelectionElement<bool>> selectionElements = new List<SelectionElement<bool>>
                        {
                            new(message: "4", value: true, spriteIndex: 0),
                            new(message: "3", value: false, spriteIndex: 1),
                        };

                        string selectPlayerMessage = "Which cost reduction do you want to use?";
                        string notSelectPlayerMessage = "The opponent is choosing which cost reduction to use.";

                        GManager.instance.userSelectionManager.SetBoolSelection(
                            selectionElements: selectionElements, selectPlayer: card.Owner,
                            selectPlayerMessage: selectPlayerMessage,
                            notSelectPlayerMessage: notSelectPlayerMessage);

                        yield return ContinuousController.instance.StartCoroutine(GManager.instance.userSelectionManager.WaitForEndSelect());

                        if (GManager.instance.userSelectionManager.SelectedBoolValue)
                            reduceCount = 4;
                    }


                    if (reduceCount >= 1)
                    {
                        ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                        ChangeCostClass changeCostClass = new ChangeCostClass();
                        changeCostClass.SetUpICardEffect($"Play Cost -{reduceCount}", CanUseCondition1, card);
                        changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => false, isChangePayingCost: () => true);
                        card.Owner.UntilCalculateFixedCostEffect.Add((_timing) => changeCostClass);

                        yield return new WaitForSeconds(0.4f);

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
                                        Cost -= reduceCount;
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
                            CardSource Card = CardEffectCommons.GetCardFromHashtable(_hashtable);

                            if (Card != null)
                            {
                                if (cardSource == Card)
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

            if (timing == EffectTiming.BeforePayCost)
            {
                cardEffects.Add(activateClass2);
            }

            if (timing == EffectTiming.None)
            {
                ChangeCostClass changeCostClass = new ChangeCostClass();
                changeCostClass.SetUpICardEffect("Play Cost -", CanUseCondition, card);
                changeCostClass.SetUpChangeCostClass(changeCostFunc: ChangeCost, cardSourceCondition: CardSourceCondition, rootCondition: RootCondition, isUpDown: isUpDown, isCheckAvailability: () => true, isChangePayingCost: () => true);
                changeCostClass.SetNotShowUI(true);
                changeCostClass.SetIsInheritedEffect(true);
                cardEffects.Add(changeCostClass);

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (!card.Owner.isYou && GManager.instance.IsAI)
                    {
                        return false;
                    }

                    if (CardEffectCommons.IsExistOnBreedingArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            int count()
                            {
                                List<CardSource> DigivolutionCards = card.PermanentOfThisCard().DigivolutionCards;

                                bool end = false;

                                while (true)
                                {
                                    if (DigivolutionCards.Count == 0)
                                    {
                                        end = true;
                                    }

                                    if (end)
                                    {
                                        break;
                                    }

                                    for (int i = 0; i < DigivolutionCards.Count; i++)
                                    {
                                        CardSource searchTargetCard = DigivolutionCards[i];

                                        if (DigivolutionCards.Some(cardSource => cardSource != searchTargetCard && cardSource.HasSameCardName(searchTargetCard)))
                                        {
                                            DigivolutionCards.Remove(searchTargetCard);
                                            break;
                                        }

                                        if (i == DigivolutionCards.Count - 1)
                                        {
                                            end = true;
                                        }
                                    }
                                }
                                return DigivolutionCards.Count;
                            }

                            int reduceCount = 3;

                            if (count() >= 5)
                            {
                                reduceCount = 4;
                            }



                            if (reduceCount >= 1)
                            {
                                if (activateClass2 != null)
                                {
                                    if (!card.cEntity_EffectController.isOverMaxCountPerTurn(activateClass2, activateClass2.MaxCountPerTurn))
                                    {
                                        return true;
                                    }
                                }
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
                                int count()
                                {
                                    List<CardSource> DigivolutionCards = card.PermanentOfThisCard().DigivolutionCards;

                                    bool end = false;

                                    while (true)
                                    {
                                        if (DigivolutionCards.Count == 0)
                                        {
                                            end = true;
                                        }

                                        if (end)
                                        {
                                            break;
                                        }

                                        for (int i = 0; i < DigivolutionCards.Count; i++)
                                        {
                                            CardSource searchTargetCard = DigivolutionCards[i];

                                            if (DigivolutionCards.Some(cardSource => cardSource != searchTargetCard && cardSource.HasSameCardName(searchTargetCard)))
                                            {
                                                DigivolutionCards.Remove(searchTargetCard);
                                                break;
                                            }

                                            if (i == DigivolutionCards.Count - 1)
                                            {
                                                end = true;
                                            }
                                        }
                                    }
                                    return DigivolutionCards.Count;
                                }

                                int reduceCount = 3;

                                if (count() >= 5)
                                {
                                    reduceCount = 4;
                                }

                                Cost -= reduceCount;
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
                    if (cardSource.IsDigimon)
                    {
                        if (cardSource.CardTraits.Contains("Seven Great Demon Lords") || cardSource.CardTraits.Contains("SevenGreatDemonLords"))
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
            #endregion

            #region Start of Main
            if (timing == EffectTiming.OnStartMainPhase)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Place the top card of your Digi-Egg deck as the bottom source of this Digimon and activate effects", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[Breeding][Start of Your Main Phase] Place the top card of your Digi-Egg deck as this Digimon's bottom digivolution card and delete all of your Digimon. If this effect deleted, place 1 card with the [Seven Great Demon Lords] trait from your trash as this Digimon's bottom digivolution card.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                { 
                    if (cardSource.CardTraits.Contains("Seven Great Demon Lords") || cardSource.CardTraits.Contains("SevenGreatDemonLords"))
                    {
                        if (!cardSource.CanNotBeAffected(activateClass))
                        {
                            if (!cardSource.IsToken)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBreedingArea(card))
                    {
                        if (CardEffectCommons.IsOwnerTurn(card))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBreedingArea(card))
                    {
                        return true;
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    List<CardSource> digivolutionCards = new List<CardSource>();

                    List<CardSource> sevenGreatDemonLordFromTrash = new List<CardSource>();

                    if (CardEffectCommons.IsExistOnBreedingArea(card))
                    {
                        CardSource topCard = null;

                        if (card.Owner.DigitamaLibraryCards.Count >= 1)
                        {
                            topCard = card.Owner.DigitamaLibraryCards[0];

                            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(new List<CardSource>() { topCard }, "Revealed Card", true, true));
                        }

                        if (topCard != null)
                        {
                            digivolutionCards.Add(topCard);

                            yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(digivolutionCards, activateClass));
                        }

                        List<Permanent> destroyedPermanetns = new List<Permanent>();


                        foreach (Permanent permanent in card.Owner.GetBattleAreaDigimons())
                        {
                            if (permanent.IsDigimon)
                            {
                                if (!permanent.TopCard.CanNotBeAffected(activateClass))
                                {
                                    if (permanent.CanBeDestroyedBySkill(activateClass))
                                    {
                                        destroyedPermanetns.Add(permanent);
                                    }
                                }
                            }
                        }
                        

                        if (destroyedPermanetns.Count >= 1)
                        {
                            yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DeletePeremanentAndProcessAccordingToResult(destroyedPermanetns,activateClass, successProcess: permanents => SuccessProcess(), failureProcess: null));
                        }

                        IEnumerator SuccessProcess()
                        {
                            int maxCount = 1;

                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();
                            selectCardEffect.SetUp(
                                canTargetCondition: CanSelectCardCondition,
                                canTargetCondition_ByPreSelecetedList: null,
                                canEndSelectCondition: null,
                                canNoSelect: () => false,
                                selectCardCoroutine: SelectCardCoroutine,
                                afterSelectCardCoroutine: null,
                                message: "Select 1 card to place on bottom of digivolution cards.",
                                maxCount: maxCount,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                mode: SelectCardEffect.Mode.Custom,
                                root: SelectCardEffect.Root.Trash,
                                customRootCardList: null,
                                canLookReverseCard: true,
                                selectPlayer: card.Owner,
                                cardEffect: activateClass);

                            selectCardEffect.SetUpCustomMessage_ShowCard("Digivolution Card");
                            selectCardEffect.SetUpCustomMessage("Select 1 card to place on bottom of digivolution cards.", "The opponent is selecting 1 card to place on bottom of digivolution cards.");

                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                            IEnumerator SelectCardCoroutine(CardSource cardSource)
                            {
                                sevenGreatDemonLordFromTrash.Add(cardSource);

                                yield return null;
                            }
                        }

                        yield return ContinuousController.instance.StartCoroutine(card.PermanentOfThisCard().AddDigivolutionCardsBottom(sevenGreatDemonLordFromTrash, activateClass));
                    }
                }
            }
            #endregion

            #region End of Opponent's Turn
            if (timing == EffectTiming.OnEndTurn)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Delete this Digimon with 7 or more different names to play [Ogudomon] from your trash.", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
                cardEffects.Add(activateClass);

                int count()
                {
                    List<CardSource> DigivolutionCards = card.PermanentOfThisCard().DigivolutionCards;

                    bool end = false;

                    while (true)
                    {
                        if (DigivolutionCards.Count == 0)
                        {
                            end = true;
                        }

                        if (end)
                        {
                            break;
                        }

                        for (int i = 0; i < DigivolutionCards.Count; i++)
                        {
                            CardSource searchTargetCard = DigivolutionCards[i];

                            if (DigivolutionCards.Some(cardSource => cardSource != searchTargetCard && cardSource.HasSameCardName(searchTargetCard)))
                            {
                                DigivolutionCards.Remove(searchTargetCard);
                                break;
                            }

                            if (i == DigivolutionCards.Count - 1)
                            {
                                end = true;
                            }
                        }
                    }
                    return DigivolutionCards.Count;
                }

                string EffectDiscription()
                {
                    return "[Breeding] [End of Opponent's Turn] By deleting this Digimon with 7 or more cards with different names in it's digivolution cards, you may play 1 [Ogudomon] from your trash without paying the cost";
                }

                bool CanSelectPermanentCondition(Permanent permanent)
                {
                    if (permanent == card.PermanentOfThisCard())
                    {
                        return true;
                    }
                    return false;
                }

                bool CanSelectOgudomonInTrash(CardSource cardSource)
                {
                    if (cardSource.CardNames.Contains("Ogudomon"))
                    {
                        return true;
                    }
                    return false;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBreedingAreaDigimon(card))  
                    {
                        if (CardEffectCommons.IsOpponentTurn(card))
                        {
                            if (count() >= 7)
                            {
                                    return true;
                            }
                        }
                    }
                    return false;
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBreedingArea(card))
                    {
                        if (CardEffectCommons.IsExistOnBreedingAreaDigimon(card))
                        {
                            if (CardEffectCommons.IsOpponentTurn(card))
                            {
                                if (count() >= 7)
                                {
                                    return true;
                                }
                            }
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBreedingAreaDigimon(card))
                    {
                        List<Permanent> DigitamaPermanents = card.Owner.GetBreedingAreaPermanents()
                                            .Filter(permanent => permanent.IsDigimon)
                                            .Filter(permanent => !permanent.TopCard.CanNotBeAffected(activateClass))
                                            .Clone();

                        if (DigitamaPermanents.Count >= 1)
                        {
                            foreach (Permanent permanent in DigitamaPermanents)
                            {
                                yield return ContinuousController.instance.StartCoroutine(permanent.DiscardEvoRoots());

                                CardSource cardSource = permanent.TopCard;

                                ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(new List<CardSource>() { cardSource }, "Cards put to trash", true, true));

                                yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveField(permanent));
                                yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddTrashCard(cardSource));
                            }
                        }

                        int maxCount = Math.Min(1, card.Owner.TrashCards.Count((cardSource) => CanSelectOgudomonInTrash(cardSource)));

                        List<CardSource> selectedCards = new List<CardSource>();

                        SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                        selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectOgudomonInTrash,
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
            #endregion

            return cardEffects;
        }
    }
}