using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using DG.Tweening;

public class TargetArrow : MonoBehaviour
{
    public RectTransform RootRect;
    public RectTransform TipRect;
    public RectTransform CanvasRect;

    bool endShowTargetArrow = false;
    bool isDrag = false;

    //being dragged
    public void SetTargetArrow(Vector3 InitialPosition, Vector3 TargetPosition)
    {
        isDrag = true;

        this.gameObject.SetActive(true);

        GetComponent<RectTransform>().localPosition = InitialPosition;

        Vector3 DirectionVector = TargetPosition - InitialPosition;

        float shita = 0;

        if (Mathf.Abs(DirectionVector.y) != 0)
        {
            // Specify rotation angle
            shita = Mathf.Atan2(DirectionVector.x, DirectionVector.y) * 180 / Mathf.PI;
        }

        if (Mathf.Abs(DirectionVector.y) < 3)
        {
            if (DirectionVector.x > 0)
            {
                shita = 90;
            }

            else
            {
                shita = 270;
            }
        }

        shita *= -1;

        RootRect.localRotation = Quaternion.Euler(new Vector3(0, 0, shita));

        //Get maximum length
        float MaxLength = DirectionVector.magnitude;

        RootRect.sizeDelta = new Vector2(RootRect.sizeDelta.x, MaxLength);

        TipRect.localPosition = new Vector3(0, RootRect.sizeDelta.y - 13, 0);
    }

    public void OffTargetArrow()
    {
        this.gameObject.SetActive(false);
    }

    public Coroutine OnTargetArrow(Vector3 InitialPosition, Vector3 targetPosition, FieldPermanentCard StartFieldUnitCard, FieldPermanentCard EndFieldUnitCard)
    {
        this.gameObject.SetActive(true);

        this.StartFieldUnitCard = StartFieldUnitCard;
        this.EndFieldUnitCard = EndFieldUnitCard;

        return StartCoroutine(OnTargetArrowCoroutine(InitialPosition, targetPosition));
    }

    //Temporary target display
    public IEnumerator OnTargetArrowCoroutine(Vector3 InitialPosition, Vector3 TargetPosition)
    {
        ContinuousController.instance.PlaySE(GManager.instance.TargetArrowSE);

        isDrag = false;

        GetComponent<RectTransform>().localPosition = InitialPosition;

        Vector3 DirectionVector = TargetPosition - InitialPosition;

        float shita = 0;

        if (Mathf.Abs(DirectionVector.y) != 0)
        {
            //Specify rotation angle
            shita = Mathf.Atan2(DirectionVector.x, DirectionVector.y) * 180 / Mathf.PI;
        }

        if (Mathf.Abs(DirectionVector.y) < 3)
        {
            if (DirectionVector.x > 0)
            {
                shita = 90;
            }

            else
            {
                shita = 270;
            }
        }

        shita *= -1;

        RootRect.localRotation = Quaternion.Euler(new Vector3(0, 0, shita));

        //Get maximum length
        float MaxLength = DirectionVector.magnitude;

        //Time interval to display
        float extendTime = 0.17f;

        //Number of times to display
        int ShowCount = 2;

        for (int i = 0; i < ShowCount; i++)
        {
            RootRect.sizeDelta = new Vector2(RootRect.sizeDelta.x, 0);

            TipRect.localPosition = new Vector3(0, RootRect.sizeDelta.y - 13, 0);

            bool end = false;

            var sequence = DOTween.Sequence();

            sequence
                .Append(DOTween.To(() => RootRect.sizeDelta, (x) => RootRect.sizeDelta = x, new Vector2(RootRect.sizeDelta.x, MaxLength), extendTime / ShowCount))
                .AppendCallback(() => { end = true; sequence.Kill(); });

            sequence.Play();

            while (!end)
            {
                TipRect.localPosition = new Vector3(0, RootRect.sizeDelta.y - 13, 0);

                yield return null;
            }

            RootRect.sizeDelta = new Vector2(RootRect.sizeDelta.x, MaxLength);

            TipRect.localPosition = new Vector3(0, RootRect.sizeDelta.y - 13, 0);

            float _waitTime = 0.07f;

            yield return new WaitForSeconds(_waitTime);
        }

        float waitTime = 0.07f;

        yield return new WaitForSeconds(waitTime);

        endShowTargetArrow = true;

        StartCoroutine(ChangePositionTargetAwwor());
    }

    FieldPermanentCard StartFieldUnitCard;
    FieldPermanentCard EndFieldUnitCard;
    public bool Destroyed { get; set; }

    IEnumerator ChangePositionTargetAwwor()
    {
        while (true)
        {
            if (Destroyed)
            {
                yield break;
            }

            yield return new WaitForSeconds(Time.deltaTime);

            if (Destroyed)
            {
                yield break;
            }

            bool end = true;

            if (this != null)
            {
                if (this.gameObject != null)
                {
                    if (!isDrag)
                    {
                        if (endShowTargetArrow)
                        {
                            if (StartFieldUnitCard != null && EndFieldUnitCard != null)
                            {
                                if (StartFieldUnitCard.ThisPermanent != null)
                                {
                                    if (StartFieldUnitCard.ThisPermanent.TopCard != null)
                                    {
                                        if (EndFieldUnitCard.ThisPermanent != null)
                                        {
                                            if (EndFieldUnitCard.ThisPermanent.TopCard != null)
                                            {
                                                end = false;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (end)
            {
                yield break;
            }

            else
            {
                this.gameObject.SetActive(true);

                GetComponent<RectTransform>().localPosition = StartFieldUnitCard.GetLocalCanvasPosition() + StartFieldUnitCard.ThisPermanent.TopCard.Owner.playerUIObjectParent.localPosition;

                Vector3 DirectionVector = (EndFieldUnitCard.GetLocalCanvasPosition() + EndFieldUnitCard.ThisPermanent.TopCard.Owner.playerUIObjectParent.localPosition) - (StartFieldUnitCard.GetLocalCanvasPosition() + StartFieldUnitCard.ThisPermanent.TopCard.Owner.playerUIObjectParent.localPosition);

                float shita = 0;

                if (Mathf.Abs(DirectionVector.y) != 0)
                {
                    //Specify rotation angle
                    shita = Mathf.Atan2(DirectionVector.x, DirectionVector.y) * 180 / Mathf.PI;
                }

                if (Mathf.Abs(DirectionVector.y) < 3)
                {
                    if (DirectionVector.x > 0)
                    {
                        shita = 90;
                    }

                    else
                    {
                        shita = 270;
                    }
                }

                shita *= -1;

                RootRect.localRotation = Quaternion.Euler(new Vector3(0, 0, shita));

                //Get maximum length
                float MaxLength = DirectionVector.magnitude;

                RootRect.sizeDelta = new Vector2(RootRect.sizeDelta.x, MaxLength);

                TipRect.localPosition = new Vector3(0, RootRect.sizeDelta.y - 13, 0);
            }
        }
    }
}
