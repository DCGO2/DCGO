using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;

public abstract partial class CEntity_Effect : MonoBehaviourPunCallbacks
{
    public virtual List<ICardEffect> CardEffects(EffectTiming timing, CardSource cardSource)
    {
        return new List<ICardEffect>();
    }

    public List<ICardEffect> GetCardEffects(EffectTiming timing, CardSource cardSource)
    {
        return CardEffects(timing,cardSource).Filter(cardEffect => cardEffect != null);
    }

    //後で消す
    public static bool isExistOnField(CardSource card)
    {
        if (card.PermanentOfThisCard() != null)
        {
            return true;
        }

        return false;
    }
}