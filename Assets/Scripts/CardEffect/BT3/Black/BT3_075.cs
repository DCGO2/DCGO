using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class BT3_075 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            cardEffects.Add(CardEffectFactory.BlockerSelfStaticEffect(isInheritedEffect: false, card: card, condition: null));
        }

        if (timing == EffectTiming.None)
        {
            bool PermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                {
                    if (permanent.HasBlocker)
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CardEffectCondition(ICardEffect cardEffect)
            {
                if (cardEffect != null)
                {
                    if (cardEffect.EffectSourceCard != null)
                    {
                        if (cardEffect.EffectSourceCard.Owner == card.Owner.Enemy)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool CanUseCondition()
            {
                return CardEffectCommons.IsExistOnBattleArea(card);
            }

            string effectName = "Your Digimon with Blocker can't be deleted by opponent's effects";

            cardEffects.Add(CardEffectFactory.CanNotBeDestroyedBySkillStaticEffect(
                permanentCondition: PermanentCondition,
                cardEffectCondition: CardEffectCondition,
                isInheritedEffect: false,
                card: card,
                condition: CanUseCondition,
                effectName: effectName
            ));
        }

        return cardEffects;
    }
}
