using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DCGO.CardEffects.EX6
{
    public class Gate_of_Deadly_Sins_EX6_006 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

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
                                canNoSelect: () => true,
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

            return cardEffects;
        }
    }
}