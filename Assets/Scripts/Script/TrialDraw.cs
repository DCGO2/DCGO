using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TrialDraw : MonoBehaviour
{
    private static readonly int CloseHash = Animator.StringToHash("Close");
    private static readonly int OpenHash = Animator.StringToHash("Open");
    public CardPrefab_CreateDeck cardPrefab_CreateDeckPrefab;
    public Animator anim;

    public ScrollRect CardScroll;
    public DetailCard_DeckEditor detailCard;

    public Text DeckCountText;

    List<CEntity_Base> _originalDeckCards = new List<CEntity_Base>();
    List<CEntity_Base> _deckCards = new List<CEntity_Base>();
    List<CEntity_Base> _drewCards = new List<CEntity_Base>();

    public Button drawButton;

    public IEnumerator SetUpTrialDraw(List<CEntity_Base> cEntity_Bases)
    {
        if (cEntity_Bases.Count == 0)
        {
            yield break;
        }

        detailCard.OffDetailCard();

        _originalDeckCards = new List<CEntity_Base>();

        foreach (CEntity_Base cEntity_Base in cEntity_Bases)
        {
            _originalDeckCards.Add(cEntity_Base);
        }

        yield return ContinuousController.instance.StartCoroutine(Set(_originalDeckCards));
    }

    IEnumerator Set(List<CEntity_Base> cEntity_Bases)
    {
        drawButton.interactable = false;

        drawButton.transform.parent.gameObject.SetActive(false);

        _deckCards = new List<CEntity_Base>();
        _drewCards = new List<CEntity_Base>();

        foreach (CEntity_Base cEntity_Base in cEntity_Bases)
        {
            _deckCards.Add(cEntity_Base);
        }

        _deckCards = RandomUtility.ShuffledDeckCards(_deckCards);

        //detailCard.OffDetailCard();

        for (int i = 0; i < CardScroll.content.childCount; i++)
        {
            Destroy(CardScroll.content.GetChild(i).gameObject);
        }

        yield return new WaitWhile(() => CardScroll.content.childCount > 0);

        for (int i = 0; i < 5; i++)
        {
            DrawCard();
        }

        yield return new WaitWhile(() => CardScroll.content.childCount < _drewCards.Count);

        this.gameObject.SetActive(true);

        Open();

        drawButton.interactable = _deckCards.Count >= 1;

        drawButton.transform.parent.gameObject.SetActive(_deckCards.Count >= 1);
    }

    public void DrawCard()
    {
        if (_deckCards.Count >= 1)
        {
            CEntity_Base drewCard = _deckCards[0];
            _deckCards.Remove(drewCard);
            _drewCards.Add(drewCard);

            CardPrefab_CreateDeck _cardPrefab_CreateDeck = null;

            _cardPrefab_CreateDeck = Instantiate(cardPrefab_CreateDeckPrefab, CardScroll.content);

            _cardPrefab_CreateDeck.OnEnterAction = (cardPrefab) => { OnDetailCard(drewCard); };

            //_cardPrefab_CreateDeck.OnExitAction = (cardPrefab) => { OffDetailCard(); };

            SetUpDeckCard(_cardPrefab_CreateDeck, drewCard);

            //_cardPrefab_CreateDeck.HideDeckCardTab();

            ContinuousController.instance.StartCoroutine(Align());
        }

        EventSystem.current.SetSelectedGameObject(transform.GetChild(0).gameObject);

        drawButton.interactable = _deckCards.Count >= 1;

        drawButton.transform.parent.gameObject.SetActive(_deckCards.Count >= 1);

        SetDeckCountText();
    }

    IEnumerator Align()
    {
        yield return new WaitForSeconds(Time.deltaTime);

        CardScroll.verticalNormalizedPosition = 0;
    }

    public void Open()
    {
        On();
        anim.SafeSetInt(OpenHash, 1);
        anim.SafeSetInt(CloseHash, 0);
    }

    public void Close()
    {
        //OffDetailCard();
        anim.SafeSetInt(OpenHash, 0);
        anim.SafeSetInt(CloseHash, 1);
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

    public void OnDetailCard(CEntity_Base cEntity_Base)
    {
        detailCard.GetComponent<DetailCard_DeckEditor>().SetUpDetailCard(cEntity_Base);
    }
    #endregion

    public void SetUpDeckCard(CardPrefab_CreateDeck _cardPrefab_CreateDeck, CEntity_Base cEntity_Base)
    {
        foreach (ScrollRect _scroll in _cardPrefab_CreateDeck.scroll)
        {
            _scroll.content = CardScroll.content;

            _scroll.viewport = CardScroll.viewport;
        }

        _cardPrefab_CreateDeck.SetUpCardPrefab_CreateDeck(cEntity_Base);
        _cardPrefab_CreateDeck.ShowCardImage();

        _cardPrefab_CreateDeck.Parent.localScale = new Vector3(1.1f, 1.1f, 1.1f);
    }

    public void SetDeckCountText()
    {
        DeckCountText.text = LocalizeUtility.GetLocalizedString(
            EngMessage: $"Remaining Cards in Deck : {_deckCards.Count}/{_originalDeckCards.Count}",
            JpnMessage: $"??? : {_deckCards.Count}/{_originalDeckCards.Count}"
        );

        if (_deckCards.Count >= 1)
        {
            DeckCountText.color = new Color32(255, 255, 255, 255);
        }

        else
        {
            DeckCountText.color = new Color32(255, 64, 64, 255);
        }
    }

    public void OnClickRedrawButton()
    {
        ContinuousController.instance.StartCoroutine(Set(_originalDeckCards));
        ContinuousController.instance.PlaySE(Opening.instance.DrawSE);
    }

    public void OnClick1DrawButton()
    {
        ContinuousController.instance.PlaySE(Opening.instance.DrawSE);
        DrawCard();
    }

    public void OnClickCloseButton()
    {
        ContinuousController.instance.PlaySE(Opening.instance.CancelSE);
        Close();
    }
}