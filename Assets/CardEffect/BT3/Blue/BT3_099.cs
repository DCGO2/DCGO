using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT3_099 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OptionSkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect(card.BaseENGCardNameFromEntity, CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Main] Neither player's Digimon can be deleted in battle for the turn.";
            }
            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerOptionMainEffect(hashtable, card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                bool PermanentCondition(Permanent permanent)
                {
                    return CardEffectCommons.IsPermanentExistsOnBattleArea(permanent);
                }

                bool CanNotBeDestroyedByBattleCondition(Permanent permanent, Permanent AttackingPermanent, Permanent DefendingPermanent, CardSource DefendingCard)
                {
                    if (permanent == AttackingPermanent)
                    {
                        return true;
                    }

                    if (permanent == DefendingPermanent)
                    {
                        return true;
                    }

                    return false;
                }

                string effectName = "Can't be deleted in battle";

                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.GainCanNotBeDeletedPlayerEffect(
                    permanentCondition: PermanentCondition,
                    canNotBeDestroyedByBattleCondition: CanNotBeDestroyedByBattleCondition,
                    effectDuration: EffectDuration.UntilOwnerTurnEnd,
                    activateClass: activateClass,
                    effectName: effectName));
            }
        }


        if (timing == EffectTiming.SecuritySkill)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect($"Add this card to hand", CanUseCondition, card);
            activateClass.SetUpActivateClass(null, ActivateCoroutine, -1, false, EffectDiscription());
            activateClass.SetIsSecurityEffect(true);
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[Security] Add this card to its owner's hand.";
            }
            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerSecurityEffect(hashtable, card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.AddThisCardToHand(card, activateClass));
            }
        }

        return cardEffects;
    }
}
