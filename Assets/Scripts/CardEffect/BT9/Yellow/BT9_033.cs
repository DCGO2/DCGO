using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;

public class BT9_033 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.None)
        {
            CanNotPutFieldClass canNotPutFieldClass = new CanNotPutFieldClass();
            canNotPutFieldClass.SetUpICardEffect("Players can't play Digimon by effect", CanUseCondition, card);
            canNotPutFieldClass.SetUpCanNotPutFieldClass(cardCondition: CardCondition, cardEffectCondition: CardEffectCondition);
            cardEffects.Add(canNotPutFieldClass);

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    return true;
                }

                return false;
            }

            bool CardCondition(CardSource cardSource)
            {
                return cardSource.IsDigimon || cardSource.IsDigiEgg;
            }

            bool CardEffectCondition(ICardEffect cardEffect)
            {
                return cardEffect != null;
            }
        }

        return cardEffects;
    }
}
