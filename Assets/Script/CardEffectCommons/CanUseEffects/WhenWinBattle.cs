using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can trigger "when a Digimon wins a battle" effect
    public static bool CanTriggerWhenWinBattle(
        Hashtable hashtable,
        Func<Permanent, bool> winnerCondition,
        Func<Permanent, bool> winnerRealCondition = null,
        bool isSecurityOnly = false)
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
                        if (battleHashtable.ContainsKey("WasTie"))
                        {
                            bool WasTie = (bool)battleHashtable["WasTie"];

                            if (!WasTie)
                            {
                                if(battleHashtable.ContainsKey("LoserCard"))
                                {
                                    CardSource LoserCard = (CardSource)battleHashtable["LoserCard"];

                                    if (!isSecurityOnly || LoserCard != null)
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

        return false;
    }
    #endregion
}