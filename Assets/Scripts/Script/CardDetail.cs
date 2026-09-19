using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CardDetail : MonoBehaviour
{
    [Header("card detail HandCard")]
    public HandCard DetailHandCard;

    public void OpenCardDetail(CardSource cardSource, bool CanLookOpponentCard)
    {
        this.gameObject.SetActive(true);

        DetailHandCard.SetUpHandCard(cardSource);

        if (CanLookOpponentCard)
        {
            DetailHandCard.SetUpHandCardImage();
        }

        #region animation
        DetailHandCard.transform.localPosition = new Vector3(400, 0, 0);
        DetailHandCard.transform.localScale = new Vector3(5.6f, 5.6f, 5.6f);

        float animationTime = 0.12f;

        var sequence = DOTween.Sequence();

        sequence
            .Append(DetailHandCard.transform.DOLocalMove(new Vector3(550, 0, 0), animationTime))
            .Join(DetailHandCard.transform.DOScale(new Vector3(7, 7, 7), animationTime));

        sequence.Play();

        #endregion
    }

    bool first = false;

    public void CloseCardDetail()
    {
        if(first)
        {
            if (GManager.instance != null)
            {
                GManager.instance.PlayCancelSE ();
            }
        }

        first = true;
        this.gameObject.SetActive(false);
    }
}
