using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using System.ComponentModel.Design;

public partial class AutomaticOrder
{
    public static int GetSkillIndexAutomaticOrder(List<SkillInfo> skillInfos_active)
    {
        if (skillInfos_active.Count == 0) return 0;
        List<SkillInfo> skillInfos_active_clone = skillInfos_active.Clone();

        #region メモリーを3にするテイマー
        List<SkillInfo> setMemory3TamerSkills = skillInfos_active_clone
            .Filter(skillInfo => skillInfo.CardEffect.EffectName.Contains("Set Memory to "));

        skillInfos_active_clone = skillInfos_active_clone
            .Filter(skillInfo => !setMemory3TamerSkills.Contains(skillInfo));
        #endregion

        skillInfos_active_clone =
            setMemory3TamerSkills
            .Concat(skillInfos_active_clone)
            .ToList();

        if (skillInfos_active_clone.Count >= 1)
        {
            int index = skillInfos_active.IndexOf(skillInfos_active_clone[0]);

            if (0 <= index && index <= skillInfos_active.Count - 1)
            {
                return index;
            }
        }

        return 0;
    }
}