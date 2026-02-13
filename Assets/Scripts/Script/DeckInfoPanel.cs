using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Events;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;



public class DeckInfoPanel : MonoBehaviour
{
    [Header("デッキ情報パネルオブジェクト")]
    public GameObject DeckInfoPanelObject;

    [Header("キーカード画像")]
    public Image KeyCardImage;

    [Header("デッキ名InputField")]
    public InputField DeckName;

    [Header("デッキ名Text")]
    public Text DeckNameText;

    [Header("カード色カウント")]
    public List<CardColorCount> cardColorCountList = new List<CardColorCount>();

    [Header("デッキコード取得ボタン")]
    public Button GetDeckCodeButton;

    [Header("デッキ枚数テキスト")]
    public Text DeckCountText;

    public Button TrialDrawButton;

    public bool isFromSelectDeck;

    public DeckData ShowingDeckData = null;

    public async Task SetUpDeckInfoPanel(DeckData deckData)
    {
        DeckInfoPanelObject.SetActive(deckData != null);

        if (deckData != null)
        {
            if (GetComponent<Animator>() != null)
            {
                GetComponent<Animator>().SetInteger("Open", 1);
                GetComponent<Animator>().SetInteger("Close", 0);
            }

            ShowingDeckData = deckData;

            //MC card image
            KeyCardImage.gameObject.SetActive(deckData.KeyCard != null);

            if (deckData.KeyCard != null)
            {
                KeyCardImage.sprite = await deckData.KeyCard.GetCardSprite();
            }

            else
            {
                KeyCardImage.sprite = null;
            }

            //deck name
            if (DeckName != null)
            {
                DeckName.text = deckData.DeckName;

                DeckName.onEndEdit.RemoveAllListeners();

                DeckName.onEndEdit.AddListener(SetDeckName);
            }

            if (DeckNameText != null)
            {
                DeckNameText.text = deckData.DeckName;

                DeckNameText.transform.parent.gameObject.SetActive(deckData.GetThisDeckCode() != DeckData.EmptyDeckData().GetThisDeckCode());
            }

            //Number of cards of each color
            if (cardColorCountList != null)
            {
                if (cardColorCountList.Count > 0)
                {
                    foreach (CardColorCount cardColorCount in cardColorCountList)
                    {
                        if (cardColorCount != null)
                        {
                            cardColorCount.SetUpCardColorCount(deckData);

                            ContinuousController.instance.StartCoroutine(cardColorCount.SetIconScale());
                        }
                    }
                }
            }

            //Number of cards in the deck
            if (DeckCountText != null)
            {
                DeckCountText.text = $"{deckData.DeckCards().Count}+{deckData.DigitamaDeckCards().Count}/50+5";

                if (deckData.IsValidDeckData())
                {
                    DeckCountText.color = new Color32(69, 255, 69, 255);
                }

                else
                {
                    DeckCountText.color = new Color32(255, 64, 64, 255);
                }
            }

            //Get Deck Code button
            if (GetDeckCodeButton != null)
            {
                GetDeckCodeButton.gameObject.SetActive(deckData.IsValidDeckData());
            }

            if (TrialDrawButton != null)
            {
                TrialDrawButton.gameObject.SetActive(ShowingDeckData.DeckCards().Count >= 1);
            }
        }
    }
    public void OnClickTrial5DrawButton()
    {
        Opening.instance.deck.trialDraw.Off();
        ContinuousController.instance.StartCoroutine(Opening.instance.deck.trialDraw.SetUpTrialDraw(ShowingDeckData.DeckCards()));
    }
    public void OnClickEditButton()
    {
        if (ShowingDeckData != null)
        {
            Opening.instance.deck.editDeck.SetUpCreateDeck(ShowingDeckData, isFromSelectDeck);
        }
    }

    public void OnClickShowDeckButton()
    {
        if (ShowingDeckData != null)
        {
            Opening.instance.deck.deckListPanel.Off();
            ContinuousController.instance.StartCoroutine(Opening.instance.deck.deckListPanel.SetUpDeckListPanel(ShowingDeckData, null, ""));
        }
    }

    public void OnClickDeleteDeckButton()
    {
        if (ShowingDeckData != null)
        {
            DeckData deckData = ShowingDeckData;

            List<UnityAction> Commands = new List<UnityAction>()
            {
                () =>
                    {
                        if(isFromSelectDeck)
                        {
                            ContinuousController.instance.DeckDatas.Remove(deckData);
                            ContinuousController.instance.StartCoroutine(Opening.instance.deck.selectDeck.SetDeckList(false));
                            Opening.instance.deck.selectDeck.ResetDeckInfoPanel();
                            ContinuousController.instance.DeleteDeck(deckData);
                        }
                    },

                    null
            };

            List<string> CommandTexts = new List<string>()
            {
                LocalizeUtility.GetLocalizedString(
                    EngMessage: "Yes",
                    JpnMessage: "はい"
                ),
                LocalizeUtility.GetLocalizedString(
                    EngMessage: "No",
                    JpnMessage: "いいえ"
                ),
            };

            Opening.instance.SetUpActiveYesNoObject(
                Commands,
                CommandTexts,
                LocalizeUtility.GetLocalizedString(
                    EngMessage: "Delete the deck?",
                    JpnMessage: "デッキを削除しますか?"
                ),
                true);
        }
    }

