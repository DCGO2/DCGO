using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT1_025 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("This Digimon gains Security Attack +1", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[When Digivolving] This Digimon gains <Security Attack +1> (This Digimon checks 1 additional security card) for the turn.";
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                return CardEffectCommons.CanTriggerWhenDigivolving(hashtable, card);
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                return CardEffectCommons.IsExistOnBattleArea(card);
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                yield return ContinuousController.instance.StartCoroutine(CardEffectCommons.ChangeDigimonSAttack(
                    targetPermanent: card.PermanentOfThisCard(),
                    changeValue: 1,
                    effectDuration: EffectDuration.UntilEachTurnEnd,
                    activateClass: activateClass));
            }
        }

        if (timing == EffectTiming.None)
        {
            DisableEffectClass invalidationClass = new DisableEffectClass();
            invalidationClass.SetUpICardEffect("Ignore Security Effect", CanUseCondition, card);
            invalidationClass.SetUpDisableEffectClass(DisableCondition: InvalidateCondition);
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

                return false;
            }
        }

        return cardEffects;
    }
}
