using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Linq;

public class SelectBattleDeck : MonoBehaviour
{
    [Header("Deck selection object")]
    public GameObject SelectDeckObject;

    [Header("Deck Information Tab Prefab")]
    public DeckInfoPrefab deckInfoPrefab;

    [Header("ScrollRect to place deck information tabs")]
    public ScrollRect deckInfoPrefabParentScroll;

    [Header("Deck Information Panel")]
    public DeckInfoPanel deckInfoPanel;

    [Header("Animator")]
    public Animator anim;

    [Header("Deck Information Panel")]
    public Button SelectDeckButton;

    [Header("LoadingObject")]
    public LoadingObject loadingObject;

    [Header("Invalid deck display")]
    public GameObject InvalidDeckObject;

    [Header("タイトルテキスト")]
    public Text TitleText;

    public void OnClickEditDeckButton()
    {
        Opening.instance.deck.editDeck.EndEditAction = () =>
        {
            SetSelectDeckButton();

            if (deckInfoPanel.ShowingDeckData != null)
            {
                InvalidDeckObject.SetActive(!deckInfoPanel.ShowingDeckData.IsValidDeckData());
            }
        };
    }

    public void SetSelectDeckButton()
    {
        SelectDeckButton.interactable = false;

        if (deckInfoPanel.ShowingDeckData != null)
        {
            if (deckInfoPanel.ShowingDeckData.DeckCardIDs != null)
            {
                if (deckInfoPanel.ShowingDeckData.IsValidDeckData())
                {
                    SelectDeckButton.interactable = true;
                }
            }
        }
    }

    bool once = false;
    public void OnClickSelectButton_RandomMatch()
    {
        if (once || deckInfoPanel.ShowingDeckData == null)
        {
            return;
        }

        ContinuousController.instance.StartCoroutine(SetOnce());

        ContinuousController.instance.BattleDeckData = deckInfoPanel.ShowingDeckData;

        Opening.instance.battle.lobbyManager_RandomMatch.SetUpLobby();
    }

    public void OnClickSelectButton_BotMatch()
    {
        if (once || deckInfoPanel.ShowingDeckData == null)
        {
            return;
        }

        ContinuousController.instance.StartCoroutine(SetOnce());

        ContinuousController.instance.BattleDeckData = deckInfoPanel.ShowingDeckData;
    }

    public IEnumerator OnClickSelectButton_RoomMatchCoroutine()
    {
        if (once || deckInfoPanel.ShowingDeckData == null)
        {
            yield break;
        }

        ContinuousController.instance.StartCoroutine(SetOnce());

        ContinuousController.instance.BattleDeckData = deckInfoPanel.ShowingDeckData;

        Off();

        yield return ContinuousController.instance.StartCoroutine(PhotonUtility.SignUpBattleDeckData());
    }

    IEnumerator SetOnce()
    {
        once = true;
        yield return new WaitForSeconds(1f);
        once = false;
    }

    public void Off()
    {
        if (this.gameObject.activeSelf)
        {
            this.gameObject.SetActive(false);
            OnCloseSelectBattleDeckAction?.Invoke();
        }

        Opening.instance.OffYesNoObjects();

        Opening.instance.deck.trialDraw.Close();

        Opening.instance.deck.deckListPanel.Close();
    }

    public UnityAction OnCloseSelectBattleDeckAction;

