using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT8_038 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            bool PermanentCondition(Permanent targetPermanent)
            {
                return targetPermanent.TopCard.CardNames.Contains("Veemon");
            }

            cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(
                permanentCondition: PermanentCondition,
                digivolutionCost: 3,
                ignoreDigivolutionRequirement: false,
                card: card,
                condition: null));
        }

        if (timing == EffectTiming.None)
        {
            cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
        }

        if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
        {
            cardEffects.Add(CardEffectFactory.ArmorPurgeEffect(card: card));
        }

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Unsuspend this Digimon and gain DP+", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] Unsuspend this Digimon, and it gets +2000 DP until the end of your opponent's next turn for each card with [Armor Form] in its traits in your trash.";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    return true;
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(
                    new List<Permanent>() { card.PermanentOfThisCard() },
                    activateClass).Unsuspend());

                int plusDP = 2000 * card.Owner.TrashCards.Count(
                    (cardSource) => cardSource.CardTraits.Contains("Armor Form") || cardSource.CardTraits.Contains("ArmorForm"));

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(
                            targetPermanent: card.PermanentOfThisCard(),
                            changeValue: plusDP,
                            effectDuration: EffectDuration.UntilOpponentTurnEnd,
                            activateClass: activateClass));
            }
        }

        return cardEffects;
    }
}
