using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ShowPhaseNotificationObject : MonoBehaviour
{
    public Text PhaseText;

    public bool isClose { get; set; }

    public void Init()
    {
        Off();
    }

    public void ShowPhase(GameContext.phase phase)
    {
        switch(phase)
        {
            case GameContext.phase.Breeding:
                PhaseText.text = "Breeding Phase";
                break;

            case GameContext.phase.Main:
                PhaseText.text = "Main Phase";
                break;
        }

        isClose = false;

        this.gameObject.SetActive(true);

        StartCoroutine(CloseCoroutine());
    }

    IEnumerator CloseCoroutine()
    {
        GManager.instance.turnStateMachine.IsSelecting = true;

        GetComponent<Animator>().SetInteger("Close", 0);

        yield return new WaitForSeconds(0.3f);

        GetComponent<Animator>().SetInteger("Close", 1);

        GManager.instance.turnStateMachine.IsSelecting = false;
    }

    public void Off()
    {
        this.gameObject.SetActive(false);
        isClose = true;
    }
}