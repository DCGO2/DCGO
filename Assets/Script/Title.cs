using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
public class Title : MonoBehaviour
{
    public Animator anim;

    public void OffTitle()
    {
        this.gameObject.SetActive(false);
    }

    public void SetUpTitle()
    {
        this.gameObject.SetActive(true);
        anim.enabled = true;
    }

    bool Clicked = false;
    public List<TextMeshProUGUI> titleTexts = new List<TextMeshProUGUI>();
    public CheckUpdate checkUpdate;
    public GameObject Parent;
    public Image TitleLogo;
    public GameObject ClickToStart;
    public void OnClick()
    {
        if(Clicked)
        {
            return;
        }

        Clicked = true;

        ContinuousController.instance.StartCoroutine(OnClickCoroutine());
    }

    IEnumerator OnClickCoroutine()
    {
        anim.enabled = false;
        ClickToStart.SetActive(false);

        float waitTime = 0.5f;

        /*
        var sequence1 = DOTween.Sequence();
        sequence1.Append(Parent.transform.DOLocalMoveY(200, waitTime));
        sequence1.Play();
        */

        /*
        foreach (TextMeshProUGUI text in titleTexts)
        {
            var sequence = DOTween.Sequence();

            sequence
                .Append(DOTween.To(() => text.color, (x) => text.color = x, new Color(text.color.r, text.color.g, text.color.b, 0), waitTime));

            sequence.Play();
        }
        */

        var sequence = DOTween.Sequence();

        sequence
            .Append(DOTween.To(() => TitleLogo.color, (x) => TitleLogo.color = x, new Color(TitleLogo.color.r, TitleLogo.color.g, TitleLogo.color.b, 0), waitTime));

        sequence.Play();

        yield return new WaitForSeconds(waitTime + 0.2f);
        
        OffTitle();

        //yield return ContinuousController.instance.StartCoroutine(checkUpdate.CheckUpdateCoroutine());

        Opening.instance.home.SetUpHome();
    }
}
