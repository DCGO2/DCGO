using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class PlayLog : MonoBehaviour
{
    [SerializeField]
    TMP_Text _logText;

    [SerializeField]
    ScrollRect _scroll;

    #region Events
    public static Action<string> OnAddLog;
    public static Action<string> OnLinkPressed;
    #endregion

    private void OnDestroy()
    {
        OnAddLog -= AddLogString;
        OnLinkPressed -= ShowCard;
    }

    string GetLogString()
    {
        string logString = "";

        foreach (string log in _logList)
        {
            logString += log;
        }

        return logString;
    }

    List<string> _logList = new List<string>();

    //16250, 13000
    int _maxLogCharacterLength = 11000;

    #region Export log
    //The whole log of the game without links, as _logList is trimmed to _maxLogCharacterLength
    List<string> _fullLogList = new List<string>();

    //Height taken from the bottom of the log to make room for the button
    const float ExportLogButtonAreaHeight = 60f;
    static readonly Vector2 ExportLogButtonSize = new Vector2(280f, 50f);
    static readonly Color ExportLogButtonColor = new Color(0.12f, 0.16f, 0.24f, 1f);

    TMP_Text _exportLogButtonText;

    string ExportLogButtonLabel => LocalizeUtility.GetLocalizedString(
        EngMessage: "Export Log",
        JpnMessage: "ログをコピー");

    //The button is built from code, so no scene or prefab has to be changed
    void CreateExportLogButton()
    {
        if (_exportLogButtonText != null)
        {
            return;
        }

        RectTransform scrollRect = _scroll.GetComponent<RectTransform>();

        scrollRect.sizeDelta -= new Vector2(0f, ExportLogButtonAreaHeight);
        scrollRect.anchoredPosition += new Vector2(0f, ExportLogButtonAreaHeight / 2f);

        GameObject buttonObject = new GameObject("ExportLogButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.layer = gameObject.layer;

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.SetParent(transform, false);
        buttonRect.anchorMin = scrollRect.anchorMin;
        buttonRect.anchorMax = scrollRect.anchorMax;
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.sizeDelta = ExportLogButtonSize;
        buttonRect.anchoredPosition = new Vector2(
            scrollRect.anchoredPosition.x,
            scrollRect.anchoredPosition.y - scrollRect.sizeDelta.y / 2f - ExportLogButtonAreaHeight / 2f);

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = ExportLogButtonColor;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = buttonImage;
        button.onClick.AddListener(OnClickExportLogButton);

        GameObject textObject = new GameObject("Text", typeof(RectTransform));
        textObject.layer = gameObject.layer;

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.SetParent(buttonRect, false);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI buttonText = textObject.AddComponent<TextMeshProUGUI>();
        buttonText.font = _logText.font;
        buttonText.fontSize = 30;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.white;
        buttonText.raycastTarget = false;
        buttonText.text = ExportLogButtonLabel;

        _exportLogButtonText = buttonText;
    }

    public void OnClickExportLogButton()
    {
        GUIUtility.systemCopyBuffer = string.Concat(_fullLogList).Trim();

        if (GManager.instance != null)
        {
            GManager.instance.PlayDecisionSE();
        }

        _exportLogButtonText.text = LocalizeUtility.GetLocalizedString(
            EngMessage: "Copied to clipboard!",
            JpnMessage: "コピーしました!");

        StopCoroutine(nameof(ResetExportLogButtonLabel));
        StartCoroutine(nameof(ResetExportLogButtonLabel));
    }

    IEnumerator ResetExportLogButtonLabel()
    {
        yield return new WaitForSecondsRealtime(1.5f);

        _exportLogButtonText.text = ExportLogButtonLabel;
    }

    private void OnEnable()
    {
        if (_exportLogButtonText != null)
        {
            _exportLogButtonText.text = ExportLogButtonLabel;
        }
    }
    #endregion

    public void OnClickLiogButton()
    {
        if (gameObject.activeSelf)
        {
            OffPlayLog();
        }

        else
        {
            SetUpPlayLog();
        }
    }

    public void SetUpPlayLog()
    {
        ContinuousController.instance.StartCoroutine(SetUpPlayLogCoroutine());
    }

    IEnumerator SetUpPlayLogCoroutine()
    {
        this.gameObject.SetActive(true);

        if (Opening.instance != null)
        {
            Opening.instance.PlayDecisionSE();
        }

        else if (GManager.instance != null)
        {
            GManager.instance.PlayDecisionSE();
        }

        _logText.text = GetLogString();

        _scroll.content.GetComponent<ContentSizeFitter>().SetLayoutVertical();

        yield return new WaitForSeconds(Time.deltaTime);

        _scroll.verticalNormalizedPosition = 0;
    }

    bool _first = false;

    public void OffPlayLog()
    {
        if (_first)
        {
            if (Opening.instance != null)
            {
                Opening.instance.PlayCancelSE();
            }

            else if (GManager.instance != null)
            {
                GManager.instance.PlayCancelSE();
            }
        }

        _first = true;

        gameObject.SetActive(false);
    }

    public void Init()
    {
        OffPlayLog();

        _logText.text = "";

        _logList = new List<string>();

        _fullLogList = new List<string>();

        CreateExportLogButton();

        OnAddLog += AddLogString;
        OnLinkPressed += ShowCard;
    }

    public void AddLogString(string logText)
    {
        logText = DataBase.ReplaceToASCII(logText);

        _fullLogList.Add(logText);

        AddLogStringCoroutine(logText);
    }

    void AddLogStringCoroutine(string log)
    {
        _logList.Add(AddLink(log));

        while (GetLogString().Length >= _maxLogCharacterLength)
        {
            if (_logList.Count >= 1)
            {
                _logList.RemoveAt(0);
            }
        }

        _logText.text = GetLogString();
    }

    string AddLink(string log)
    {
        List<int> startIndex = AllIndexesOf(log, "(");
        List<int> endIndex = AllIndexesOf(log, ")");
        List<string> subStrings = new List<string>();

        for(int i = 0; i < startIndex.Count; i++)
        {
            if (startIndex[i] < 0 && endIndex[i] < 0)
                continue;

            startIndex[i] += 1;

            string str = log.Substring(startIndex[i], endIndex[i] - startIndex[i]);

            if(!subStrings.Contains(str) && !String.IsNullOrEmpty(str))
                subStrings.Add(str);
        }

        foreach(string str in subStrings)
            log = log.Replace(str, $"<link={str}><color=#92F6FF><u>{str}</u></color></link>");

        return log;
    }

    void ShowCard(string cardID)
    {
        CardSource founcdCardSource = GManager.instance.turnStateMachine.gameContext.ActiveCardList
        .Find(cardSource1 => cardSource1.CardID == cardID);

        if (founcdCardSource != null)
        {
            GManager.instance.cardDetail.OpenCardDetail(founcdCardSource, true);
        }
    }

    //Might need to move this more relavent
    List<int> AllIndexesOf(string str, string value)
    {
        if (String.IsNullOrEmpty(value))
            throw new ArgumentException("the string to find may not be empty", "value");
        List<int> indexes = new List<int>();
        for (int index = 0; ; index += value.Length)
        {
            index = str.IndexOf(value, index);
            if (index == -1)
                return indexes;
            indexes.Add(index);
        }
    }
}
