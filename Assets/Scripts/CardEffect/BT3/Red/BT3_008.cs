using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT3_008 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Reveal the top 5 cards of deck", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[On Play] Reveal the top 5 cards of your deck. Add 1 [RagnaLoardmon] and 1 Digimon card with [Legend-Arms] in its traits among them to your hand. Place the remaining cards at the bottom of your deck in any order.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                if (cardSource != null)
                {
                    if (cardSource.Owner == card.Owner)
                    {
                        if (cardSource.CardNames.Contains("RagnaLoardmon"))
                        {
                            return true;
                        }
                    }

                }

                return false;
            }

            bool CanSelectCardCondition1(CardSource cardSource)
            {
                if (cardSource != null)
                {
                    if (cardSource.Owner == card.Owner)
                    {
                        if (cardSource.IsDigimon)
                        {
                            if (cardSource.CardTraits.Contains("Legend-Arms"))
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
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.SimplifiedRevealDeckTopCardsAndSelect(
                    revealCount: 5,
                    simplifiedSelectCardConditions:
                    new SimplifiedSelectCardConditionClass[]
                    {
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition,
                            message: "Select 1 [RagnaLoardmon].",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                        new SimplifiedSelectCardConditionClass(
                            canTargetCondition:CanSelectCardCondition1,
                            message:  "Select 1 Digimon card with [Legend-Arms] in its traits.",
                            mode: SelectCardEffect.Mode.AddHand,
                            maxCount: 1,
                            selectCardCoroutine: null),
                    },
                    remainingCardsPlace: RemainingCardsPlace.DeckBottom,
                    activateClass: activateClass
                ));

                #region old
                /*
                                        if (card.Owner.LibraryCards.Count >= 1)
                                        {
                                            IRevealLibrary revealLibrary = new IRevealLibrary(card.Owner, 5);

                                            yield return ContinuousController.instance.StartCoroutine(revealLibrary.RevealLibrary());

                                            List<CardSource> libraryCards = new List<CardSource>();

                                            int maxCount = 2;

                                            #region 最大枚数
                                            List<CardSource[]> cardsList = ParameterComparer.Enumerate(revealLibrary.revealedCards, maxCount).ToList();

                                            List<int> maxCounts = new List<int>() { 0 };

                                            foreach(CardSource[] cardSources in cardsList)
                                            {
                                                List<int> _maxCounts = new List<int>();

                                                if(cardSources.Length >= 2)
                                                {
                                                    if(CanSelectCardCondition(cardSources[0]))
                                                    {
                                                        if (CanSelectCardCondition1(cardSources[1]))
                                                        {
                                                            _maxCounts.Add(2);
                                                        }

                                                        else
                                                        {
                                                            _maxCounts.Add(1);
                                                        }
                                                    }

                                                    if (CanSelectCardCondition1(cardSources[0]))
                                                    {
                                                        if (CanSelectCardCondition(cardSources[1]))
                                                        {
                                                            _maxCounts.Add(2);
                                                        }

                                                        else
                                                        {
                                                            _maxCounts.Add(1);
                                                        }
                                                    }

                                                    if (!CanSelectCardCondition(cardSources[0]) && !CanSelectCardCondition1(cardSources[0]))
                                                    {
                                                        if (CanSelectCardCondition(cardSources[1]))
                                                        {
                                                            _maxCounts.Add(1);
                                                        }

                                                        if (CanSelectCardCondition1(cardSources[1]))
                                                        {
                                                            _maxCounts.Add(1);
                                                        }
                                                    }
                                                }

                                                else if(cardSources.Length == 1)
                                                {
                                                    if (CanSelectCardCondition(cardSources[0]))
                                                    {
                                                        _maxCounts.Add(1);
                                                    }

                                                    else if (CanSelectCardCondition1(cardSources[0]))
                                                    {
                                                        _maxCounts.Add(1);
                                                    }
                                                }

                                                if(_maxCounts.Count >= 1)
                                                {
                                                    maxCounts.Add(_maxCounts.Max());
                                                }
                                            }

                                            if(maxCounts.Count >= 1)
                                            {
                                                maxCount = maxCounts.Max();
                                            }
                                            #endregion

                                            SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                            selectCardEffect.SetUp(
                                                CanTargetCondition: (cardSource) => CanSelectCardCondition(cardSource) || CanSelectCardCondition1(cardSource),
                                                CanTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                                                CanEndSelectCondition: CanEndSelectCondition,
                                                CanNoSelect: () => false,
                                                SelectCardCoroutine: null,
                                                AfterSelectCardCoroutine: AfterSelectCardCoroutine,
                                                Message: "Select cards to add to your hand.",
                                                MaxCount: maxCount,
                                                CanEndNotMax: false,
                                                isShowOpponent: true,
                                                mode: SelectCardEffect.Mode.AddHand,
                                                root: SelectCardEffect.Root.Custom,
                                                CustomRootCardList: revealLibrary.revealedCards,
                                                CanLookReverseCard: true,
                                                SelectPlayer: card.Owner,
                                                cardEffect: activateClass);

                                            yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                                            #region リストによる選択条件
                                            bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                                            {
                                                List<CardSource> _cardSources = new List<CardSource>();

                                                foreach(CardSource cardSource1 in cardSources)
                                                {
                                                    _cardSources.Add(cardSource1);
                                                }

                                                _cardSources.Add(cardSource);

                                                List<CardSource[]> cardsList = ParameterComparer.Enumerate(_cardSources, _cardSources.Count).ToList();

                                                bool match = false;

                                                if (cardsList.Count >= 2)
                                                {
                                                    foreach (CardSource[] cardSources1 in cardsList)
                                                    {
                                                        if (CanSelectCardCondition(cardSources1[0]))
                                                        {
                                                            if (CanSelectCardCondition1(cardSources1[1]))
                                                            {
                                                                match = true;
                                                            }
                                                        }

                                                        if (CanSelectCardCondition1(cardSources1[0]))
                                                        {
                                                            if (CanSelectCardCondition(cardSources1[1]))
                                                            {
                                                                match = true;
                                                            }
                                                        }
                                                    }
                                                }

                                                else
                                                {
                                                    match = true;
                                                }

                                                if (!match)
                                                {
                                                    return false;
                                                }

                                                return true;
                                            }
                                            #endregion

                                            #region 選択条件
                                            bool CanEndSelectCondition(List<CardSource> cardSources)
                                            {
                                                List<CardSource[]> cardsList = ParameterComparer.Enumerate(cardSources, cardSources.Count).ToList();

                                                bool match = false;

                                                if(cardsList.Count >= 2)
                                                {
                                                    foreach (CardSource[] cardSources1 in cardsList)
                                                    {
                                                        if(CanSelectCardCondition(cardSources1[0]))
                                                        {
                                                            if(CanSelectCardCondition1(cardSources1[1]))
                                                            {
                                                                match = true;
                                                            }
                                                        }

                                                        if (CanSelectCardCondition1(cardSources1[0]))
                                                        {
                                                            if (CanSelectCardCondition(cardSources1[1]))
                                                            {
                                                                match = true;
                                                            }
                                                        }
                                                    }
                                                }

                                                else
                                                {
                                                    match = true;
                                                }

                                                if(!match)
                                                {
                                                    return false;
                                                }

                                                return true;
                                            }
                                            #endregion

                                            IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                                            {
                                                foreach (CardSource cardSource in revealLibrary.revealedCards)
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
                                       */
                #endregion
            }
        }

        return cardEffects;
    }
}
