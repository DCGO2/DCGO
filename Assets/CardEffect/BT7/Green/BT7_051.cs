using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
using System.Net.Security;
public class BT7_051 : CEntity_Effect
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
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(targetPermanent, card))
                {
                    if (targetPermanent.DigivolutionCards.Count((cardSource) => cardSource.IsTamer) >= 1)
                    {
                        return true;
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

        if (timing == EffectTiming.OnAllyAttack)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Digivolve this Digimon into [Insectoid] or [Ten Warriors] Digimon", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, true, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Attacking] If a card with [Hybrid] or [Insectoid] in its traits is in this Digimon's digivolution cards, this Digimon can digivolve into a Digimon card with [Insectoid] or [Ten Warriors] in its traits in your hand for a memory cost of 3.";
            }

            bool CanSelectCardCondition(CardSource cardSource)
            {
                return cardSource.IsDigimon && (cardSource.CardTraits.Contains("Insectoid") || cardSource.CardTraits.Contains("Ten Warriors") || cardSource.CardTraits.Contains("TenWarriors"));
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOnAttack(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (card.Owner.HandCards.Count >= 1)
                    {
                        if (card.PermanentOfThisCard().DigivolutionCards.Count((cardSource) => cardSource.CardTraits.Contains("Hybrid") || cardSource.CardTraits.Contains("Insectoid")) >= 1)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.DigivolveIntoHandOrTrashCard(
                    targetPermanent: card.PermanentOfThisCard(),
                    cardCondition: CanSelectCardCondition,
                    payCost: true,
                    reduceCostTuple: null,
                    fixedCostTuple: (fixedCost: 3, fixedCostCardCondition: null),
                    ignoreDigivolutionRequirementFixedCost: -1,
                    isHand: true,
                    activateClass: activateClass,
                    successProcess: null));
            }
        }

        return cardEffects;
    }
}
