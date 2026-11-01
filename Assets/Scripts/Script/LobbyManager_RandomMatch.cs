using DCGO.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager_RandomMatch : MonoBehaviour
{
    private static readonly int CloseHash = Animator.StringToHash("Close");
    private static readonly int OpenHash = Animator.StringToHash("Open");

    [Header("message text")]
    public Text MessageText;

    [Header("time count text")]
    public Text TimeText;

    [Header("return to title button")]
    public GameObject ReturnButton;

    [Header("deck info panel")]
    public DeckInfoPanel deckInfoPanel;

    public Animator anim;

    public LoadingObject loadingObject;

    public LoadingObject disconnectLoadingObject;

    Button _returnButton = null;
    Coroutine _waitingTextCoroutine;

    private void Start()
    {
        _returnButton = ReturnButton.transform.GetChild(0).GetComponent<Button>();
    }
    public void StartMatchmaking()
    {
        Opening.instance.OffYesNoObjects();

        Opening.instance.deck.trialDraw.Close();

        Opening.instance.deck.deckListPanel.Close();

        ContinuousController.instance.isAI = false;
        ContinuousController.instance.isRandomMatch = true;
        this.gameObject.SetActive(true);

        if (ContinuousController.instance.BattleDeckData != null)
        {
            _ = deckInfoPanel.SetUpDeckInfoPanel(
                ContinuousController.instance.BattleDeckData
            );
        }

        StartCoroutine(TimeCountUp());

        StateConnecting();

        MatchmakingEvents MatchmakingEvents = new MatchmakingEvents();
        MatchmakingEvents.OnConnected += HandleOnConnected;
        MatchmakingEvents.OnMatchFound += StateMatchFound;
        MatchmakingEvents.OnCanceled += OnMatchmakingCanceled;
        MatchmakingEvents.OnDisconnected += OnMatchmakingCanceled;

        DCGONetwork.Provider.StartMatchmaking(MatchmakingEvents);

        anim.SetInteger(OpenHash, 1);
        anim.SetInteger(CloseHash, 0);
    }

    public void StateConnecting()
    {
        ReturnButton.SetActive(true);

        SetWaitingText(LocalizeUtility.GetLocalizedString(
            EngMessage: "Connecting",
            JpnMessage: "接続中"
        ), 3);

        MessageText.transform.localPosition = new Vector3(-180, -193, 0);
    }

    void HandleOnConnected(bool success)
    {
        if (success)
        {
            StateMatching();
        }
        else
        {
            CloseSelf();
        }
    }

    void StateMatching()
    {
        ReturnButton.SetActive(true);

        SetWaitingText(LocalizeUtility.GetLocalizedString(
            EngMessage: "Matching",
            JpnMessage: "マッチング中"
        ), 3);

        MessageText.transform.localPosition = new Vector3(-148, -193, 0);
    }
    void StateMatchFound()
    {
        ReturnButton.SetActive(false);

        SetWaitingText(LocalizeUtility.GetLocalizedString(
            EngMessage: "Matching completed!",
            JpnMessage: "マッチングしました!"
        ));

        MessageText.transform.localPosition = new Vector3(-390, -193, 0);

        StartCoroutine(GoToBattleSceneCoroutine());
    }

    IEnumerator GoToBattleSceneCoroutine()
    {
        ContinuousController.instance.StartCoroutine(Opening.instance.OpeningBGM.FadeOut(0.2f));

        Debug.Log("Matching completed!");

        yield return new WaitForSeconds(0.1f);

        //Opening.instance.MainCamera.gameObject.SetActive(false);

        foreach (Camera camera in Opening.instance.openingCameras)
        {
            camera.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(0.1f);
        SceneManager.LoadSceneAsync("BattleScene", LoadSceneMode.Additive);
        yield return null;
    }

    public void StopMatchmaking()
    {
        ContinuousController.instance.StartCoroutine(CloseLobbyCoroutine());
    }
    public IEnumerator CloseLobbyCoroutine()
    {
        yield return ContinuousController.instance.StartCoroutine(disconnectLoadingObject.StartLoading("Now Loading"));
        ReturnButton.SetActive(false);

        DCGONetwork.Provider.CancelMatchmaking();
    }

    public void OnMatchmakingCanceled()
    {
        ContinuousController.instance.StartCoroutine(disconnectLoadingObject.StartLoading("Now Loading"));
        Close();
        ContinuousController.instance.StartCoroutine(disconnectLoadingObject.EndLoading());
    }

    public void Close()
    {
        this.gameObject.SetActive(false);
    }

    void CloseSelf()
    {
        gameObject.SetActive(false);
    }

    void SetWaitingText(string WaitingText, int EllipsisCount = 0)
    {
        if (_waitingTextCoroutine != null)
        {
            StopCoroutine(_waitingTextCoroutine);
        }

        if (EllipsisCount > 0)
        {
            _waitingTextCoroutine = StartCoroutine(StartWaitingText(WaitingText, EllipsisCount));
        }
        else
        {
            MessageText.text = WaitingText;
        }
    }

    private IEnumerator StartWaitingText(string defaultString, int EllipsisCount)
    {
        float waitTime = 0.18f;
        int count = 0;

        while (true)
        {
            count++;

            if (count > EllipsisCount)
            {
                count = 0;
            }

            MessageText.text = defaultString;

            for (int i = 0; i < count; i++)
            {
                MessageText.text += ".";
            }

            yield return new WaitForSeconds(waitTime);
        }
    }

    #region Time text count up
    int time = 0;

    IEnumerator TimeCountUp()
    {
        time = 0;
        TimeText.gameObject.SetActive(true);

        while (gameObject.activeSelf)
        {
            string min = (time / 60).ToString();
            string sec = (time % 60).ToString();

            if (min.Length == 1)
            {
                min = $"0{min}";
            }

            if (sec.Length == 1)
            {
                sec = $"0{sec}";
            }

            TimeText.text = $"{min}:{sec}";

            time++;

            yield return new WaitForSeconds(1.0f);
        }

        TimeText.gameObject.SetActive(false);
    }
    #endregion
}
