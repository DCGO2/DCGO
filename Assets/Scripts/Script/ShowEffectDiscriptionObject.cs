using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
public class ShowEffectDiscriptionObject : MonoBehaviour
{
    [SerializeField] Transform parent;
    [SerializeField] Image CardImage;
    [SerializeField] Image CardImageOutline;
    [SerializeField] TextMeshProUGUI EffectDiscriptionText;

    ICardEffect cardEffect;

    public async void ShowEffectDiscription(ICardEffect cardEffect)
    {
        if (cardEffect != null)
        {
            if (cardEffect.EffectSourceCard != null)
            {
                if (string.IsNullOrEmpty(cardEffect.EffectDiscription))
                {
                    CloseShowEffectDiscription();
                }

                else
                {
                    this.cardEffect = cardEffect;

                    CardImage.sprite = await cardEffect.EffectSourceCard.GetCardSprite();
                    CardImageOutline.color = DataBase.CardColor_ColorDarkDictionary[cardEffect.EffectSourceCard.BaseCardColorsFromEntity[0]];
                    EffectDiscriptionText.text = DataBase.ReplaceToASCII(cardEffect.EffectDiscription);

                    Sequence sequence = DOTween.Sequence();

                    sequence
                        .Append(parent.DOLocalMoveX(840, 0.1f).SetEase(Ease.OutQuad));

                    sequence.Play();

                    StartCoroutine(CloseIEnumerator());
                }
            }
        }
    }

    IEnumerator CloseIEnumerator()
    {
        yield return new WaitForSeconds(5.5f);

        CloseShowEffectDiscription();
    }

    public void CloseShowEffectDiscription()
    {
        DestroyImmediate(this.gameObject);
    }

    public void OnClickCardImage()
    {
        if (this.cardEffect != null)
        {
            if (this.cardEffect.EffectSourceCard != null)
            {
                GManager.instance.cardDetail.OpenCardDetail(this.cardEffect.EffectSourceCard, true);

                if (GManager.instance != null)
                {
                    GManager.instance.PlayDecisionSE();
                }
            }
        }
    }
}
