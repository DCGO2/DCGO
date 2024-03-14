using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class Draggable_HandCard : Draggable
{
    public HandCard handCard;

    float shrink = 0.6f;

    public bool isExpand = false;

    public Vector3 startScale = new Vector3(1.2f, 1.2f, 1);

    public float DefaultY = 0;

    public bool CanPointerEnterExitAction { get; set; } = true;

    public void OnPointerEnter(BaseEventData eventData)
    {
        if (!CanPointerEnterExitAction)
        {
            return;
        }

        if (handCard.cardSource != null)
        {
            if (!handCard.cardSource.Owner.isYou)
            {
                return;
            }

            if (handCard.cardSource.Owner.HandTransform.GetComponent<HandContoller>() != null)
            {
                if (handCard.cardSource.Owner.HandTransform.GetComponent<HandContoller>().isDragging)
                {
                    return;
                }
            }

            for (int i = 0; i < this.transform.parent.childCount; i++)
            {
                if (this.transform.parent == handCard.cardSource.Owner.HandTransform || this.transform.parent == GManager.instance.selectCardPanel.scrollRect.content)
                {
                    if (this.transform.parent.GetChild(i) == this.transform)
                    {
                        this.isExpand = true;
                        this.transform.localScale = startScale * 1.2f;
                        this.transform.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.08f);
                    }

                    else
                    {
                        this.transform.parent.GetChild(i).GetComponent<Draggable_HandCard>().isExpand = false;
                        this.transform.parent.GetChild(i).localScale = startScale;
                        this.transform.parent.GetChild(i).GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
                    }
                }
            }
        }

    }

    public void OnPointerExit(BaseEventData eventData)
    {
        if (!CanPointerEnterExitAction)
        {
            return;
        }

        if (handCard.cardSource != null)
        {
            if (!handCard.cardSource.Owner.isYou)
            {
                return;
            }

            if (handCard.cardSource.Owner.HandTransform.GetComponent<HandContoller>() != null)
            {
                if (handCard.cardSource.Owner.HandTransform.GetComponent<HandContoller>().isDragging)
                {
                    return;
                }
            }

            if (isExpand)
            {
                if (this.transform.parent == handCard.cardSource.Owner.HandTransform || this.transform.parent == GManager.instance.selectCardPanel.scrollRect.content)
                {
                    for (int i = 0; i < this.transform.parent.childCount; i++)
                    {
                        this.transform.parent.GetChild(i).GetComponent<Draggable_HandCard>().isExpand = false;
                        this.transform.parent.GetChild(i).localScale = startScale;
                        this.transform.parent.GetChild(i).GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
                        this.transform.parent.GetChild(i).transform.localPosition = new Vector3(this.transform.parent.GetChild(i).transform.localPosition.x, DefaultY, 0);
                    }
                }

            }
        }

    }

    public override void OnBeginDrag(BaseEventData eventData)
    {
        if (handCard.cardSource != null)
        {
            if (!handCard.cardSource.Owner.isYou)
            {
                return;
            }

            if (handCard.CanDrag)
            {
                base.OnBeginDrag(eventData);

                isExpand = false;
                handCard.cardSource.Owner.HandTransform.GetComponent<HandContoller>().isDragging = true;

                this.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
                this.transform.localScale = startScale * shrink;
                handCard.CardImage.color = new Color(1, 1, 1, 0.27f);

                for (int i = 0; i < handCard.cardSource.Owner.HandTransform.childCount; i++)
                {
                    if (handCard.cardSource.Owner.HandTransform.GetChild(i).GetComponent<Draggable_HandCard>() != this)
                    {
                        handCard.cardSource.Owner.HandTransform.GetChild(i).GetComponent<Draggable_HandCard>().isExpand = false;
                        handCard.cardSource.Owner.HandTransform.GetChild(i).localScale = startScale;
                        handCard.cardSource.Owner.HandTransform.GetChild(i).GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
                        handCard.cardSource.Owner.HandTransform.GetChild(i).transform.localPosition = new Vector3(handCard.cardSource.Owner.HandTransform.GetChild(i).transform.localPosition.x, DefaultY, 0);
                    }
                }

                oldChildIndex = this.transform.GetSiblingIndex();
                oldParent = this.transform.parent;

                if (oldParent != null)
                {
                    if (oldParent.GetComponent<GridLayoutGroup>() != null)
                    {
                        oldParent.GetComponent<GridLayoutGroup>().enabled = false;
                    }
                }

                this.transform.SetSiblingIndex(this.transform.parent.childCount - 1);

                handCard.BeginDragAction?.Invoke(handCard);
            }
        }
    }

    public override void OnDrag(BaseEventData eventData)
    {
        if (handCard.cardSource != null)
        {
            if (!handCard.cardSource.Owner.isYou)
            {
                return;
            }

            if (!handCard.cardSource.Owner.HandTransform.GetComponent<HandContoller>().isDragging)
            {
                return;
            }

            if (handCard.CanDrag)
            {
                base.OnDrag(eventData);

                this.transform.SetParent(GManager.instance.You.HandTransform);
                this.transform.localScale = startScale * shrink;
                handCard.CardImage.color = new Color(1, 1, 1, 0.27f);

                if (GetRaycastArea() != null)
                {
                    handCard.OnDragAction?.Invoke(GetRaycastArea());
                }
            }
        }

    }

    public override void OnEndDrag(BaseEventData eventData)
    {
        if (handCard.cardSource != null)
        {
            if (!handCard.cardSource.Owner.isYou)
            {
                return;
            }

            if (!handCard.cardSource.Owner.HandTransform.GetComponent<HandContoller>().isDragging)
            {
                return;
            }

            if (handCard.CanDrag)
            {
                base.OnEndDrag(eventData);

                handCard.cardSource.Owner.HandTransform.GetComponent<HandContoller>().isDragging = false;
                this.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
                this.transform.localScale = startScale;
                handCard.CardImage.color = new Color(1, 1, 1, 1);

                this.transform.SetParent(GManager.instance.canvas.transform);

                if (GetRaycastArea() != null)
                {
                    handCard.EndDragAction?.Invoke(GetRaycastArea());
                }
            }
        }
    }
}
