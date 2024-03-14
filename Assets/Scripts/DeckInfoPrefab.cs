using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Events;

public class DeckInfoPrefab : MonoBehaviour
{
    [Header("MC image")]
    public Image KeyCardImage;

    [Header("DefaultKeyCardSprite")]
    public Sprite DefaultKeyCardSprite;

    [Header("deck name")]
    public Text DeckName;

    [Header("red")]
    public Image RedIcon;

    [Header("blue")]
    public Image BlueIcon;

    [Header("green")]
    public Image GreenIcon;

    [Header("yellow")]
    public Image YellowIcon;

    [Header("scrollRect")]
    public ScrollRect scrollRect;

    [Header("Outline")]
    public GameObject Outline;

    [Header("no MC image")]
    public Image ReverseFace;

    public DeckData thisDeckData { get; set; }

    public UnityAction<DeckData> OnClickAction;

    private void OnEnable()
    {
        OnExit();
    }

    public void OnEnter()
    {
        this.gameObject.transform.localScale = Opening.instance.DeckInfoPrefabExpandScale;
    }

    public void OnClick()
    {
        OnClick_DeckInfoPrefab(true);
    }

    public void OnClick_DeckInfoPrefab(bool playSE)
    {
        if (playSE)
        {
            Opening.instance.PlayDecisionSE();
        }

        OnClickAction?.Invoke(thisDeckData);

        Outline.SetActive(true);

        for (int j = 0; j < this.transform.parent.childCount; j++)
        {
            if (this.transform.parent.GetChild(j) != this.transform)
            {
                if (this.transform.parent.GetChild(j).GetComponent<DeckInfoPrefab>() != null)
                {
                    this.transform.parent.GetChild(j).GetComponent<DeckInfoPrefab>().Outline.SetActive(false);
                }

                if (this.transform.parent.GetChild(j).GetComponent<CreateNewDeckButton>() != null)
                {
                    this.transform.parent.GetChild(j).GetComponent<CreateNewDeckButton>().Outline.SetActive(false);
                }
            }
        }
    }

    public void OnExit()
    {
        if (Opening.instance != null)
            this.gameObject.transform.localScale = Opening.instance.DeckInfoPrefabStartScale;
    }

    public async void SetUpDeckInfoPrefab(DeckData deckData)
    {
        this.gameObject.SetActive(deckData != null);

        if (deckData != null)
        {
            //MC image
            if (deckData.KeyCard != null)
            {
                KeyCardImage.sprite = await deckData.KeyCard.GetCardSprite();
                ReverseFace.gameObject.SetActive(false);
            }

            else
            {
                KeyCardImage.sprite = DefaultKeyCardSprite;
                ReverseFace.gameObject.SetActive(true);
            }

            //deck name
            DeckName.text = deckData.DeckName;

            //color icon
            RedIcon.gameObject.SetActive(deckData.DeckCards().Count((card) => card.cardColors[0] == CardColor.Green) > 0);
            BlueIcon.gameObject.SetActive(deckData.DeckCards().Count((card) => card.cardColors[0] == CardColor.Red) > 0);
            GreenIcon.gameObject.SetActive(deckData.DeckCards().Count((card) => card.cardColors[0] == CardColor.Blue) > 0);
            YellowIcon.gameObject.SetActive(deckData.DeckCards().Count((card) => card.cardColors[0] == CardColor.Yellow) > 0);

            thisDeckData = deckData;

            Outline.SetActive(false);
        }
    }

}
