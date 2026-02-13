using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class CheckUpdate : MonoBehaviour
{
    public GameObject UpdateButton;

    private void Awake()
    {
        if (UpdateButton != null)
        {
            UpdateButton.SetActive(false);
        }
    }

    public IEnumerator CheckUpdateCoroutine()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            yield break;
        }

        bool end = false;
        GetComponent<GSSReader>().OnLoadEnd.RemoveAllListeners();
        GetComponent<GSSReader>().OnLoadEnd.AddListener(() => { EndLoad(); end = true; });
        GetComponent<GSSReader>().Reload();

        yield return new WaitWhile(() => !end);
        end = false;
    }

    public void EndLoad()
    {
        GetComponent<OpenURL>().URL = GetComponent<GSSReader>().Datas[0][1];

#if UNITY_STANDALONE_OSX && !UNITY_EDITOR
        GetComponent<OpenURL>().URL = GetComponent<GSSReader>().Datas[0][2];
#endif

        ContinuousController.instance.NeedUpdate = false;

        if (GetComponent<GSSReader>().Datas.Length > 0)
        {
            if (GetComponent<GSSReader>().Datas[0].Length > 0)
            {
                ContinuousController.instance.NeedUpdate = GetComponent<GSSReader>().Datas[0][0] != ContinuousController.instance.GameVerString;
            }

            if (ContinuousController.instance.IgnoreUpdate)
            {
                ContinuousController.instance.NeedUpdate = false;
            }
        }

        if (ContinuousController.instance.NeedUpdate)
        {
            Opening.instance.SetUpActiveYesNoObject(
                new List<UnityAction>() { () => GetComponent<OpenURL>().Open(), null },
                new List<string>()
                {
                    "最新版をDLする",
                    "今はDLしない"
                },
                Message(),
                true);
        }

        if (UpdateButton != null)
        {
            UpdateButton.SetActive(ContinuousController.instance.NeedUpdate);
        }

        string Message() { return "このゲームの最新版があります。\n 最新版をDLしますか?\n(verが違う相手とはと対戦出来ません)"; };
    }
}
