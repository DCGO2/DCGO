using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class ShowPhaseObject : MonoBehaviour
{
    [SerializeField] Sprite OnSprite;
    [SerializeField] Sprite OffSprite;

    [SerializeField] List<PhaseIcon> phaseIcons = new List<PhaseIcon>();

    int count = 0;
    int UpdateFrame = 8;
    async void Start()
    {
        OnSprite = await StreamingAssetsUtility.GetSprite("CurrentPhaseBar_You");
        OffSprite = await StreamingAssetsUtility.GetSprite("CurrentPhaseBar_Opponent");
    }

    private void Update()
    {
        #region ”ƒtƒŒ[ƒ€‚Éˆê“x‚¾‚¯”½‰f
        count++;

        if (count < UpdateFrame)
        {
            return;
        }

        else
        {
            count = 0;
        }
        #endregion

        ShowPhase();
    }

    void ShowPhase()
    {
        bool showPhase = false;

        if (GManager.instance != null)
        {
            if (GManager.instance.turnStateMachine != null)
            {
                if (GManager.instance.turnStateMachine.DoneStartGame)
                {
                    showPhase = true;
                }
            }
        }

        foreach (PhaseIcon phaseIcon in phaseIcons)
        {
            if (!showPhase)
            {
                phaseIcon.image.transform.parent.gameObject.SetActive(false);
            }

            else
            {
                phaseIcon.SetUpPhaseIcon(OnSprite, OffSprite, GManager.instance.turnStateMachine.gameContext.TurnPhase);
            }
        }
    }
}

[Serializable]
public class PhaseIcon
{
    public Image image;
    [SerializeField] GameContext.phase phase;

    public void SetUpPhaseIcon(Sprite OnSprite, Sprite OffSprite, GameContext.phase phase)
    {
        if (image != null)
        {
            if (phase == this.phase)
            {
                if (GManager.instance.turnStateMachine.gameContext.TurnPlayer.isYou)
                {
                    image.sprite = OnSprite;
                }

                else
                {
                    image.sprite = OffSprite;
                }

                image.transform.parent.gameObject.SetActive(true);
            }

            else
            {
                image.transform.parent.gameObject.SetActive(false);
            }
        }
    }
}
