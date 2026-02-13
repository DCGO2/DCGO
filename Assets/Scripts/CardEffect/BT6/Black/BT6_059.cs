using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT6_059 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.WhenPermanentWouldBeDeleted)
        {
            bool CanSelectPermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, card))
                {
                    if (permanent != card.PermanentOfThisCard())
                    {
                        if (permanent.TopCard.CardColors.Contains(CardColor.Black))
                        {
                            if (permanent.willBeRemoveField)
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }

            string EffectDiscription()
            {
                return "<Decoy (Black)> (When one of your other black Digimon would be deleted by an opponent's effect, you may delete this Digimon to prevent that deletion.)";
            }

            cardEffects.Add(CardEffectFactory.DecoySelfEffect(isInheritedEffect: false, card: card, condition: null, permanentCondition: CanSelectPermanentCondition, effectName: "Decoy (Black)", effectDiscription: EffectDiscription()));
        }

        return cardEffects;
    }
}
