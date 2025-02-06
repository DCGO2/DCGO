using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;


public class P_122 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Add 1 card from security to hand", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[On Play] Search your security stack. You may add 1 yellow/black card with 2 or more colors among them to the hand. If you did, <Recovery +1 (Deck)> (Place the top card of your deck on top of your security stack). Then, shuffle your security stack.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                if (cardSource.CardColors.Count >= 2)
                {
                    if (cardSource.CardColors.Contains(CardColor.Yellow))
                    {
                        return true;
                    }

                    if (cardSource.CardColors.Contains(CardColor.Black))
                    {
                        return true;
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
                    if (card.Owner.SecurityCards.Count >= 1)
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                int maxCount = Math.Min(1, card.Owner.SecurityCards.Count(CanSelectCardCondition));

                CardSource selectedCard = null;

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
                            refSkillInfos: ref ContinuousController.instance.nullSkillInfos).ReduceSecurity());
                    }

                    yield return null;
                }

                if (selectedCard != null)
                {
                    yield return ContinuousController.instance.StartCoroutine(new IRecovery(card.Owner, 1, activateClass).Recovery());
                }

                ContinuousController.instance.PlaySE(GManager.instance.ShuffleSE);

                card.Owner.SecurityCards = RandomUtility.ShuffledDeckCards(card.Owner.SecurityCards);
            }
        }

        if (timing == EffectTiming.None)
        {
            bool CardCondition(CardSource cardSource)
            {
                return cardSource.Owner == card.Owner.Enemy;
            }

            bool Condition()
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        return true;
                    }
                }
                return false;
            }

            cardEffects.Add(CardEffectFactory.ChangeSecurityDigimonCardDPStaticEffect(
                cardCondition: CardCondition,
                changeValue: -2000,
                isInheritedEffect: true,
                card: card,
                condition: Condition,
                effectName: "Opponent's Security Digimon gains DP -2000"));
        }

        return cardEffects;
    }
}
