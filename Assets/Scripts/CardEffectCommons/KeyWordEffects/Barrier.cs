using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public partial class CardEffectCommons
{
    #region Can activate [Barrier]
    public static bool CanActivateBarrier(Permanent permanent)
    {
        if (IsPermanentExistsOnBattleArea(permanent))
        {
            if (permanent.TopCard.Owner.SecurityCards.Count >= 1)
            {
                return true;
            }
        }

        return false;
    }
    #endregion

    #region Effect process of [Barrier]
    public static IEnumerator BarrierProcess(Permanent permanent, ICardEffect activateClass)
    {
        yield return ContinuousController.instance.StartCoroutine(new BarrierClass(permanent, activateClass).Barrier());
    }
    #endregion

    #region Barrier class
    public class BarrierClass
    {
        public BarrierClass(Permanent permanent, ICardEffect cardEffect)
        {
            _permanent = permanent;
            _cardEffect = cardEffect;
        }

        Permanent _permanent = null;
        ICardEffect _cardEffect = null;

        public IEnumerator Barrier()
        {
            if (_permanent != null)
            {
                if (_permanent.TopCard != null)
                {
                    CardSource topCard = _permanent.TopCard;

                    if (topCard.Owner.SecurityCards.Count >= 1)
                    {
                        _permanent.ShowDeleteEffect();

                        yield return ContinuousController.instance.StartCoroutine(new IDestroySecurity(
                                                player: topCard.Owner,
                                                destroySecurityCount: 1,
                                                cardEffect: _cardEffect,
                                                fromTop: true).DestroySecurity());

                        _permanent.willBeRemoveField = false;

                        _permanent.HideDeleteEffect();

                        #region log
                        string log = "";

                        log += $"\nBarrier :";

                        log += $"\n{topCard.BaseENGCardNameFromEntity}({topCard.CardID})";

                        log += "\n";

                        GManager.instance.playLog.AddLogString(log);
                        #endregion
                    }
                }
            }
        }
    }
    #endregion
}