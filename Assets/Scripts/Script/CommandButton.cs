using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
public class CommandButton : MonoBehaviour, IPointerClickHandler
{
    public UnityAction OnClickAction;
    public bool isPositive;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isPositive)
        {
            Opening.instance.PlayDecisionSE();
        }

        else
        {
            Opening.instance.PlayCancelSE();
        }

        OnClickAction?.Invoke();
    }
}
