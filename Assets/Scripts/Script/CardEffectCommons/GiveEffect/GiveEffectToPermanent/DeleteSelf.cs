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

    public static IEnumerator AddSelfDeleteEffect(Permanent permanent, DeleteTiming deleteTiming)
    {
        bool deleteOnOwnturn = deleteTiming != DeleteTiming.AtOpponentTurnEnd;
        bool deleteOnOpponentsTurn = deleteTiming != DeleteTiming.AtOwnTurnEnd;
        permanent.PermanentEffects.Add(GetCardEffect);

        ICardEffect GetCardEffect(EffectTiming timing)
        {
            if (timing == EffectTiming.OnEndTurn)
            {
                return CardEffectFactory.DeleteSelfEffect(permanent, deleteOnOwnturn, deleteOnOpponentsTurn);
            }
            return null;
        }

        yield return null;
    }
}