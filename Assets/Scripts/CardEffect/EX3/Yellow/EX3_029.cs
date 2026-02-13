using System;
using System.Collections;
using System.Collections.Generic;

namespace DCGO.CardEffects.EX3
{
    public class EX3_029 : CEntity_Effect
    {
        public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
        {
            List<ICardEffect> cardEffects = new List<ICardEffect>();

            if (timing == EffectTiming.OnEnterFieldAnyone)
            {
                ActivateClass activateClass = new ActivateClass();
                activateClass.SetUpICardEffect("Add a card from security to hand", CanUseCondition, card);
                activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
                cardEffects.Add(activateClass);

                string EffectDiscription()
                {
                    return "[On Play] Search your security stack, reveal 1 card from it, and add it to your hand. If it's a yellow card, <Recovery +1 (Deck)>. (Place the top card of your deck on top of your security stack.) Then, shuffle your security stack.";
                }

                bool CanSelectCardCondition(CardSource cardSource)
                {
                    return true;
                }

                bool CanUseCondition(Hashtable hashtable)
                {
                    return CardEffectCommons.CanTriggerOnPlay(hashtable, card);
                }

                bool CanActivateCondition(Hashtable hashtable)
                {
                    if (CardEffectCommons.IsExistOnBattleArea(card))
                    {
                        if (card.Owner.SecurityCards.Count >= 1)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                IEnumerator ActivateCoroutine(Hashtable _hashtable)
                {
                    if (isExistOnField(card))
                    {
                        if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                        {
                            if (card.Owner.SecurityCards.Count >= 1)
                            {
                                int maxCount = Math.Min(1, card.Owner.SecurityCards.Count);

                                CardSource selectedCard = null;

                                SelectCardEffect selectCardEffect = GManager.instance.GetComponent<SelectCardEffect>();

                                selectCardEffect.SetUp(
                                    canTargetCondition: CanSelectCardCondition,
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
                                    root: SelectCardEffect.Root.Security,
                                    customRootCardList: null,
                                    canLookReverseCard: true,
                                    selectPlayer: card.Owner,
                                    cardEffect: activateClass);

                                yield return ContinuousController.instance.StartCoroutine(selectCardEffect.Activate());

                                IEnumerator AfterSelectCardCoroutine(List<CardSource> cardSources)
                                {
                                    foreach (CardSource cardSource in cardSources)
                                    {
                                        selectedCard = cardSource;
                                    }

                                    if (cardSources.Count >= 1)
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(new IReduceSecurity(
                                            player: card.Owner,
                                            refSkillInfos: ref ContinuousController.instance.nullSkillInfos,
                                            activateClass).ReduceSecurity());
                                    }

                                    yield return null;
                                }

                                if (selectedCard != null)
                                {
                                    if (selectedCard.CardColors.Contains(CardColor.Yellow))
                                    {
                                        yield return ContinuousController.instance.StartCoroutine(new IRecovery(card.Owner, 1, activateClass).Recovery());
                                    }
                                }

                                ContinuousController.instance.PlaySE(GManager.instance.ShuffleSE);

                                card.Owner.SecurityCards = RandomUtility.ShuffledDeckCards(card.Owner.SecurityCards);
                            }
                        }
                    }
                }
            }

            return cardEffects;
        }
    }
}