using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;


public class BT1_002 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        #region Your Turn

        if (timing != EffectTiming.None)
        {
            if (CardEffectCommons.IsExistOnBattleAreaDigimon(card))
            {
                Permanent thisPermanent = card.PermanentOfThisCard();

                if (Condition())
                    thisPermanent.AddBoost(new Permanent.DPBoost("BT1_002", 2000, Condition));
                else
                    thisPermanent.RemoveBoost("BT1_002");
            }

            bool Condition()
            {
                return CardEffectCommons.IsPermanentExistsOnOwnerBattleArea(card.PermanentOfThisCard(), card) &&
                       CardEffectCommons.IsOwnerTurn(card) &&
                       card.PermanentOfThisCard().DigivolutionCards.Contains(card) &&
                       card.PermanentOfThisCard().HasPierce;
            }
        }

        #endregion

        return cardEffects;
    }
}
