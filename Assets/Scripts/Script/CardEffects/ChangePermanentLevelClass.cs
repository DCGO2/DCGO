using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;

public class ChangePermanentLevelClass : ICardEffect, IChangePermanentLevelEffect
{
    Func<Permanent, int, int> GetLevel { get; set; } = null;
    public void SetUpChangePermanentLevelClass(Func<Permanent, int, int> GetLevel)
    {
        this.GetLevel = GetLevel;
    }

    public int GetPermanentLevel(int level, Permanent permanent)
    {
        if (permanent != null)
        {
            if (permanent.TopCard != null)
            {
                if (GetLevel != null)
                {
                    level = GetLevel(permanent, level);
                }
            }
        }

        return level;
    }
}