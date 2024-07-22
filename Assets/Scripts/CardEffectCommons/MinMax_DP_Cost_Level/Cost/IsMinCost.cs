using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using System.Security;

public partial class CardEffectCommons
{
    public static bool IsMinCost(Permanent permanent, Player owner, bool IsDigimonOnly, Func<Permanent, bool> condition = null)
    {
        if (permanent == null) return false;
        if (permanent.TopCard == null) return false;
        if (permanent.TopCard.Owner != owner) return false;
        if (!IsPermanentExistsOnOwnerBattleArea(permanent, permanent.TopCard)) return false;
        if (!permanent.IsDigimon && !permanent.IsTamer) return false;
        if (condition != null && !condition(permanent)) return false;
        if (!permanent.TopCard.HasPlayCost) return false;

        List<Permanent> permanents = permanent.TopCard.Owner.GetBattleAreaPermanents();
        List<int> costs = new List<int>();

        if (IsDigimonOnly)
        {
            costs = permanent.TopCard.Owner.GetBattleAreaPermanents()
                .Filter(permanent1 => condition != null && condition(permanent1))
                .Filter(permanent1 => permanent1.IsDigimon && permanent1.TopCard.HasPlayCost)
                .Map(permanent1 => permanent1.TopCard.GetCostItself);
        }
        else
        {
            costs = permanent.TopCard.Owner.GetBattleAreaPermanents()
                .Filter(permanent1 => condition != null && condition(permanent1))
                .Filter(permanent1 => (permanent1.IsDigimon || permanent1.IsTamer) && permanent1.TopCard.HasPlayCost)
                .Map(permanent1 => permanent1.TopCard.GetCostItself);
        }

        return costs.Count >= 1 && permanent.TopCard.GetCostItself == costs.Min();
    }
}