using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using System.Security;

public partial class CardEffectCommons
{
    public static bool IsMinDigivolutionCards(Permanent permanent, Player owner, Func<Permanent, bool> condition = null)
    {
        if (permanent == null) return false;
        if (permanent.TopCard == null) return false;
        if (permanent.TopCard.Owner != owner) return false;
        if (!IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, permanent.TopCard)) return false;
        if (condition != null && !condition(permanent)) return false;

        List<int> DigivolutionCardCounts = permanent.TopCard.Owner.GetBattleAreaDigimons()
            .Filter(permanent1 => condition == null || (condition != null && condition(permanent1)))
            .Map(permanent1 => permanent1.DigivolutionCards.Count);

        return DigivolutionCardCounts.Count >= 1 && permanent.DigivolutionCards.Count == DigivolutionCardCounts.Min();
    }
}