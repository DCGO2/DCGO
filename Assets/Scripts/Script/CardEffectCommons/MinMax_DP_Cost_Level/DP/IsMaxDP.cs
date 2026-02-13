using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using System.Security;

public partial class CardEffectCommons
{
    public static bool IsMaxDP(Permanent permanent, Player owner, Func<Permanent, bool> permanentCondition)
    {
        if (permanent == null) return false;
        if (permanent.TopCard == null) return false;
        if (permanent.TopCard.Owner != owner) return false;
        if (!IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, permanent.TopCard)) return false;
        if (!permanent.TopCard.HasDP && (permanent.BaseDP <= 0)) return false;
        if (permanentCondition != null && !permanentCondition(permanent)) return false;

        List<int> DPs = permanent.TopCard.Owner.GetBattleAreaDigimons()
            .Filter(permanent1 => (permanent1.TopCard.HasDP || (permanent1.BaseDP > 0)) && (permanentCondition == null || permanentCondition(permanent1)))
            .Map(permanent1 => permanent1.DP);

        return DPs.Count >= 1 && permanent.DP == DPs.Max();
    }
}