using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class P_071 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.SecuritySkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Play 1 level 3 Digimon from trash and add this card to hand", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            activateClass.SetIsSecurityEffect(true);
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Security] At the end of the battle, you may play 1 purple level 3 Digimon card from your trash without paying its memory cost. Then, add this card to its owner's hand.";
            }


            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnExecutingArea(card))
                {
                    return true;
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return null;

                ContinuousController.instance.PlaySE(GManager.instance.GetComponent<Effects>().BuffSE);

                ActivateClass activateClass1 = new ActivateClass();
                activateClass1.SetUpICardEffect("Play 1 level 3 Digimon from trash and add this card to hand", CanUseCondition1, card);
                activateClass1.SetUpActivateClass(CanActivateCondition1, ActivateCoroutine1, -1, false, EffectDiscription1());
                card.Owner.UntilEndBattleEffects.Add(GetCardEffect1);

                string EffectDiscription1()
                {
                    return "Play 1 purple level 3 Digimon card from your trash without paying its memory cost. Then, add this card to its owner's hand.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    if (cardSource != null)
                    {
                        if (cardSource.IsDigimon)
                        {
                            if (cardSource.Owner == card.Owner)
                            {
                                if (cardSource.CardColors.Contains(CardColor.Purple))
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
                        }
                    }

                    return false;
                }

                bool CanUseCondition1(Hashtable hashtable)
                {
                    return true;
                }

                bool CanActivateCondition1(Hashtable hashtable)
                {
                    return true;
                }

                IEnumerator ActivateCoroutine1(Hashtable _hashtable1)
                {
                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, (cardSource) => CanSelectCardCondition(cardSource)))
                    {
                        int maxCount = Math.Min(1, card.Owner.TrashCards.Count((cardSource) => CanSelectCardCondition(cardSource)));

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

                    if (card.Owner.ExecutingCards.Contains(card))
                    {
                        yield return ContinuousController.instance.StartCoroutine(card.Owner.brainStormObject.CloseBrainstrorm(card));

                        yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddHandCards(new List<CardSource>() { card }, false, activateClass));
                    }
                }

                ICardEffect GetCardEffect1(EffectTiming _timing)
                {
                    if (_timing == EffectTiming.OnEndBattle)
                    {
                        return activateClass1;
                    }

                    return null;
                }
            }
        }

        return cardEffects;
    }
}
