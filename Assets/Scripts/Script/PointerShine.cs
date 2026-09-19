using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class PointerShine : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Color EnterColor;
    public Color ExitColor;
    public Image image
    {
        get
        {
            return GetComponent<Image>();
        }
    }

    private void OnEnable()
    {
        image.color = ExitColor;// new Color32(58, 58, 58,255);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        image.color = EnterColor;// new Color32(94, 94, 94, 255);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        image.color = ExitColor;// new Color32(58, 58, 58,255);
    }
}
