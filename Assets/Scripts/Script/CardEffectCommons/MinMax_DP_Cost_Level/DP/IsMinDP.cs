using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using System.Security;

public partial class CardEffectCommons
{
    public static bool IsMinDP(Permanent permanent, Player owner, Func<Permanent, bool> condition = null)
    {
        if (permanent == null) return false;
        if (permanent.TopCard == null) return false;
        if (permanent.TopCard.Owner != owner) return false;
        if (!IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, permanent.TopCard)) return false;
        if (condition != null && !condition(permanent)) return false;
        if (!permanent.TopCard.HasDP && (permanent.BaseDP <= 0)) return false;

        List<int> DPs = permanent.TopCard.Owner.GetBattleAreaDigimons()
            .Filter(permanent1 => condition == null || (condition != null && condition(permanent1)))
            .Filter(permanent1 => permanent1.TopCard.HasDP || (permanent1.BaseDP > 0))
            .Map(permanent1 => permanent1.DP);

        return DPs.Count >= 1 && permanent.DP == DPs.Min();
    }
}