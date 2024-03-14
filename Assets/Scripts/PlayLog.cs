using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hypertext;
using UnityEngine.UI;
public class PlayLog : MonoBehaviour
{
    [SerializeField]
    [Header("ログテキスト")] RegexHypertext regexHypertext;

    [SerializeField]
    [Header("スクロール")] ScrollRect scroll;

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

        regexHypertext.text = GetLogString();

        scroll.content.GetComponent<ContentSizeFitter>().SetLayoutVertical();

        yield return new WaitForSeconds(Time.deltaTime);

        scroll.verticalNormalizedPosition = 0;
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

        regexHypertext.text = "";

        _logList = new List<string>();
    }

    public void AddLogString(string logText)
    {
        ContinuousController.instance.StartCoroutine(AddLogStringCoroutine(DataBase.ReplaceToASCII(logText)));
    }

    IEnumerator AddLogStringCoroutine(string logText)
    {
        _logList.Add(logText);

        while (GetLogString().Length >= _maxLogCharacterLength)
        {
            if (_logList.Count >= 1)
            {
                _logList.RemoveAt(0);
            }
        }

        if (gameObject.activeSelf)
        {
            regexHypertext.text = GetLogString();

            yield break;

            scroll.content.GetComponent<ContentSizeFitter>().SetLayoutVertical();

            yield return new WaitForSeconds(Time.deltaTime);

            scroll.verticalNormalizedPosition = 0;
        }

    }

    public void AddOnClick_ShowCard(CardSource cardSource)
    {
        regexHypertext.OnClick(cardSource.CardID, new Color32(146, 246, 255, 255), ShowCard);

        static void ShowCard(string cardID)
        {
            CardSource founcdCardSource = GManager.instance.turnStateMachine.gameContext.ActiveCardList
            .Find(cardSource1 => cardSource1.CardID == cardID);

            if (founcdCardSource != null)
            {
                GManager.instance.cardDetail.OpenCardDetail(founcdCardSource, true);
            }
        }
    }
}
