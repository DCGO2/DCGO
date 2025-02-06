using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT7_072 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnDiscardHand)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Play this card", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "When you trash this card in your hand using one of your effects, if [Eyesmon: Scatter Mode] is in your trash, you may play this card without paying its memory cost.";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                bool SkillCondition(ICardEffect cardEffect)
                {
                    if (cardEffect != null)
                    {
                        if (cardEffect.EffectSourceCard != null)
                        {
                            if (cardEffect.EffectSourceCard.Owner == card.Owner)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }

                return CardEffectCommons.CanTriggerOnTrashSelfHand(hashtable, SkillCondition, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnTrash(card))
                {
                    if (CardEffectCommons.HasMatchConditionOwnersCardInTrash(card, (cardSource) => cardSource.CardNames.Contains("Eyesmon: Scatter Mode") || cardSource.CardNames.Contains("Eyesmon:ScatterMode")))
                    {
                        if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: card, payCost: false, cardEffect: activateClass))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                if (CardEffectCommons.IsExistOnTrash(card))
                {
                    if (CardEffectCommons.CanPlayAsNewPermanent(cardSource: card, payCost: false, cardEffect: activateClass))
                    {
                        yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.PlayPermanentCards(
                        cardSources: new List<CardSource>() { card },
                        activateClass: activateClass,
                        payCost: false,
                        isTapped: false,
                        root: SelectCardEffect.Root.Trash,
                        activateETB: true));
                    }
                }
            }
        }

        if (timing == EffectTiming.None)
        {
            int count()
            {
                return card.Owner.TrashCards.Count((cardSource) => cardSource.CardNames.Contains("Eyesmon: Scatter Mode") || cardSource.CardNames.Contains("Eyesmon:ScatterMode"));
            }

            bool Condition()
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        if (count() >= 1)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            cardEffects.Add(CardEffectFactory.ChangeSelfDPStaticEffect<Func<int>>(changeValue: () => 2000 * count(), isInheritedEffect: false, card: card, condition: Condition));
        }

        return cardEffects;
    }
}
