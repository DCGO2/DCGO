using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT3_058 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnDetermineDoSecurityCheck)
        {
            cardEffects.Add(CardEffectFactory.PierceSelfEffect(isInheritedEffect: false, card: card, condition: null));
        }

        if (timing == EffectTiming.OnAllyAttack)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("DP +7000 and Security Attack +2", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Attacking] If you attack an opponent's Digimon that has 12000 DP or more, this Digimon gets +7000 DP and <Security Attack +2> (This Digimon checks 2 additional security cards) for the turn.";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.CanTriggerOnAttack(hashtable, card))
                {
                    if (GManager.instance.attackProcess.DefendingPermanent != null)
                    {
                        if (GManager.instance.attackProcess.DefendingPermanent.TopCard != null)
                        {
                            if (GManager.instance.attackProcess.DefendingPermanent.TopCard.Owner == card.Owner.Enemy)
                            {
                                if (GManager.instance.attackProcess.DefendingPermanent.IsDigimon)
                                {
                                    if (GManager.instance.attackProcess.DefendingPermanent.DP >= 12000)
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }

                return false;
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
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonDP(targetPermanent: card.PermanentOfThisCard(), changeValue: 7000, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));

                    yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonSAttack(targetPermanent: card.PermanentOfThisCard(), changeValue: 2, effectDuration: EffectDuration.UntilEachTurnEnd, activateClass: activateClass));
                }
            }
        }

        return cardEffects;
    }
}