    public async void SetUpSelectBattleDeck(UnityAction OnClickSelectButtonAction, int defaulSelectDeckIndex)
    {
        if (SelectDeckObject.activeSelf)
        {
            return;
        }

        OnCloseSelectBattleDeckAction = null;

        //ContinuousController.instance.ModifyAllDeckDatas();

        SelectDeckObject.SetActive(true);

        anim.SetInteger("Open", 1);
        anim.SetInteger("Close", 0);

        ContinuousController.instance.StartCoroutine(SetDeckList(true));

        deckInfoPanel.OnClickSelectDeckAction = OnClickSelectButtonAction;

        if (ContinuousController.instance.DeckDatas.Count > 0)
        {
            if (ContinuousController.instance.LastBattleDeckData != null
            && ContinuousController.instance.DeckDatas.Contains(ContinuousController.instance.LastBattleDeckData))
            {
                await deckInfoPanel.SetUpDeckInfoPanel(ContinuousController.instance.LastBattleDeckData);
            }

            else
            {
                await deckInfoPanel.SetUpDeckInfoPanel(ContinuousController.instance.DeckDatas[0]);
            }
        }

        else
        {
            ResetDeckInfoPanel();
        }

        /*
        if (0 <= defaulSelectDeckIndex && defaulSelectDeckIndex <= ContinuousController.instance.DeckDatas.Count - 1)
        {
            await deckInfoPanel.SetUpDeckInfoPanel(ContinuousController.instance.DeckDatas[defaulSelectDeckIndex]);
        }
        */

        string message = "";

        if (ContinuousController.instance.isAI)
        {
            message = LocalizeUtility.GetLocalizedString(
                EngMessage: "Select Your Deck - Bot Match",
                JpnMessage: "使用デッキ選択 - Bot戦"
                );
        }

        else if (ContinuousController.instance.isRandomMatch)
        {
            message = LocalizeUtility.GetLocalizedString(
                EngMessage: "Select Your Deck - Random Match",
                JpnMessage: "使用デッキ選択 - ランダムマッチ"
                );
        }

        else
        {
            message = LocalizeUtility.GetLocalizedString(
                EngMessage: "Select Your Deck - Room Match",
                JpnMessage: "使用デッキ選択 - ルームマッチ"
                );
        }

        TitleText.text = message;

        SetSelectDeckButton();

        if (deckInfoPanel.ShowingDeckData != null)
        {
            InvalidDeckObject.SetActive(!deckInfoPanel.ShowingDeckData.IsValidDeckData());
        }
    }

    public void Close()
    {
        Close_(true);
    }

    public void Close_(bool playSE)
    {
        if (playSE)
        {
            Opening.instance.PlayCancelSE();
        }

        anim.SetInteger("Open", 0);
        anim.SetInteger("Close", 1);
    }

    public void ResetDeckInfoPanel()
    {
        deckInfoPanel.SetUpDeckInfoPanel(null);
    }

    public IEnumerator SetDeckList(bool open)
    {
        for (int i = 0; i < deckInfoPrefabParentScroll.content.childCount; i++)
        {
            Destroy(deckInfoPrefabParentScroll.content.GetChild(i).gameObject);
        }

        for (int i = 0; i < ContinuousController.instance.DeckDatas.Count; i++)
        {
            DeckInfoPrefab _deckInfoPrefab = Instantiate(deckInfoPrefab, deckInfoPrefabParentScroll.content);

            _deckInfoPrefab.scrollRect.content = deckInfoPrefabParentScroll.content;

            _deckInfoPrefab.scrollRect.viewport = deckInfoPrefabParentScroll.viewport;

            _deckInfoPrefab.scrollRect.verticalScrollbar = deckInfoPrefabParentScroll.verticalScrollbar;

            _deckInfoPrefab.SetUpDeckInfoPrefab(ContinuousController.instance.DeckDatas[i]);

            _deckInfoPrefab.transform.localScale = Opening.instance.DeckInfoPrefabStartScale * 1.02f;

            _deckInfoPrefab.OnClickAction = (deckdata) =>
            {
                deckInfoPanel.SetUpDeckInfoPanel(deckdata);

                SetSelectDeckButton();

                if (deckInfoPanel.ShowingDeckData != null)
                {
                    InvalidDeckObject.SetActive(!deckInfoPanel.ShowingDeckData.IsValidDeckData());
                }

                Opening.instance.CreateOnClickEffect();
            };
        }

        yield return null;

        for (int i = 0; i < deckInfoPrefabParentScroll.content.childCount; i++)
        {
            deckInfoPrefabParentScroll.content.GetChild(i).transform.localScale = Opening.instance.DeckInfoPrefabStartScale;
        }

        if (ContinuousController.instance.DeckDatas.Count == 0)
        {

        }

        else
        {
            for (int i = 0; i < deckInfoPrefabParentScroll.content.childCount; i++)
            {
                if (deckInfoPrefabParentScroll.content.GetChild(i).GetComponent<DeckInfoPrefab>() != null)
                {
                    if (deckInfoPrefabParentScroll.content.GetChild(i).GetComponent<DeckInfoPrefab>().thisDeckData == deckInfoPanel.ShowingDeckData && deckInfoPanel.DeckInfoPanelObject.activeSelf)
                    {
                        deckInfoPrefabParentScroll.content.GetChild(i).GetComponent<DeckInfoPrefab>().Outline.SetActive(true);
                    }

                    else
                    {
                        deckInfoPrefabParentScroll.content.GetChild(i).GetComponent<DeckInfoPrefab>().Outline.SetActive(false);
                    }
                }
            }
        }

        if (open)
        {
            yield return new WaitForSeconds(Time.deltaTime);
            deckInfoPrefabParentScroll.verticalNormalizedPosition = 1;
        }
    }
}
