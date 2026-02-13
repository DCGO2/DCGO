using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CutInProcess : MonoBehaviourPunCallbacks
{
    public IEnumerator CutInProcessCoroutine()
    {
        bool DoEndGameProcess(Player player)
        {
            if (player.IsLose && !GManager.instance.turnStateMachine.endGame)
            {
                return true;
            }

            return false;

        }

        bool DoAddTrashDigitamaProcess(Player player)
        {
            foreach (Permanent permanent in player.GetBattleAreaPermanents())
            {
                if (permanent.DP <= 0 && permanent.IsDigimon)
                {
                    return true;
                }
            }

            return false;
        }

        bool DoDPZeroProcess(Player player)
        {
            foreach (Permanent permanent in player.GetBattleAreaPermanents())
            {
                if (permanent.DP == 0 && permanent.IsDigimon)
                {
                    return true;
                }
            }

            return false;
        }

        while (GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer.Count((player) => DoAddTrashDigitamaProcess(player) || DoDPZeroProcess(player) || DoEndGameProcess(player)) > 0)
        {
            foreach (Player player in GManager.instance.turnStateMachine.gameContext.Players_ForTurnPlayer)
            {
                if (DoEndGameProcess(player))
                {
                    GManager.instance.turnStateMachine.EndGame(player.Enemy, false);
                    yield break;
                }

                while (DoAddTrashDigitamaProcess(player))
                {
                    List<Permanent> LackPowerCharacters = player.GetFieldPermanents().Clone().Filter(permanent => permanent.DP <= 0 && permanent.IsDigimon);

                    if (LackPowerCharacters.Count >= 1)
                    {
                        foreach (Permanent permanent in LackPowerCharacters)
                        {
                            if (permanent != null)
                            {
                                if (permanent.TopCard != null)
                                {
                                    yield return ContinuousController.instance.StartCoroutine(permanent.DiscardEvoRoots());

                                    CardSource cardSource = permanent.TopCard;

                                    yield return ContinuousController.instance.StartCoroutine(GManager.instance.GetComponent<Effects>().ShowCardEffect(new List<CardSource>() { cardSource }, "Cards put to trash", true, true));

                                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.RemoveField(permanent));
                                    yield return ContinuousController.instance.StartCoroutine(CardObjectController.AddTrashCard(cardSource));
                                }
                            }
                        }
                    }
                }

                while (DoDPZeroProcess(player))
                {
                    List<Permanent> LackPowerCharacters = new List<Permanent>();

                    foreach (Permanent permanent in player.GetFieldPermanents())
                    {
                        if (permanent.DP == 0 && permanent.IsDigimon)
                        {
                            LackPowerCharacters.Add(permanent);
                        }
                    }

                    if (LackPowerCharacters.Count >= 1)
                    {
                        foreach (Permanent permanent in LackPowerCharacters)
                        {
                            yield return ContinuousController.instance.StartCoroutine(new DestroyPermanentsClass(new List<Permanent>() { permanent }, null).Destroy());
                        }
                    }
                }
            }
        }
    }
}
