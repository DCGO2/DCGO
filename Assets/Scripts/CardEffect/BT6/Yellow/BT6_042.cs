using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT6_042 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnDestroyedAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Play Digimons from hand", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[On Deletion] You may play 1 [Rosemon] or up to 2 yellow level 3 Digimon cards from your hand without paying their memory costs.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                if (cardSource.CardNames.Contains("Rosemon"))
                {
                    if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CanSelectCardCondition1(CardSource cardSource)
            {
                if (cardSource.IsDigimon)
                {
                    if (cardSource.CardColors.Contains(CardColor.Yellow))
                    {
                        if (cardSource.Level == 3)
                        {
                            if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: cardSource, payCost: false, cardEffect: activateClass))
                            {
                                if (cardSource.HasLevel)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }

                return false;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOnDeletion(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnTrash(card))
                {
                    if (card.Owner.HandCards.Count >= 1)
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (card.Owner.HandCards.Count((cardSource) => CanSelectCardCondition(cardSource) || CanSelectCardCondition1(cardSource)) >= 1)
                {
                    List<CardSource> selectedCards = new List<CardSource>();

                    int maxCount = Math.Min(2, card.Owner.HandCards.Count((cardSource) => CanSelectCardCondition(cardSource) || CanSelectCardCondition1(cardSource)));

                    SelectHandEffect selectHandEffect = GManager.instance.GetComponent<SelectHandEffect>();

                    selectHandEffect.SetUp(
                        selectPlayer: card.Owner,
                        canTargetCondition: (cardSource) => CanSelectCardCondition(cardSource) || CanSelectCardCondition1(cardSource),
                        canTargetCondition_ByPreSelecetedList: CanTargetCondition_ByPreSelecetedList,
                        canEndSelectCondition: CanEndSelectCondition,
                        maxCount: maxCount,
                        canNoSelect: true,
                        canEndNotMax: true,
                        isShowOpponent: true,
                        selectCardCoroutine: SelectCardCoroutine,
                        afterSelectCardCoroutine: null,
                        mode: SelectHandEffect.Mode.Custom,
                        cardEffect: activateClass);

                    selectHandEffect.SetUpCustomMessage("Select cards to play.", "The opponent is selecting cards to play.");
                    selectHandEffect.SetUpCustomMessage_ShowCard("Played Cards");

                    yield return StartCoroutine(selectHandEffect.Activate());

                    bool CanTargetCondition_ByPreSelecetedList(List<CardSource> cardSources, CardSource cardSource)
                    {
                        if (cardSources.Count(CanSelectCardCondition) >= 1)
                        {
                            if (CanSelectCardCondition1(cardSource))
                            {
                                return false;
                            }
                        }

                        if (cardSources.Count(CanSelectCardCondition1) >= 1)
                        {
                            if (CanSelectCardCondition(cardSource))
                            {
                                return false;
                            }
                        }

                        if (cardSources.Count(CanSelectCardCondition) >= 1)
                        {
                            if (CanSelectCardCondition(cardSource))
                            {
                                return false;
                            }
                        }

                        if (cardSources.Count(CanSelectCardCondition1) >= 2)
                        {
                            if (CanSelectCardCondition1(cardSource))
                            {
                                return false;
                            }
                        }

                        return true;
                    }

                    bool CanEndSelectCondition(List<CardSource> cardSources)
                    {
                        if (maxCount >= 1)
                        {
                            if (cardSources.Count <= 0)
                            {
                                return false;
                            }
                        }

                        if (cardSources.Count(CanSelectCardCondition) >= 2)
                        {
                            return false;
                        }

                        if (cardSources.Count(CanSelectCardCondition1) >= 3)
                        {
                            return false;
                        }

                        if (cardSources.Count(CanSelectCardCondition) >= 1)
                        {
                            if (cardSources.Count(CanSelectCardCondition1) >= 1)
                            {
                                return false;
                            }
                        }

                        return true;
                    }

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
                            root: SelectCardEffect.Root.Hand,
                            activateETB: true));
                }
            }
        }

        return cardEffects;
    }
}
