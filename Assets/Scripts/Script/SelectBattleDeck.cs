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
        // === DCGO-CUSTOM:tournament begin ===
        if (ContinuousController.instance != null && ContinuousController.instance.isTournamentStarted)
        {
            return;
        }
    // === DCGO-CUSTOM:ranked begin ===
    IEnumerator AppendRankToDeckTitleWhenReady(string baseMessage)
    {
        yield return RankedServices.EnsureExists().BootstrapForRanked();
        var profile = RankedServices.Instance != null ? RankedServices.Instance.Profile?.Cached : null;
        if (TitleText != null && profile != null && ContinuousController.instance != null &&
            ContinuousController.instance.isRanked)
        {
            TitleText.text = $"{baseMessage}\n{profile.FormatStatusLine()}";
        }
    }
    // === DCGO-CUSTOM:ranked end ===
        // === DCGO-CUSTOM:tournament begin ===
        else if (ContinuousController.instance.isTournament)
        {
            message = LocalizeUtility.GetLocalizedString(
                EngMessage: "Select Your Deck - Tournament (locked after start)",
                JpnMessage: "使用デッキ選択 - トーナメント（開始後は変更不可）"
                );
        }
        // === DCGO-CUSTOM:tournament end ===
        // === DCGO-CUSTOM:ranked begin ===
        else if (ContinuousController.instance.isRanked)
        {
            message = LocalizeUtility.GetLocalizedString(
                EngMessage: "Select Your Deck - Ranked Match",
                JpnMessage: "使用デッキ選択 - ランクマッチ"
                );

            var profile = RankedServices.Instance != null ? RankedServices.Instance.Profile?.Cached : null;
            if (profile != null)
            {
                message = $"{message}\n{profile.FormatStatusLine()}";
            }
            else
            {
                ContinuousController.instance.StartCoroutine(AppendRankToDeckTitleWhenReady(message));
            }
        }
        // === DCGO-CUSTOM:ranked end ===
        // === DCGO-CUSTOM:tournament end ===
        Opening.instance.deck.editDeck.EndEditAction = () =>
        {
            SetSelectDeckButton();

            if (deckInfoPanel.ShowingDeckData != null)
            {
                InvalidDeckObject.SetActive(!deckInfoPanel.ShowingDeckData.IsValidDeckData());
            }
    // === DCGO-CUSTOM:ranked begin ===
    public void OnClickSelectButton_RankedMatch()
    {
        if (_once || deckInfoPanel.ShowingDeckData == null)
        {
            return;
        }

        ContinuousController.instance.StartCoroutine(SetOnce());

        ContinuousController.instance.BattleDeckData = deckInfoPanel.ShowingDeckData;
        ContinuousController.instance.isRanked = true;
        ContinuousController.instance.isRandomMatch = false;
        ContinuousController.instance.isAI = false;
        ContinuousController.instance.useBanlist = true;

        var rankedLobby = Opening.instance.battle.lobbyManager_RankedMatch;
        if (rankedLobby == null)
        {
            // Auto-create ranked lobby component if not wired in the scene yet
            rankedLobby = Opening.instance.battle.gameObject.GetComponent<LobbyManager_RankedMatch>();
            if (rankedLobby == null)
            {
                rankedLobby = Opening.instance.battle.gameObject.AddComponent<LobbyManager_RankedMatch>();
            }

            Opening.instance.battle.lobbyManager_RankedMatch = rankedLobby;
        }

        rankedLobby.SetUpLobby();
    }
    // === DCGO-CUSTOM:ranked end ===
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
        // === DCGO-CUSTOM:ranked begin ===
        ContinuousController.instance.isRanked = false;
        // === DCGO-CUSTOM:ranked end ===

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