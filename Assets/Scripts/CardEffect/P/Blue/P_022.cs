using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class P_022 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OptionSkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Main] If you have [Davis Motomiya] and [Ken Ichijoji] in play, you may place 1 [ExVeemon] and 1 [Stingmon] from your hand at the bottom of your deck in any order to play 1 [Paildramon] from your hand without paying its memory cost.";
            }

            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                {
                    if (permanent.TopCard.CardNames.Contains("Davis Motomiya"))
                    {
                        return true;
                    }

                    if (permanent.TopCard.CardNames.Contains("DavisMotomiya"))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CanSelectPermanentCondition1(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(permanent, card))
                {
                    if (permanent.TopCard.CardNames.Contains("Ken Ichijoji"))
                    {
                        return true;
                    }

                    if (permanent.TopCard.CardNames.Contains("KenIchijoji"))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                return cardSource.CardNames.Contains("ExVeemon");
            }

            bool CanSelectCardCondition1(CardSource cardSource)
            {
                return cardSource.CardNames.Contains("Stingmon");
            }

            bool CanSelectCardCondition2(CardSource cardSource)
            {
                if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass, root: SelectCardEffect.Root.Hand))
                {
                    if (cardSource.CardNames.Contains("Paildramon"))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition) >= 1 && card.Owner.GetBattleAreaPermanents().Count(CanSelectPermanentCondition1) >= 1)
                {
                    bool returned = false;

                    if (card.Owner.HandCards.Count(CanSelectCardCondition) >= 1 && card.Owner.HandCards.Count(CanSelectCardCondition1) >= 1)
                    {
                        int maxCount = 2;

                        #region 最大枚数
                        List<CardSource[]> cardsList = ParameterComparer.Enumerate(card.Owner.HandCards, maxCount).ToList();

                        List<int> maxCounts = new List<int>() { 0 };

                        foreach (CardSource[] cardSources in cardsList)
                        {
                            List<int> _maxCounts = new List<int>();

                            if (cardSources.Length >= 2)
                            {
                                if (CanSelectCardCondition(cardSources[0]))
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

                            else if (cardSources.Length == 1)
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

                            if (_maxCounts.Count >= 1)
                            {
                                maxCounts.Add(_maxCounts.Max());
                            }
                        }

                        if (maxCounts.Count >= 1)
                        {
                            maxCount = maxCounts.Max();
                        }
                        #endregion

                        if (maxCount == 2)
                        {
                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: (cardSource) => CanSelectCardCondition(cardSource) || CanSelectCardCondition1(cardSource),
                                canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                                canEndSelectCondition: CanEndSelectCondition,
                                maxCount: maxCount,
                                canNoSelect: true,
                                canEndNotMax: false,
                                isShowOpponent: true,
                                selectCardCoroutine: null,
                                afterSelectCardCoroutine: AfterSelectCardCoroutine,
                                mode: SelectHandEffect.Mode.PutLibraryBottom,
                                cardEffect: activateClass);

                            yield return StartCoroutine(selectHandEffect.Activate());

                            #region リストによる選択条件
                            bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                            {
                                List<CardSource> _cardSources = new List<CardSource>();

                                foreach (CardSource cardSource1 in cardSources)
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

                            IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                            {
                                if (cardSources.Count == 2)
                                {
                                    returned = true;
                                }

                                yield return null;
                            }
                        }

                    }

                    if (returned)
                    {
                        if (card.Owner.HandCards.Count(CanSelectCardCondition2) >= 1)
                        {
                            List<CardSource> selectedCards = new List<CardSource>();

                            int maxCount = 1;

                            SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                            selectHandEffect.SetUp(
                                selectPlayer: card.Owner,
                                canTargetCondition: CanSelectCardCondition2,
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
            }
        }


        if (timing == EffectTiming.SecuritySkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect($"Add this card to hand", CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            activateClass.SetIsSecurityEffect(true);
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Security] Add this card to its owner's hand.";
            }
            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.AddThisCardToHand(card, activateClass));
            }
        }

        return cardEffects;
    }
}
