using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class CreateNewDeckButton : MonoBehaviour
{
    public EditDeck editDeck;
    public SelectDeck selectDeck;
    public GameObject Outline;

    public YesNoObject CreateNewDeckWayObject;

    private void Start()
    {
        CreateNewDeckWayObject.Off();
    }

    #region Creating a new deck
    public void CreateNewDeck()
    {
        DeckData deckData = new DeckData(DeckData.GetDeckCode("NewDeck", new List<CEntity_Base>(), new List<CEntity_Base>(), null));

        ContinuousController.instance.DeckDatas.Insert(0, deckData);

        editDeck.SetUpCreateDeck(deckData, true);

        ContinuousController.instance.StartCoroutine(selectDeck.SetDeckList(true));

        CreateNewDeckWayObject.Off();
    }
    #endregion

    #region Processing when the Create from Deck Code button is pressed
    public void OnClickFromDeckCode()
    {
        CreateNewDeckWayObject.Off();

        string deckCode = "";

        //deckCode = ContinuousController.instance.ShuffleDeckCode.GetDeckCode(GUIUtility.systemCopyBuffer);

        deckCode = GUIUtility.systemCopyBuffer;

        Debug.Log($"DeckCode\n{deckCode}");

        List<CEntity_Base> AllDeckCards = DeckCodeUtility.GetAllDeckCardsFromDeckBuilderDeckCode(deckCode);

        if (AllDeckCards.Count == 0)
        {
            AllDeckCards = DeckCodeUtility.GetAllDeckCardsFromTTSDeckCode(deckCode);
        }

        if (AllDeckCards.Count == 0)
        {
            Opening.instance.SetUpActiveYesNoObject(
                new List<UnityAction>() { null },
                new List<string>() { "OK" },
                LocalizeUtility.GetLocalizedString(
                    EngMessage: "Error!\nDeck code could not be loaded.",
                    JpnMessage: "エラー!\nデッキコードの読み込みに失敗しました"
                ),
                true);
            return;
        }

        List<CEntity_Base> deckCards = new List<CEntity_Base>();
        List<CEntity_Base> digitamaDeckCards = new List<CEntity_Base>();

        foreach (CEntity_Base cEntity_Base in AllDeckCards)
        {
            if (cEntity_Base.cardKind == CardKind.DigiEgg)
            {
                digitamaDeckCards.Add(cEntity_Base);
            }

            else
            {
                deckCards.Add(cEntity_Base);
            }
        }

        DeckData deckData = (new DeckData(DeckData.GetDeckCode("", deckCards, digitamaDeckCards, null))).ModifiedDeckData();

        if (deckData.DeckName == "新しいデッキ" || deckData.DeckName == "NewDeck")
        {
            deckData.DeckName = "NewDeck";
        }

        ContinuousController.instance.DeckDatas.Insert(0, deckData);

        editDeck.SetUpCreateDeck(deckData, true, isFromClipboard: true);

        ContinuousController.instance.StartCoroutine(selectDeck.SetDeckList(true));
    }
    #endregion

    public async void OnClick()
    {
        Opening.instance.PlayDecisionSE();

        await selectDeck.deckInfoPanel.SetUpDeckInfoPanel(null);

        List<UnityAction> Commands = new List<UnityAction>()
            {
                () =>
                {
                    CreateNewDeck();
                },

                () =>
                {
                    OnClickFromDeckCode();
                },
            };

        List<string> CommandTexts = new List<string>()
            {
                LocalizeUtility.GetLocalizedString(
                    EngMessage:"Create one yourself",
                    JpnMessage:"自分で作成する"
                ),
                LocalizeUtility.GetLocalizedString(
                    EngMessage: "Import from clipboard",
                    JpnMessage:"クリップボードからインポート"
                ) ,
            };

        CreateNewDeckWayObject.SetUpYesNoObject(
            Commands,
            CommandTexts,
            LocalizeUtility.GetLocalizedString(
            EngMessage: "Choose how to create your deck.",
            JpnMessage: "デッキの作成方法を選択してください"
            ),
            true);

        Outline.SetActive(true);

        for (int j = 0; j < this.transform.parent.childCount; j++)
        {
            if (this.transform.parent.GetChild(j) != this.transform)
            {
                if (this.transform.parent.GetChild(j).GetComponent<DeckInfoPrefab>() != null)
                {
                    this.transform.parent.GetChild(j).GetComponent<DeckInfoPrefab>().Outline.SetActive(false);
                }
            }
        }
    }

    public void OnEnter()
    {
        if (Opening.instance != null)
            this.gameObject.transform.localScale = Opening.instance.DeckInfoPrefabExpandScale;
    }

    public void OnExit()
    {
        if (Opening.instance != null)
            this.gameObject.transform.localScale = Opening.instance.DeckInfoPrefabStartScale;
    }

    private void OnEnable()
    {
        OnExit();
    }
}
