using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LoadingObject : MonoBehaviour
{
    public Animator anim;

    public Text LoadingText;

    public GameObject AnimationParent;

    public GameObject Meat;
    public GameObject Agumon;

    Vector3 defaultAgumonPos = new Vector3(250, 0 ,0); 

    public float speed = 700f;

    public IEnumerator StartLoading(string DefaultString)
    {
        this.transform.parent.gameObject.SetActive(true);
        this.gameObject.SetActive(true);
        anim.SetInteger("Close", 0);
        LoadingText.gameObject.SetActive(true);

        yield return new WaitWhile(() => !this.gameObject.activeSelf || !this.transform.parent.gameObject.activeSelf);

        if(ContinuousController.instance != null)
        {
            ContinuousController.instance.StartCoroutine(SetLoadingText(DefaultString));
        }

        else
        {
            StartCoroutine(SetLoadingText(DefaultString));
        }

        if (AnimationParent.activeSelf)
        {
            Agumon.transform.localPosition = defaultAgumonPos;
            moveAgumonCoroutine = StartCoroutine(moveAgumonIEnumerator());
        }
    }

    Coroutine moveAgumonCoroutine = null;

    IEnumerator SetLoadingText(string DefaultString)
    {
        float waitTime = 0.18f;

        int count = 0;

        while(true)
        {
            count++;

            if(count >= 4)
            {
                count = 0;
            }

            LoadingText.text = DefaultString;

            for(int i=0;i<count;i++)
            {
                LoadingText.text += ".";
            }

            yield return new WaitForSeconds(waitTime);
        }
    }

    IEnumerator moveAgumonIEnumerator()
    {
        while(true)
        {
            Agumon.transform.localPosition -= new Vector3(speed*Time.deltaTime, 0 ,0);

            if(Mathf.Abs(Agumon.transform.localPosition.x - Meat.transform.localPosition.x) < speed * Time.deltaTime * 2)
            {
                Agumon.transform.localPosition = Meat.transform.localPosition;
                yield break;
            }

            yield return null;
        }
    }

    public IEnumerator EndLoading()
    {
        if(moveAgumonCoroutine != null)
        {
            StopCoroutine(moveAgumonCoroutine);
        }

        if(AnimationParent.activeSelf)
        {
            bool end = false;
            Sequence sequence = DOTween.Sequence();

            sequence
                .Append(Agumon.transform.DOLocalMove(Meat.transform.localPosition, 0.1f))
                .AppendCallback(() => end = true);

            sequence.Play();

            yield return new WaitWhile(() => !end);
            end = false;
        }
        
        anim.SetInteger("Close", 1);

        LoadingText.gameObject.SetActive(false);

        yield return new WaitWhile(() => this.gameObject.activeSelf);

        if (AnimationParent.activeSelf)
        {
            Agumon.transform.localPosition = defaultAgumonPos;
        }
    }

    public void Off()
    {
        this.gameObject.SetActive(false);
    }
}
