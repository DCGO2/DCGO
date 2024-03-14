using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can activate [Retaliation]
    public static bool CanActivateRetaliation(Hashtable hashtable)
    {
        List<Hashtable> hashtables = GetHashtablesFromHashtable(hashtable);

        if (hashtables != null)
        {
            foreach (Hashtable hashtable1 in hashtables)
            {
                if (hashtable1 != null)
                {
                    CardSource TopCard = GetTopCardFromOneHashtable(hashtable1);

                    if (TopCard != null)
                    {
                        if (IsByBattle(hashtable))
                        {
                            IBattle battle = GetBattleFromHashtable(hashtable);

                            if (battle != null)
                            {
                                Hashtable battleHashtable = battle.hashtable;

                                if (battleHashtable != null)
                                {
                                    List<Permanent> WinnerPermanents = GetWinnerPermanentsRealFromHashtable(battleHashtable);

                                    if (WinnerPermanents != null)
                                    {
                                        if (WinnerPermanents.Count((permanent) => permanent.cardSources.Contains(TopCard)) == 0)
                                        {
                                            bool canDestroyWinner = WinnerPermanents.Some(permanent => IsOpponentPermanent(permanent, TopCard));

                                            if (canDestroyWinner)
                                            {
                                                List<Permanent> LoserPermanents = GetLoserPermanentsFromHashtable(battleHashtable);

                                                if (LoserPermanents != null)
                                                {
                                                    if (LoserPermanents.Some((permanent) => permanent.cardSources.Contains(TopCard)))
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

        return false;
    }
    #endregion

    #region Effect process of [Retaliation]
    public static IEnumerator RetaliationProcess(Hashtable hashtable, ICardEffect activateClass)
    {
        if (hashtable != null)
        {
            List<Hashtable> hashtables = GetHashtablesFromHashtable(hashtable);

            if (hashtables != null)
            {
                foreach (Hashtable hashtable1 in hashtables)
                {
                    if (hashtable1 != null)
                    {
                        CardSource TopCard = GetTopCardFromOneHashtable(hashtable1);

                        if (TopCard != null)
                        {
                            if (IsByBattle(hashtable))
                            {
                                IBattle battle = GetBattleFromHashtable(hashtable);

                                if (battle != null)
                                {
                                    Hashtable battleHashtable = battle.hashtable;

                                    if (battleHashtable != null)
                                    {
                                        List<Permanent> WinnerPermanents = GetWinnerPermanentsRealFromHashtable(battleHashtable);

                                        if (WinnerPermanents != null)
                                        {
                                            List<Permanent> destroyTargetPermanents = WinnerPermanents.Filter(permanent => IsOpponentPermanent(permanent, TopCard));

                                            if (destroyTargetPermanents.Count >= 1)
                                            {
                                                yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(destroyTargetPermanents, CardEffectHashtable(activateClass)).Destroy());
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
    #endregion

    #region Target 1 Digimon gains [Retaliation]
    public static IEnumerator GainRetaliation(Permanent targetPermanent, EffectDuration effectDuration, ICardEffect activateClass)
    {
        if (targetPermanent == null) yield break;
        if (!IsPermanentExistsOnBattleArea(targetPermanent)) yield break;
        if (activateClass == null) yield break;
        if (activateClass.EffectSourceCard == null) yield break;

        CardSource card = activateClass.EffectSourceCard;

        bool CanUseCondition()
        {
            if (IsPermanentExistsOnBattleArea(targetPermanent))
            {
                if (!targetPermanent.TopCard.CanNotBeAffected(activateClass))
                {
                    return true;
                }
            }

            return false;
        }

        ActivateClass retaliation = CardEffectFactory.RetaliationEffect(targetPermanent: targetPermanent, isInheritedEffect: false, condition: CanUseCondition, rootCardEffect: activateClass, targetPermanent.TopCard);

        AddEffectToPermanent(targetPermanent: targetPermanent, effectDuration: effectDuration, card: card, cardEffect: retaliation, timing: EffectTiming.OnDestroyedAnyone);

        if (!targetPermanent.TopCard.CanNotBeAffected(activateClass))
        {
            yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().CreateBuffEffect(targetPermanent));
        }
    }
    #endregion
}