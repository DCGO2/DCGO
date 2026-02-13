using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can trigger "when a Digimon deletes opponent's Digimon by battle" effect
    public static bool CanTriggerWhenDeleteOpponentDigimonByBattle(
        Hashtable hashtable,
        Func<Permanent, bool> winnerCondition,
        Func<Permanent, bool> loserCondition,
        bool isOnlyWinnerSurvive,
        Func<Permanent, bool> winnerRealCondition = null,
        Func<Permanent, bool> loserRealCondition = null)
    {
        if (hashtable != null)
        {
            IBattle battle = GetBattleFromHashtable(hashtable);

            if (battle != null)
            {
                Hashtable battleHashtable = battle.hashtable;

                if (battleHashtable != null)
                {
                    List<Permanent> WinnerPermanents = null;

                    if (battleHashtable.ContainsKey("WinnerPermanents"))
                    {
                        if (battleHashtable["WinnerPermanents"] is List<Permanent>)
                        {
                            WinnerPermanents = (List<Permanent>)battleHashtable["WinnerPermanents"];
                        }
                    }

                    bool WinnerCondition()
                    {
                        if (WinnerPermanents == null || WinnerPermanents.Count == 0)
                        {
                            if (winnerCondition == null)
                            {
                                return true;
                            }
                        }

                        else
                        {
                            if (WinnerPermanents.Some((permanent) => permanent != null && permanent.TopCard != null && (winnerCondition == null || winnerCondition(permanent))))
                            {
                                return true;
                            }
                        }

                        return false;
                    }

                    if (WinnerCondition())
                    {
                        if (battleHashtable.ContainsKey("LoserPermanents"))
                        {
                            List<Permanent> LoserPermanents = (List<Permanent>)battleHashtable["LoserPermanents"];

                            if (LoserPermanents != null)
                            {
                                if (!isOnlyWinnerSurvive || winnerCondition == null || LoserPermanents.Count((permanent) => permanent != null && permanent.TopCard != null && winnerCondition(permanent)) == 0)
                                {
                                    if (loserCondition == null || LoserPermanents.Some((permanent) => permanent != null && permanent.TopCard != null && loserCondition(permanent)))
                                    {
                                        if (battleHashtable.ContainsKey("LoserPermanents_real"))
                                        {
                                            List<Permanent> LoserPermanents_real = (List<Permanent>)battleHashtable["LoserPermanents_real"];

                                            if (LoserPermanents_real != null)
                                            {
                                                if (loserRealCondition == null || LoserPermanents_real.Some((permanent) => permanent.IsDestroyedByBattle &&
                                                (loserRealCondition(permanent))))
                                                {
                                                    if (battleHashtable.ContainsKey("WinnerPermanents_real"))
                                                    {
                                                        List<Permanent> WinnerPermanents_real = (List<Permanent>)battleHashtable["WinnerPermanents_real"];

                                                        if (WinnerPermanents_real != null)
                                                        {
                                                            if (winnerRealCondition == null || WinnerPermanents_real.Some((permanent) => permanent != null && permanent.TopCard != null && (winnerRealCondition(permanent))))
                                                            {
                                                                return true;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        return false;
    }
    #endregion
}