using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;

public class SelectDeck : OffAnimation
{
    [Header("Select Deck Object")]
    public GameObject SelectDeckObject;

    [Header("Deck Info Tab Prefab")]
    public DeckInfoPrefab deckInfoPrefab;

    [Header("ScrollRect to place deck information tabs")]
    public ScrollRect deckInfoPrefabParentScroll;

    [Header("Deck Info Panel")]
    public DeckInfoPanel deckInfoPanel;

    [Header("Animator")]
    public Animator anim;

    public bool isOpen { get; set; } = false;

    public void OffSelectDeck()
    {
        anim.enabled = true;
        anim.SetInteger("Open", 0);
        anim.SetInteger("Close", 1);

        isOpen = false;
    }

    public override void Off()
    {
        SelectDeckObject.SetActive(false);
    }

    public async void SetUpSelectDeck()
    {
        if (isOpen)
        {
            return;
        }

        int lastBattleDeckDataIndex = -1;

        if (ContinuousController.instance.LastBattleDeckData != null
            && ContinuousController.instance.DeckDatas.Contains(ContinuousController.instance.LastBattleDeckData))
        {
            lastBattleDeckDataIndex = ContinuousController.instance.DeckDatas.IndexOf(ContinuousController.instance.LastBattleDeckData);
        }

        ContinuousController.instance.ModifyAllDeckDatas();

        if (0 <= lastBattleDeckDataIndex && lastBattleDeckDataIndex <= ContinuousController.instance.DeckDatas.Count - 1)
        {
            ContinuousController.instance.BattleDeckData = ContinuousController.instance.DeckDatas[lastBattleDeckDataIndex];
        }

        Opening.instance.deck.deckListPanel.Off();

        isOpen = true;

        ContinuousController.instance.StartCoroutine(SetDeckList(true));

        SelectDeckObject.SetActive(true);

        anim.SetInteger("Open", 1);
        anim.SetInteger("Close", 0);

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
    }

    public async void ResetDeckInfoPanel()
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

            _deckInfoPrefab.OnClickAction = (deckdata) =>
            {
                deckInfoPanel.SetUpDeckInfoPanel(deckdata);

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
                if (deckInfoPrefabParentScroll.content.GetChild(i).GetComponent<CreateNewDeckButton>() != null)
                {
                    deckInfoPrefabParentScroll.content.GetChild(i).GetComponent<CreateNewDeckButton>().Outline.SetActive(true);
                    break;
                }
            }
        }

        else
        {
            for (int i = 0; i < deckInfoPrefabParentScroll.content.childCount; i++)
            {
                if (deckInfoPrefabParentScroll.content.GetChild(i).GetComponent<CreateNewDeckButton>() != null)
                {
                    deckInfoPrefabParentScroll.content.GetChild(i).GetComponent<CreateNewDeckButton>().Outline.SetActive(false);
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

    public void OnClickCopyDeckListButton()
    {
        string deckListCode = "";

        for (int i = 0; i < ContinuousController.instance.DeckDatas.Count; i++)
        {
            deckListCode += DeckData.GetDeckCode(ContinuousController.instance.DeckDatas[i].DeckName, DeckData.SortedDeckCardsList(ContinuousController.instance.DeckDatas[i].DeckCards()), DeckData.SortedDeckCardsList(ContinuousController.instance.DeckDatas[i].DigitamaDeckCards()), ContinuousController.instance.DeckDatas[i].KeyCard) + "分";
        }

        if (deckListCode.Length >= 1)
        {
            GUIUtility.systemCopyBuffer = deckListCode;

            List<UnityAction> Commands = new List<UnityAction>()
                {
                    null
                };

            List<string> CommandTexts = new List<string>()
                {
                    "OK"
                };

            Opening.instance.SetUpActiveYesNoObject(Commands, CommandTexts, $"現在のデッキリストの\nコードをクリップボードにコピーしました!", false);
        }

        else
        {
            List<UnityAction> Commands = new List<UnityAction>()
                {
                    null
                };

            List<string> CommandTexts = new List<string>()
                {
                    "OK"
                };

            Opening.instance.SetUpActiveYesNoObject(Commands, CommandTexts, $"デッキリストのコード取得に失敗しました", false);
        }
    }

    public void OnClickPasterDeckListButton()
    {
        List<DeckData> gotDeckDatas = new List<DeckData>();

        string deckListCode = GUIUtility.systemCopyBuffer;

        if (!string.IsNullOrEmpty(deckListCode))
        {
            string[] deckCodes = deckListCode.Split('分');

            foreach (string deckCode in deckCodes)
            {
                DeckData deckData = new DeckData(deckCode);

                if (deckData.DeckCards().Count >= 1)
                {
                    gotDeckDatas.Add(deckData);
                }
            }
        }

        if (gotDeckDatas.Count >= 1)
        {
            gotDeckDatas.Reverse();

            foreach (DeckData deckdata in gotDeckDatas)
            {
                ContinuousController.instance.DeckDatas.Insert(0, deckdata);
                ContinuousController.instance.SaveDeckData(deckdata);
            }

            List<UnityAction> Commands = new List<UnityAction>()
                {
                    null
                };

            List<string> CommandTexts = new List<string>()
                {
                    "OK"
                };

            Opening.instance.SetUpActiveYesNoObject(Commands, CommandTexts, $"デッキリストを\n復元しました!", false);
        }

        else
        {
            List<UnityAction> Commands = new List<UnityAction>()
                {
                    null
                };

            List<string> CommandTexts = new List<string>()
                {
                    "OK"
                };

            Opening.instance.SetUpActiveYesNoObject(Commands, CommandTexts, $"デッキリストの復元に失敗しました", false);
        }

        ContinuousController.instance.StartCoroutine(SetDeckList(false));
        ContinuousController.instance.SaveDeckDatas();
    }

    public void OnClickDeleteAllDecksButton()
    {
        List<UnityAction> Commands = new List<UnityAction>()
                {
            () =>
            {
                List<DeckData> deckDatas = new List<DeckData>();

                for(int i =0;i<ContinuousController.instance.DeckDatas.Count;i++)
                {
                    deckDatas.Add(ContinuousController.instance.DeckDatas[i]);
                }

                for(int i =0;i<deckDatas.Count;i++)
                {
                    ContinuousController.instance.DeckDatas.Remove(deckDatas[i]);
                }

                ContinuousController.instance.StartCoroutine(SetDeckList(false));
                ResetDeckInfoPanel();
                ContinuousController.instance.SaveDeckDatas();
                ContinuousController.instance.DeleteAllDecks();
            }
                    ,null
                };

        List<string> CommandTexts = new List<string>()
                {
                    "削除する",
                    "しない"
                };

        Opening.instance.SetUpActiveYesNoObject(Commands, CommandTexts, $"全デッキを削除しますか?", false);
    }
}
