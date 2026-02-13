using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon;
using System;
using Photon.Pun;
public class BT4_060 : CEntity_Effect
{
    public override List<ICardEffect> CardEffects(EffectTiming timing, CardSource card)
    {
        List<ICardEffect> cardEffects = new List<ICardEffect>();

        if (timing == EffectTiming.OnEnterFieldAnyone)
        {
            ActivateClass activateClass = new ActivateClass();
            activateClass.SetUpICardEffect("Suspend the played Digimon", CanUseCondition, card);
            activateClass.SetUpActivateClass(CanActivateCondition, ActivateCoroutine, -1, false, EffectDiscription());
            cardEffects.Add(activateClass);

            string EffectDiscription()
            {
                return "[All Turns] When you or your opponent play a level 4 or lower Digimon, suspend it.";
            }

            bool PermanentCondition(Permanent permanent)
            {
                if (CardEffectCommons.IsPermanentExistsOnBattleArea(permanent))
                {
                    if (permanent.IsDigimon)
                    {
                        if (permanent.Level <= 4)
                        {
                            if (permanent.TopCard.HasLevel)
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }

            bool CanUseCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    if (CardEffectCommons.CanTriggerOnPermanentPlay(hashtable, PermanentCondition))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CanActivateCondition(Hashtable hashtable)
            {
                if (CardEffectCommons.IsExistOnBattleArea(card))
                {
                    List<Permanent> permanents = CardEffectCommons.GetPlayedPermanentsFromEnterFieldHashtable(
                    hashtable: hashtable,
                    rootCondition: null);

                    if (permanents != null)
                    {
                        if (permanents.Count(permanent => CardEffectCommons.IsPermanentExistsOnBattleArea(permanent)
                            && !permanent.TopCard.CanNotBeAffected(activateClass)
                            && !permanent.IsSuspended && permanent.CanSuspend) >= 1)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            IEnumerator ActivateCoroutine(Hashtable _hashtable)
            {
                List<Permanent> permanents = CardEffectCommons.GetPlayedPermanentsFromEnterFieldHashtable(
                    hashtable: _hashtable,
                    rootCondition: null);

                if (permanents != null)
                {
                    List<Permanent> suspendTargetPermanents = permanents
                    .Filter(permanent => CardEffectCommons.IsPermanentExistsOnBattleArea(permanent)
                        && !permanent.TopCard.CanNotBeAffected(activateClass)
                        && !permanent.IsSuspended && permanent.CanSuspend);

                    yield return ContinuousController.instance.StartCoroutine(new SuspendPermanentsClass(suspendTargetPermanents, CardEffectCommons.CardEffectHashtable(activateClass)).Tap());
                }
            }
        }

        return cardEffects;
    }
}