    public void OnClickGetDeckCodeButton()
    {
        if (ShowingDeckData != null)
        {
            if (ShowingDeckData != null)
            {
                GUIUtility.systemCopyBuffer = DeckCodeUtility.GetDeckBuilderDeckCode(ShowingDeckData.AllDeckCards());

                List<UnityAction> Commands = new List<UnityAction>()
                {
                    null
                };

                List<string> CommandTexts = new List<string>()
                {
                    "OK"
                };

                Opening.instance.SetUpActiveYesNoObject(
                    Commands,
                    CommandTexts,
                    LocalizeUtility.GetLocalizedString(
                    EngMessage: "Copied the deck code to the clipboard!\n(Also, you can use it for Digimon Card dev.)",
                    JpnMessage: "デッキコードをクリップボードにコピーしました!\n(Digimon Card dev.でも使えます)"
                ),
                    true);
            }
        }
    }

    public void SetDeckName(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            text = ShowingDeckData.DeckName;
            DeckName.text = ShowingDeckData.DeckName;
        }

        if (ShowingDeckData.DeckName == text)
        {
            return;
        }

        while (text.Length > DeckName.characterLimit)
        {
            text = text.Substring(0, text.Length - 1);
        }

        ContinuousController.instance.RenameDeck(ShowingDeckData, text);

        if (isFromSelectDeck)
        {
            if (Opening.instance.deck.selectDeck != null)
            {
                if (Opening.instance.deck.editDeck != null)
                {
                    if (Opening.instance.deck.editDeck.CreateDeckObject.activeSelf)
                    {
                        return;
                    }
                }

                ContinuousController.instance.StartCoroutine(Opening.instance.deck.selectDeck.SetDeckList(false));

                for (int i = 0; i < Opening.instance.deck.selectDeck.deckInfoPrefabParentScroll.content.childCount; i++)
                {
                    if (Opening.instance.deck.selectDeck.deckInfoPrefabParentScroll.content.GetChild(i).GetComponent<DeckInfoPrefab>() != null)
                    {
                        if (Opening.instance.deck.selectDeck.deckInfoPrefabParentScroll.content.GetChild(i).GetComponent<DeckInfoPrefab>().thisDeckData == ShowingDeckData)
                        {
                            Opening.instance.deck.selectDeck.deckInfoPrefabParentScroll.content.GetChild(i).GetComponent<DeckInfoPrefab>().OnClick_DeckInfoPrefab(true);
                        }
                    }
                }
            }
        }

        else
        {
            if (Opening.instance.battle.selectBattleDeck != null)
            {
                if (Opening.instance.deck.editDeck != null)
                {
                    if (Opening.instance.deck.editDeck.CreateDeckObject.activeSelf)
                    {
                        return;
                    }
                }

                ContinuousController.instance.StartCoroutine(Opening.instance.battle.selectBattleDeck.SetDeckList(false));

                for (int i = 0; i < Opening.instance.battle.selectBattleDeck.deckInfoPrefabParentScroll.content.childCount; i++)
                {
                    if (Opening.instance.battle.selectBattleDeck.deckInfoPrefabParentScroll.content.GetChild(i).GetComponent<DeckInfoPrefab>() != null)
                    {
                        if (Opening.instance.battle.selectBattleDeck.deckInfoPrefabParentScroll.content.GetChild(i).GetComponent<DeckInfoPrefab>().thisDeckData == ShowingDeckData)
                        {
                            Opening.instance.battle.selectBattleDeck.deckInfoPrefabParentScroll.content.GetChild(i).GetComponent<DeckInfoPrefab>().OnClick_DeckInfoPrefab(true);
                        }
                    }
                }
            }
        }

    }

    public UnityAction OnClickSelectDeckAction;

    public void OnClickSelectDeck()
    {
        Opening.instance.PlayDecisionSE();
        OnClickSelectDeckAction?.Invoke();
    }
}

[Serializable]
public class CardColorCount
{
    [Header("Corresponding color")]
    public CardColor cardColor;

    [Header("Count Text")]
    public Text CountText;

    public Image icon;

    public void SetUpCardColorCount(DeckData deckData)
    {
        if (CountText != null)
        {
            CountText.text = $"{deckData.DeckCards().Count((card) => card.cardColors[0] == cardColor)}";
        }
    }

    public IEnumerator SetIconScale()
    {
        if (icon != null)
        {
            icon.transform.localScale = new Vector3(1.02f, 1.02f, 1);
            yield return null;
            icon.transform.localScale = new Vector3(1, 1, 1);
        }
    }
}
