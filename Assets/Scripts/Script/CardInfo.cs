using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using TMPro;
public class CardInfo : MonoBehaviour
{
    [Header("BackGround")]
    public List<Image> BackGrounds = new List<Image>();
    public GameObject BackGround;
    public GameObject LinkBackground;

    [Header("CardImage")]
    public Image CardImage;

    [Header("進化元効果")]
    public TextMeshProUGUI InheritedEffectText;

    [SerializeField] TMP_FontAsset InheritedEffectFont_ENG;
    [SerializeField] TMP_FontAsset InheritedEffectFont_JPN;
    [SerializeField] Material InheritedEffectMaterial;

    public Image outlineImage;

    public Sprite HatenaCard;

    public List<ScrollRect> scrollRects = new List<ScrollRect>();

    UnityAction OnEnterAction;
    UnityAction OnExitAction;

    private Permanent detailPermanent;

    public CardSource cardSource { get; set; }

    public void OnEnter()
    {
        OnEnterAction?.Invoke();
    }

    public void OnExit()
    {
        OnExitAction?.Invoke();
    }

    public async void SetUpCardInfo(CardSource cardSource, Permanent permanent = null)
    {
        if (permanent != null)
            detailPermanent = permanent;

        this.cardSource = cardSource;

        this.gameObject.SetActive(true);

        InheritedEffectText.text = "";

        if (!cardSource.IsFlipped)
        {
            outlineImage.color = DataBase.CardColor_ColorDarkDictionary[cardSource.BaseCardColorsFromEntity[0]];

            if (cardSource.PermanentOfThisCard() != null)
            {
                if (cardSource != cardSource.PermanentOfThisCard().TopCard)
                {
                    if (ContinuousController.instance.language == Language.ENG)
                    {
                        InheritedEffectText.font = InheritedEffectFont_ENG;
                        InheritedEffectText.fontSharedMaterial = InheritedEffectMaterial;

                        if(!cardSource.IsLinked)
                            InheritedEffectText.text = cardSource.InheritedEffectDiscription_ENG;
                        else
                            InheritedEffectText.text = cardSource.LinkEffectDiscription;
                    }

                    else
                    {
                        InheritedEffectText.font = InheritedEffectFont_JPN;
                        InheritedEffectText.text = cardSource.InheritedEffectDiscription_JPN;
                    }
                }
            }

            InheritedEffectText.color = Color.white;

            if (cardSource.BaseCardColorsFromEntity.Count >= 1)
            {
                if (!cardSource.BaseCardColorsFromEntity.Contains(CardColor.Black))
                {
                    if (cardSource.BaseCardColorsFromEntity[0] == CardColor.Yellow || cardSource.BaseCardColorsFromEntity[0] == CardColor.White)
                    {
                        InheritedEffectText.color = Color.black;
                    }
                }
            }

            CardImage.color = new Color(1, 1, 1, 1);
            CardImage.sprite = await cardSource.GetCardSprite();


            for (int i = 0; i < BackGrounds.Count; i++)
            {
                CardColor cardColor = CardColor.None;

                if (i < cardSource.BaseCardColorsFromEntity.Count)
                {
                    cardColor = cardSource.BaseCardColorsFromEntity[i];
                    BackGrounds[i].color = DataBase.CardColor_ColorDarkDictionary[cardColor];
                    BackGrounds[i].gameObject.SetActive(true);
                }
                else
                {
                    BackGrounds[i].gameObject.SetActive(false);
                }
            }

            //Link
            LinkBackground.SetActive(cardSource.IsLinked);
        }

        else
        {
            outlineImage.color = DataBase.CardColor_ColorDarkDictionary[CardColor.None];
            InheritedEffectText.text = "???";
            CardImage.color = new Color(1, 1, 1, 1);
            CardImage.sprite = HatenaCard;

            for (int i = 0; i < BackGrounds.Count; i++)
            {
                BackGrounds[i].color = DataBase.CardColor_ColorDarkDictionary[CardColor.White];
            }
        }
    }

    public void CloseSoulInfo()
    {
        this.gameObject.SetActive(false);
    }

    public void OnClick()
    {
        if (cardSource.IsFlipped && !cardSource.Owner.isYou)
        {
            return;
        }

        GManager.instance.cardDetail.OpenCardDetail(cardSource, true);

        if (GManager.instance != null)
        {
            GManager.instance.PlayDecisionSE();
        }
    }

    List<string> IndexStrings = new List<string>()
    {
        "①","②","③","④","⑤","⑥","⑦","⑧","⑨","⑩",
        "⑪","⑫","⑬","⑭","⑮","⑯","⑰","⑱","⑲","⑳",
    };
}
