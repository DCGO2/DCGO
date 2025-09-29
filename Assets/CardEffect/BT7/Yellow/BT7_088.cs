using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT7_088 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.SecuritySkill)
        {
            cardEffects.Add(CardEffectFactory.PlaySelfTamerSecurityEffect(card));
        }

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Add 1 card from security to hand", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[On Play] You may search your security stack for 1 card with [Hybrid] or [Ten Warriors] in its traits, reveal it, and add it to your hand. If you added a card to your hand, <Recovery +1 (Deck)>. (Place the top card of your deck on top of your security stack.) Then, shuffle your security stack.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                if (cardSource.CardTraits.Contains("Hybrid"))
                {
                    return true;
                }

                if (cardSource.CardTraits.Contains("Ten Warriors"))
                {
                    return true;
                }

                if (cardSource.CardTraits.Contains("TenWarriors"))
                {
                    return true;
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
                return cardSource.Owner == card.Owner;
            }

            bool Condition()
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOpponentTurn(card))
                    {
                        return true;
                    }
                }
                return false;
            }

            cardEffects.Add(CardEffectFactory.ChangeSecurityDigimonCardDPStaticEffect(
                cardCondition: CardCondition,
                changeValue: 3000,
                isInheritedEffect: true,
                card: card,
                condition: Condition,
                effectName: "Your Security Digimon gains DP +3000"));
        }

        return cardEffects;
    }
}
