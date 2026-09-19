using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SideBar : MonoBehaviour
{
    [SerializeField] Image switchButtonImage;
    [SerializeField] Sprite leftArrow;
    [SerializeField] Sprite rightArrow;
    [SerializeField] float defaultPos_x;
    [SerializeField] float shrinkPos_x;
    [SerializeField] GameObject cover;
    [SerializeField] GameObject popup;
    [SerializeField] GameObject parent;

    bool isShrink { get; set; } = false;

    public void Init()
    {
        OffSideBar();

        parent.gameObject.SetActive(true);

        cover.SetActive(false);
    }

    public void OffSideBar(bool returnDefaultPos = true)
    {
        if (popup != null)
        {
            popup.SetActive(false);
        }

        if (returnDefaultPos)
        {
            SetIsShrink(false);
        }
    }

    public void SetUpSideBar()
    {
        if (popup != null)
        {
            popup.SetActive(true);
        }

        SetIsShrink(false);
    }

    public void SetIsShrink(bool isShrink)
    {
        this.isShrink = isShrink;

        if (!isShrink)
        {
            parent.transform.localPosition = new Vector2(defaultPos_x, parent.transform.localPosition.y);
            switchButtonImage.sprite = leftArrow;
        }

        else
        {
            parent.transform.localPosition = new Vector2(shrinkPos_x, parent.transform.localPosition.y);
            switchButtonImage.sprite = rightArrow;
        }
    }

    public void OnClickSwitchButton()
    {
        ContinuousController.instance.StartCoroutine(movePopup());
    }

    IEnumerator movePopup()
    {
        bool end = false;
        float anjmTime = 0.16f;

        float targetPos_x = isShrink ? defaultPos_x : shrinkPos_x;

        cover.SetActive(true);

        var sequence = DOTween.Sequence();
        sequence.Append(parent.transform.DOLocalMoveX(targetPos_x, anjmTime))
            .AppendCallback(() => end = true);

        yield return new WaitWhile(() => !end);
        end = false;

        SetIsShrink(!isShrink);
        cover.SetActive(false);
    }
}
