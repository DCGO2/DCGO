using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
public class Draggable_Window : Draggable
{
    Vector3 startInputLocalPos = Vector3.zero;
    Vector3 startLocalPos = Vector3.zero;
    [SerializeField] float minX;
    [SerializeField] float maxX;
    [SerializeField] float minY;
    [SerializeField] float maxY;
    [SerializeField] Vector3 defaultPos;

    private void OnEnable()
    {
        transform.localPosition = defaultPos;
    }
    public override void OnBeginDrag(BaseEventData eventData)
    {
        startInputLocalPos = GetLocalPosition(Input.mousePosition, this.transform);
        startLocalPos = this.self.localPosition;
    }

    public override void OnDrag(BaseEventData eventData)
    {
        this.self.localPosition = GetLocalPosition(Input.mousePosition, this.transform) - startInputLocalPos + startLocalPos;
    }

    public override void OnEndDrag(BaseEventData eventData)
    {
        while (!isWithinRange())
        {
            if(minX > this.transform.localPosition.x)
            {
                this.transform.localPosition += new Vector3(0.1f, 0f);
            }

            else if (maxX < this.transform.localPosition.x)
            {
                this.transform.localPosition -= new Vector3(0.1f, 0f);
            }

            else if (minY > this.transform.localPosition.y)
            {
                this.transform.localPosition += new Vector3(0f, 0.1f);
            }

            else if (maxY < this.transform.localPosition.y)
            {
                this.transform.localPosition -= new Vector3(0f, 0.1f);
            }
        }

        bool isWithinRange()
        {
            if(minX <= this.transform.localPosition.x && this.transform.localPosition.x <= maxX)
            {
                if (minY <= this.transform.localPosition.y && this.transform.localPosition.y <= maxY)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
