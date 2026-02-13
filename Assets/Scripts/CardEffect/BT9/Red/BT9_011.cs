using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class BT9_011 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            bool PermanentCondition(Permanent targetPermanent)
            {
                return targetPermanent.TopCard.CardNames.Contains("Growlmon");
            }

            cardEffects.Add(CardEffectFactory.AddSelfDigivolutionRequirementStaticEffect(permanentCondition: PermanentCondition, digivolutionCost: 0, ignoreDigivolutionRequirement: false, card: card, condition: null));
        }

        if (timing == EffectTiming.None)
        {
            ChangeDPDeleteEffectMaxDPClass changeDPDeleteEffectMaxDPClass = new ChangeDPDeleteEffectMaxDPClass();
            changeDPDeleteEffectMaxDPClass.SetUpICardEffect("Maximum DP of DP-based deletion effects gets +1000 DP", CanUseCondition, card);
            changeDPDeleteEffectMaxDPClass.SetUpChangeDPDeleteEffectMaxDPClass(changeMaxDP: ChangeMaxDP);

            changeDPDeleteEffectMaxDPClass.SetIsInheritedEffect(true);
            cardEffects.Add(changeDPDeleteEffectMaxDPClass);

            bool CanUseCondition(Hashtable hashtable)
            {
                if (isExistOnField(card))
                {
                    if (CardEffectCommons.IsOwnerTurn(card))
                    {
                        if (card.Owner.GetBattleAreaDigimons().Contains(card.PermanentOfThisCard()))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }



            int ChangeMaxDP(int maxDP, ICardEffect cardEffect)
            {
                if (cardEffect != null)
                {
                    if (cardEffect.EffectSourceCard != null)
                    {
                        if (cardEffect.EffectSourceCard.Owner == card.Owner)
                        {
                            maxDP += 1000;
                        }
                    }
                }

                return maxDP;
            }
        }

        return cardEffects;
    }
}
