using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT7_014 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            bool Condition()
            {
                return card.Owner.HandCards.Contains(card);
            }

            bool PermanentCondition(Permanent targetPermanent)
            {
                if (targetPermanent != null)
                {
                    if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(targetPermanent, card))
                    {
                        if (targetPermanent.DigivolutionCards.Count((cardSource) => cardSource.IsTamer) >= 1)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool CardSourceCondition(CardSource cardSource)
            {
                return cardSource == card && cardSource.Owner.HandCards.Contains(cardSource);
            }

            bool RootCondition(SelectCardEffect.Root root)
            {
                return root == SelectCardEffect.Root.Hand;
            }

            cardEffects.Add(CardEffectFactory.ChangeDigivolutionCostStaticEffect(
                changeValue: -2,
                permanentCondition: PermanentCondition,
                cardCondition: CardSourceCondition,
                rootCondition: RootCondition,
                isInheritedEffect: false,
                card: card,
                condition: Condition,
                setFixedCost: false));
        }

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("DP +4000", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] If a card with [Hybrid] in its traits is in this Digimon's digivolution cards, this Digimon gets +4000 DP for the turn.";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.CardTraits.Contains("Hybrid")) >= 1)
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                    targetPermanent: card.PermanentOfThisCard(),
                    changeValue: 4000,
                    effectDuration: EffectDuration.UntilEachTurnEnd,
                    activateClass: activateClass));
            }
        }

        if (timing == EffectTiming.None)
        {
            DisableEffectClass invalidationClass = new DisableEffectClass();
            invalidationClass.SetUpICardEffect("Ignore Security Effect", CanUseCondition, card);
            invalidationClass.SetUpDisableEffectClass(DisableCondition: InvalidateCondition);
            invalidationClass.SetIsInheritedEffect(true);

            cardEffects.Add(invalidationClass);

            bool CanUseCondition(Hashtable hashtable)
            {
                return true;
            }

            bool InvalidateCondition(ICardEffect cardEffect)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        if (card.PermanentOfThisCard().TopCard.ContainsTraits("Hybrid") || card.PermanentOfThisCard().TopCard.ContainsTraits("TenWarriors") || card.PermanentOfThisCard().TopCard.ContainsTraits("Ten Warriors"))
                        {

                            if (cardEffect != null)
                            {
                                if (cardEffect.EffectSourceCard != null)
                                {
                                    if (cardEffect.EffectSourceCard.IsOption)
                                    {
                                        if (cardEffect.IsSecurityEffect)
                                        {
                                            if (GManager.instance.attackProcess.AttackingPermanent == card.PermanentOfThisCard())
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }
                            
                        }
                    }
                }

                return false;
            }
        }

        return cardEffects;
    }
}
