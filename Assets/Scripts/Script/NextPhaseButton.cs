using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Photon.Pun;
using System;
using Photon;
using TMPro;

public class NextPhaseButton : MonoBehaviourPunCallbacks
{
    [Header("ボタンテキスト")]
    public TextMeshProUGUI ButtonText;

    [Header("ボタン")]
    public Button Button;

    Image ButtonImage;

    public GameObject Outline;

    public GameObject Cover;

    public Sprite MyTurnSprite;
    public Sprite OpponentTurnSprite;

    private void Awake()
    {
        if (Cover != null)
        {
            Cover.SetActive(false);

            Button.gameObject.SetActive(false);
            Outline.SetActive(false);
        }

        ButtonImage = Button.GetComponent<Image>();
    }

    public void OnClick()
    {
        if (!GManager.instance.turnStateMachine.isSync && !GManager.instance.turnStateMachine.IsSelecting && !GManager.instance.turnStateMachine.isExecuting && !GManager.instance.turnStateMachine.isSecurityCehck)
        {
            if (GManager.instance.turnStateMachine.gameContext.TurnPlayer.isYou)
            {
                if (GManager.instance.turnStateMachine.gameContext.TurnPhase == GameContext.phase.Breeding || GManager.instance.turnStateMachine.gameContext.TurnPhase == GameContext.phase.Main)
                {
                    GManager.instance.turnStateMachine.isSync = true;
                    photonView.RPC("NextPhase", RpcTarget.All);
                    StartCoroutine(CoverCoroutine(1.5f));

                    Button.interactable = false;

                    SwitchTurnSprite();

                    Button.interactable = true;
                }
            }
        }
    }

    public IEnumerator CoverCoroutine(float waitTime)
    {
        if (Cover != null)
        {
            Cover.SetActive(true);

            yield return new WaitForSeconds(waitTime);

            Cover.SetActive(false);
        }
    }

    bool oldActive = false;
    private void Update()
    {
        oldActive = Button.gameObject.activeSelf;

        bool active = false;

        if (GManager.instance != null)
        {
            if (GManager.instance.turnStateMachine != null)
            {
                if (GManager.instance.turnStateMachine.endGame || !GManager.instance.turnStateMachine.DoneStartGame)
                {
                    return;
                }

                if (GManager.instance.turnStateMachine.gameContext != null)
                {
                    if (GManager.instance.turnStateMachine.gameContext.TurnPlayer != null)
                    {
                        if (GManager.instance.turnStateMachine.gameContext.TurnPlayer.isYou)
                        {
                            if (!GManager.instance.attackProcess.IsAttacking && !GManager.instance.turnStateMachine.isSync && !GManager.instance.turnStateMachine.IsSelecting && !GManager.instance.turnStateMachine.isExecuting && !GManager.instance.turnStateMachine.isSecurityCehck)
                            {
                                switch (GManager.instance.turnStateMachine.gameContext.TurnPhase)
                                {
                                    case GameContext.phase.Breeding:
                                        active = true;
                                        ButtonText.text = LocalizeUtility.GetLocalizedString(
                                        EngMessage: "End\nBreeding",
                                        JpnMessage: "育成\n終了"
                                        );
                                        break;

                                    case GameContext.phase.Main:
                                        active = true;
                                        ButtonText.text = LocalizeUtility.GetLocalizedString(
                                        EngMessage: "End\nTurn",
                                        JpnMessage: "ターン\n終了"
                                        );
                                        break;
                                }
                            }
                        }

                        else
                        {
                            active = true;
                            ButtonText.text = LocalizeUtility.GetLocalizedString(
                                        EngMessage: "Opponent's\nTurn",
                                        JpnMessage: "相手\nターン"
                                        );
                        }
                    }
                }

            }

        }

        if (!active)
        {
            Button.gameObject.SetActive(false);
            Outline.SetActive(false);
        }

        else
        {
            Button.gameObject.SetActive(true);

            if (GManager.instance.turnStateMachine.gameContext.TurnPlayer.isYou)
            {
                Button.enabled = true;
                Outline.SetActive(true);

                if (!oldActive)
                {
                    StartCoroutine(CoverCoroutine(0.3f));
                }
            }

            else
            {
                Button.enabled = false;
                Outline.SetActive(false);

                Cover.SetActive(true);
            }
        }
    }

    [PunRPC]
    public void NextPhase()
    {
        GManager.instance.turnStateMachine.NextPhase();
    }

    public void SwitchTurnSprite()
    {
        if (GManager.instance.turnStateMachine.gameContext.TurnPlayer.isYou)
        {
            SetMyTurnSprite();
        }

        else
        {
            EnemyTurnSprite();
        }

        //Button.gameObject.SetActive(false);
        //Outline.gameObject.SetActive(false);
    }

    public void SetMyTurnSprite()
    {
        ButtonImage.sprite = MyTurnSprite;
    }

    public void EnemyTurnSprite()
    {
        ButtonImage.sprite = OpponentTurnSprite;
    }
}
