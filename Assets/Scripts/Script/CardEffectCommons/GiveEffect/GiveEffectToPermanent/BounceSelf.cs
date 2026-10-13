using System.Collections;

public partial class CardEffectCommons
{
    public enum BounceTiming
    {
        AtTurnEnd,
        AtOwnTurnEnd,
        AtOpponentTurnEnd
    }

    public static IEnumerator AddSelfBounceEffect(Permanent permanent, BounceTiming bounceTiming, ICardEffect activateClass)
    {
        bool bounceOnOwnturn = bounceTiming != BounceTiming.AtOpponentTurnEnd;
        bool bounceOnOpponentsTurn = bounceTiming != BounceTiming.AtOwnTurnEnd;
        string message = "Bounce this at turn end";
        if (bounceOnOpponentsTurn && !bounceOnOwnturn)
            message = "Bounce this at opponent's turn end";
        if (bounceOnOwnturn && ! bounceOnOpponentsTurn)
            message = "Bounce this at your turn end.";
        permanent.PermanentEffects.Add(GetCardEffect);
        permanent.PermanentEffects.Add(GetDetailEffect);

        ICardEffect GetCardEffect(EffectTiming timing)
        {
            if (timing == EffectTiming.OnEndTurn)
            {
                return PermanentEffectFactory.BounceSelfEffect(permanent, activateClass, bounceOnOwnturn, bounceOnOpponentsTurn);
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