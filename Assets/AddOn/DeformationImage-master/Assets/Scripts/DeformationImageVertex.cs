using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class DeformationImageVertex : MonoBehaviour
{
    public UIVertex vertex = new UIVertex();
    public DeformationImage deformationImage;

    RectTransform _rectTransform;
    public RectTransform rectTransform
    {
        get
        {
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }
            return _rectTransform;
        }
    }

    void Awake()
    {
        if (deformationImage == null)
        {
            deformationImage = GetComponentInParent<DeformationImage>();
        }
    }

    public void Init(DeformationImage di)
    {
        this.deformationImage = di;
    }

    void Update()
    {
        var lb = -deformationImage.rectTransform.rect.size * rectTransform.anchorMin;
        var vt = (rectTransform.anchoredPosition - lb) / deformationImage.rectTransform.rect.size;
        vertex.uv0 = vt;
        vertex.position = transform.localPosition;
        vertex.color = deformationImage.GraphicColor;
    }
}
