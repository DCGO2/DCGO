using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class AdjustUIColor : MonoBehaviour
{
    private Image _image;

    // Start is called before the first frame update
    void Awake()
    {
        _image = GetComponent<Image>();
    }

    private void Start()
    {
        if (_image.sprite == null)
            _image.color = Color.gray;
    }
}
