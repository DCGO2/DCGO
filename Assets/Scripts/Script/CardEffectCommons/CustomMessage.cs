using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Custom message

    #region Custom message when select permanent

    #region Custom message template
    public static string[] customPermanentMessageArrayTemplate(string customText, int maxCount, bool CanSelectDigimon, bool CanSelectTamer)
    {
        bool CanSelectMultiple = maxCount >= 2;

        string permanentKindText()
        {
            if (CanSelectDigimon && CanSelectTamer)
            {
                return "Digimon or Tamer";
            }

            if (CanSelectDigimon && !CanSelectTamer)
            {
                return "Digimon";
            }

            if (!CanSelectDigimon && CanSelectTamer)
            {
                return "Tamer";
            }

            return "card";
        }

        string targetText()
        {
            if (CanSelectMultiple)
            {
                if (!CanSelectDigimon || !CanSelectTamer)
                {
                    return $"{permanentKindText()}s";
                }
            }

            else
            {
                return $"1 {permanentKindText()}";
            }

            return permanentKindText();
        }

        return new string[] { $"Select {targetText()} {customText}.", $"The opponent is selecting {targetText()} {customText}." };
    }
    #endregion

    #region Custom message when Digimon's DP will be changed
    public static string[] customPermanentMessageArray_ChangeDP(int changeValue, int maxCount)
    {
        bool isUpValue = changeValue > 0;
        string changeValueText = isUpValue ? $"that will gain DP +{changeValue}" : $"that will gain DP {changeValue}";

        return customPermanentMessageArrayTemplate(customText: changeValueText, maxCount: maxCount, CanSelectDigimon: true, CanSelectTamer: false);
    }
    #endregion

    #region Custom message when Digimon's origin DP will be changed
    public static string[] customPermanentMessageArray_ChangeOriginDP(int changeValue, int maxCount)
    {
        string changeValueText = $"whose origin DP will be {changeValue}";

        return customPermanentMessageArrayTemplate(customText: changeValueText, maxCount: maxCount, CanSelectDigimon: true, CanSelectTamer: false);
    }
    #endregion

    #region Custom message when Digimon's SAttack will be changed
    public static string[] customPermanentMessageArray_ChangeSAttack(int changeValue, int maxCount)
    {
        bool isUpValue = changeValue > 0;
        string changeValueText = isUpValue ? $"that will gain Security Attack +{changeValue}" : $"that will gain Security Attack {changeValue}";

        return customPermanentMessageArrayTemplate(customText: changeValueText, maxCount: maxCount, CanSelectDigimon: true, CanSelectTamer: false);
    }
    #endregion

    #endregion

    #endregion
}