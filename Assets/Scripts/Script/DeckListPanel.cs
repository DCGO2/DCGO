using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;
using System.Linq;

public class DeckListPanel : MonoBehaviour
{
    public Animator anim;

    public CardPrefab_CreateDeck cardPrefab_CreateDeckPrefab;

    public DetailCard_DeckEditor detailCard;

    public ScrollRect DeckScroll;

    public Text DeckNameText;
    public Text DeckCountText;

    public GameObject DeckListPanelObject;

    public IEnumerator SetUpDeckListPanel(DeckData deckData, UnityAction<CEntity_Base> onClickAction, string CustomMessage)
    {
        detailCard.OffDetailCard();

        for (int i = 0; i < DeckScroll.content.childCount; i++)
        {
            Destroy(DeckScroll.content.GetChild(i).gameObject);
        }

        yield return new WaitWhile(() => DeckScroll.content.childCount > 0);

        #region デッキ名を表示
        DeckNameText.text = deckData.DeckName;

        if(!string.IsNullOrEmpty(CustomMessage))
        {
            DeckNameText.text = CustomMessage;
        }
        #endregion

        #region デッキのカードを取得
        List<CEntity_Base> DeckCards = new List<CEntity_Base>();

        foreach (CEntity_Base cEntity_Base in deckData.AllDeckCards())
        {
            DeckCards.Add(cEntity_Base);
        }
        #endregion

        #region デッキのカードを生成
        foreach (CEntity_Base cEntity_Base in DeckCards)
        {
            CardPrefab_CreateDeck _cardPrefab_CreateDeck = null;

            _cardPrefab_CreateDeck = Instantiate(cardPrefab_CreateDeckPrefab, DeckScroll.content);

            _cardPrefab_CreateDeck.OnEnterAction = (cardPrefab) => { OnDetailCard(cEntity_Base, Vector3.zero); };

            //_cardPrefab_CreateDeck.OnExitAction = (cardPrefab) => { OffDetailCard(); };

            SetUpDeckCard(_cardPrefab_CreateDeck, cEntity_Base);

            _cardPrefab_CreateDeck.HideDeckCardTab();

            if(onClickAction != null)
            {
                _cardPrefab_CreateDeck.OnClickAction = () => onClickAction(_cardPrefab_CreateDeck.cEntity_Base);
            }
        }
        #endregion

        SetDeckCountText(deckData);

        yield return new WaitWhile(() => DeckScroll.content.childCount < DeckCards.Count);

        this.gameObject.SetActive(true);

        yield return new WaitForSeconds(Time.deltaTime);

        DeckScroll.verticalNormalizedPosition = 1;

        yield return ContinuousController.instance.StartCoroutine(OpenCoroutine(deckData));
    }

    IEnumerator OpenCoroutine(DeckData deckData)
    {
        yield return new WaitWhile(() => DeckScroll.content.childCount < deckData.AllDeckCards().Count);
        yield return new WaitForSeconds(Time.deltaTime);

        Open();

        yield return new WaitWhile(() => DeckListPanelObject.transform.localScale.y < 0.9f);

        DeckScroll.verticalNormalizedPosition = 1;

        for (int i = 0; i < DeckScroll.content.childCount; i++)
        {
            DeckScroll.content.GetChild(i).GetComponent<CardPrefab_CreateDeck>().SetUpCardPrefab_CreateDeck(DeckScroll.content.GetChild(i).GetComponent<CardPrefab_CreateDeck>().cEntity_Base);
            DeckScroll.content.GetChild(i).GetComponent<CardPrefab_CreateDeck>().ShowCardImage();
        }
    }

    public void Open()
    {
        On();
        anim.SetInteger("Open", 1);
        anim.SetInteger("Close", 0);
    }

    public void Close()
    {
        //OffDetailCard();
        anim.SetInteger("Open", 0);
        anim.SetInteger("Close", 1);
    }

    public void On()
    {
        this.gameObject.SetActive(true);
    }

    public void Off()
    {
        this.gameObject.SetActive(false);
        //Close();
    }

    #region Card Detail View
    public void OffDetailCard()
    {
        //detailCard.GetComponent<DetailCard_DeckEditor>().OffDetailCard();
    }

    public void OnDetailCard(CEntity_Base cEntity_Base, Vector3 position)
    {
        detailCard.GetComponent<DetailCard_DeckEditor>().SetUpDetailCard(cEntity_Base);
    }
    #endregion

    public void SetUpDeckCard(CardPrefab_CreateDeck _cardPrefab_CreateDeck, CEntity_Base cEntity_Base)
    {
        foreach (ScrollRect _scroll in _cardPrefab_CreateDeck.scroll)
        {
            _scroll.content = DeckScroll.content;

            _scroll.viewport = DeckScroll.viewport;
        }

        _cardPrefab_CreateDeck.SetUpCardPrefab_CreateDeck(cEntity_Base);
        _cardPrefab_CreateDeck.ShowCardImage();

        _cardPrefab_CreateDeck.Parent.localScale = new Vector3(0.7f, 0.7f, 0.7f);
    }

    #region Display the number of cards in the deck text
    public void SetDeckCountText(DeckData deckData)
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
    #endregion
}
