using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can trigger "when a Digimon deletes opponent's Digimon" effect
    public static bool CanTriggerWhenDeleteOpponentDigimon(
        Hashtable hashtable,
        Func<Permanent, bool> winnerCondition,
        Func<Permanent, bool> loserCondition)
    {
        if (hashtable != null)
        {
            ICardEffect effect = GetCardEffectFromHashtable(hashtable);
            IBattle battle = GetBattleFromHashtable(hashtable);

            if (effect != null)
            {
                List<Permanent> permanents = new List<Permanent>();
                List<Hashtable> effectHashtables = GetHashtablesFromHashtable(hashtable);

                foreach (Hashtable effTable in effectHashtables)
                {
                    Permanent permanent = GetPermanentFromHashtable(effTable);
                    if (loserCondition == null || loserCondition(permanent))
                        permanents.Add(permanent);
                }

                permanents = permanents.Filter(loserCondition);

                if (winnerCondition == null || winnerCondition(effect.EffectSourcePermanent))
                {
                    if(loserCondition == null || permanents.Count > 0)
                    {
                        return true;
                    }
                }
                
            }

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
                                if (winnerCondition == null || LoserPermanents.Count((permanent) => permanent != null && permanent.TopCard != null && winnerCondition(permanent)) == 0)
                                {
                                    if (loserCondition == null || LoserPermanents.Some((permanent) => permanent != null && permanent.TopCard != null && loserCondition(permanent)))
                                    {
                                        if (battleHashtable.ContainsKey("LoserPermanents_real"))
                                        {
                                            List<Permanent> LoserPermanents_real = (List<Permanent>)battleHashtable["LoserPermanents_real"];

                                            if (LoserPermanents_real != null)
                                            {
                                                if (LoserPermanents_real.Some((permanent) => permanent.IsDestroyedByBattle))
                                                {
                                                    if (battleHashtable.ContainsKey("WinnerPermanents_real"))
                                                    {
                                                        List<Permanent> WinnerPermanents_real = (List<Permanent>)battleHashtable["WinnerPermanents_real"];

                                                        if (WinnerPermanents_real != null)
                                                        {
                                                            if (WinnerPermanents_real.Some((permanent) => permanent != null && permanent.TopCard != null))
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