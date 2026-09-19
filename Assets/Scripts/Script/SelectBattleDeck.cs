using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Threading.Tasks;

public class SelectBattleDeck : MonoBehaviour
{
    private static readonly int OpenHash = Animator.StringToHash("Open");
    private static readonly int CloseHash = Animator.StringToHash("Close");
    private static WaitForSeconds _waitForSeconds1 = new WaitForSeconds(1f);
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

    bool _once = false;
    public void OnClickSelectButton_RandomMatch()
    {
        if (_once || deckInfoPanel.ShowingDeckData == null)
        {
            return;
        }

        ContinuousController.instance.StartCoroutine(SetOnce());

        ContinuousController.instance.BattleDeckData = deckInfoPanel.ShowingDeckData;

        Opening.instance.battle.lobbyManager_RandomMatch.SetUpLobby();
    }

    public void OnClickSelectButton_BotMatch()
    {
        if (_once || deckInfoPanel.ShowingDeckData == null)
        {
            return;
        }

        ContinuousController.instance.StartCoroutine(SetOnce());

        ContinuousController.instance.BattleDeckData = deckInfoPanel.ShowingDeckData;
    }

    public IEnumerator OnClickSelectButton_RoomMatchCoroutine()
    {
        if (_once || deckInfoPanel.ShowingDeckData == null)
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
        _once = true;
        yield return _waitForSeconds1;
        _once = false;
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

    public async void SetUpSelectBattleDeck(UnityAction OnClickSelectButtonAction, int _)
    {
        if (SelectDeckObject.activeSelf)
        {
            return;
        }

        OnCloseSelectBattleDeckAction = null;

        //ContinuousController.instance.ModifyAllDeckDatas();

        SelectDeckObject.SetActive(true);

        anim.SafeSetInt(OpenHash, 1);
        anim.SafeSetInt(CloseHash, 0);

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
            await ResetDeckInfoPanel();
        }

        /*
        if (0 <= defaulSelectDeckIndex && defaulSelectDeckIndex <= ContinuousController.instance.DeckDatas.Count - 1)
        {
            await deckInfoPanel.SetUpDeckInfoPanel(ContinuousController.instance.DeckDatas[defaulSelectDeckIndex]);
        }
        */

        string message;

        if (ContinuousController.instance.isAI)
        {
            message = LocalizeUtility.GetLocalizedString(
                EngMessage: "WARNING: THE BOT MAKES ILLEGAL PLAYS",
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

        anim.SafeSetInt(OpenHash, 0);
        anim.SafeSetInt(CloseHash, 1);
    }

    public async Task ResetDeckInfoPanel()
    {
        await deckInfoPanel.SetUpDeckInfoPanel(null);
    }

    public IEnumerator SetDeckList(bool open)
    {
        for (int i = 0; i < deckInfoPrefabParentScroll.content.childCount; i++)
        {
            if (i > 0)
            {
                Destroy(deckInfoPrefabParentScroll.content.GetChild(i).gameObject);
            }
        }

        for (int i = 0; i < ContinuousController.instance.DeckDatas.Count; i++)
        {
            DeckInfoPrefab _deckInfoPrefab = Instantiate(deckInfoPrefab, deckInfoPrefabParentScroll.content);

            _deckInfoPrefab.scrollRect.content = deckInfoPrefabParentScroll.content;

            _deckInfoPrefab.scrollRect.viewport = deckInfoPrefabParentScroll.viewport;

            _deckInfoPrefab.scrollRect.verticalScrollbar = deckInfoPrefabParentScroll.verticalScrollbar;

            _deckInfoPrefab.SetUpDeckInfoPrefab(ContinuousController.instance.DeckDatas[i]);

            _deckInfoPrefab.transform.localScale = Opening.instance.DeckInfoPrefabStartScale * 1.02f;

            _deckInfoPrefab.OnClickAction = async (deckdata) =>
            {
                await deckInfoPanel.SetUpDeckInfoPanel(deckdata);

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
            for (int i = 0; i < deckInfoPrefabParentScroll.content.childCount; i++)
            {
                if (deckInfoPrefabParentScroll.content.GetChild(i).GetComponent<SelectRandomDeckButton>() != null)
                {
                    deckInfoPrefabParentScroll.content.GetChild(i).GetComponent<SelectRandomDeckButton>().Outline.SetActive(true);
                    break;
                }
            }
        }

        else
        {
            for (int i = 0; i < deckInfoPrefabParentScroll.content.childCount; i++)
            {
                if (deckInfoPrefabParentScroll.content.GetChild(i).GetComponent<SelectRandomDeckButton>() != null)
                {
                    deckInfoPrefabParentScroll.content.GetChild(i).GetComponent<SelectRandomDeckButton>().Outline.SetActive(false);
                    break;
                }
            }

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