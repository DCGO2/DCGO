using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class Pikodevimon_EX1_056 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnDestroyedAnyone)
        {
            cardEffects.Add(CardEffectFactory.RetaliationSelfEffect(isInheritedEffect: false, card: card, condition: null));
        }

        if (timing == EffectTiming.None)
        {
            bool DefenderCondition(Permanent defender)
            {
                return CardEffectCommons.IsPermanentExistsOnOpponentBattleArea(defender, card);
            }

            bool Condition()
            {
                if (CardEffectCommons.IsOwnerTurn(card))
                {
                    if (!CardEffectCommons.HasMatchConditionOwnersPermanent(card, (permanent) => permanent.IsDigimon && permanent.TopCard.ContainsCardName("Myotismon")))
                    {
                        return true;
                    }
                }

                return false;
            }

            cardEffects.Add(CardEffectFactory.CanNotAttackSelfStaticEffect(defenderCondition: DefenderCondition, isInheritedEffect: false, card: card, condition: Condition, effectName: "Can't Attack to Digimon"));
        }

        return cardEffects;
    }
}