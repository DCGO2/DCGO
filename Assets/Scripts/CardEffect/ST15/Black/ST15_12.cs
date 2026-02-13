using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class ST15_12 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            bool PermanentCondition(Permanent targetPermanent)
            {
                return targetPermanent.TopCard.HasGreymonName && targetPermanent.TopCard.HasLevel && targetPermanent.TopCard.Level == 5;
            }

            cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 3, ignoreDigivolutionRequirement: false, card: card, condition: null));
        }

        if (timing == EffectTiming.OnCounterTiming)
        {
            cardEffects.Add(CardEffectFactory.BlastDigivolveEffect(card: card, condition: null));
        }

        if (timing == EffectTiming.None)
        {
            cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
        }

        if (timing == EffectTiming.OnLoseSecurity)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Unsuspend this Digimon", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, 1, true, EffectDiscription());
            activateClass.SetHashString("Unsuspend_ST15_12");
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[All Turns][Once Per Turn] When a card is removed from a security stack, you may unsuspend this Digimon.";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.CanTriggerWhenLoseSecurity(hashtable, player => true))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.CanUnsuspend(card.PermanentOfThisCard()))
                    {
                        return true;
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                Permanent selectedPermanent = card.PermanentOfThisCard();

                yield return ContinuousController.instance.StartCoroutine(new IUnsuspendPermanents(new List<Permanent>() { selectedPermanent }, activateClass).Unsuspend());
            }
        }

        return cardEffects;
    }
}
