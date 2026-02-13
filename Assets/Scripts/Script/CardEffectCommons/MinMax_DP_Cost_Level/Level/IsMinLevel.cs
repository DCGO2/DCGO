using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using System.Security;

public partial class CardEffectCommons
{
    public static bool IsMinLevel(Permanent permanent, Player owner)
    {
        if (permanent == null) return false;
        if (permanent.TopCard == null) return false;
        if (permanent.TopCard.Owner != owner) return false;
        if (!IsPermanentExistsOnOwnerBattleAreaDigimon(permanent, permanent.TopCard)) return false;
        if (!permanent.TopCard.HasLevel) return false;

        List<int> Levels = permanent.TopCard.Owner.GetBattleAreaDigimons()
            .Filter(permanent1 => permanent1.TopCard.HasLevel)
            .Map(permanent1 => permanent1.Level);

        return Levels.Count >= 1 && permanent.Level == Levels.Min();
    }
    public static bool IsMinLevelBoard(Permanent permanent)
    {
        if (permanent == null) return false;
        if (permanent.TopCard == null) return false;
        if (!IsPermanentExistsOnBattleAreaDigimon(permanent)) return false;
        if (!permanent.TopCard.HasLevel) return false;

        List<int> Levels = GManager.instance.turnStateMachine.gameContext.Players
                .Map(player => player.GetBattleAreaDigimons())
                .Flat()
                .Filter(permanent1 => permanent1.TopCard.HasLevel)
                .Map(permanent1 => permanent1.Level);

        return Levels.Count >= 1 && permanent.Level == Levels.Min();
    }

}