using System;
using System.Collections;
using System.Collections.Generic;

public partial class CardEffectCommons
{
    public enum DeleteTiming
    {
        AtTurnEnd,
        AtOwnTurnEnd,
        AtOpponentTurnEnd
    }

    public static IEnumerator AddSelfDeleteEffect(Permanent permanent, DeleteTiming deleteTiming, ICardEffect activateClass)
    {
        bool deleteOnOwnturn = deleteTiming != DeleteTiming.AtOpponentTurnEnd;
        bool deleteOnOpponentsTurn = deleteTiming != DeleteTiming.AtOwnTurnEnd;
        string message = "Delete this at turn end";
        if (deleteOnOpponentsTurn && !deleteOnOwnturn)
            message = "Delete this at opponent's turn end";
        if (deleteOnOwnturn && ! deleteOnOpponentsTurn)
            message = "Delete this at your turn end.";
        permanent.PermanentEffects.Add(GetCardEffect);
        permanent.PermanentEffects.Add(GetDetailEffect);

        ICardEffect GetCardEffect(EffectTiming timing)
        {
            if (timing == EffectTiming.OnEndTurn)
            {
                return PermanentEffectFactory.DeleteSelfEffect(permanent, activateClass, deleteOnOwnturn, deleteOnOpponentsTurn);
            }
            return null;
        }

        ICardEffect GetDetailEffect(EffectTiming timing)
        {
            if (timing == EffectTiming.None)
            {
                return PermanentEffectFactory.AddDetailClass(permanent, message, true, activateClass);
            }
            return null;
        }

        yield return null;
    }
}