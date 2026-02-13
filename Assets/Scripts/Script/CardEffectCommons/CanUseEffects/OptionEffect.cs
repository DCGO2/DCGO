using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can trigger option [Main] effect
    public static bool CanTriggerOptionMainEffect(Hashtable hashtable, CardSource card)
    {
        CardSource Card = GetCardFromHashtable(hashtable);

        if (Card != null)
        {
            if (Card == card)
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    #region Can declare option [Delay] effect
    public static bool CanDeclareOptionDelayEffect(CardSource card)
    {
        if (IsExistOnBattleArea(card))
        {
            if (card.PermanentOfThisCard().EnterFieldTurnCount != GManager.instance.turnStateMachine.TurnCount)
            {
                return true;
            }
        }

        return false;
    }
    #endregion
}