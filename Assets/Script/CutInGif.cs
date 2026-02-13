using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CutInGif : MonoBehaviour
{
    Sprite[] frames;
    float framesPerSecond = 10f;
    Image image;

    public Sprite[] RedBackGrounds;
    public Sprite[] BlueBackGrounds;
    private void Start()
    {
        image = GetComponent<Image>();

        SetRedBackGround();
    }


    void Update()
    {
        float index = Time.time * framesPerSecond;
        index = (int)index;
        index = index % frames.Length;
        image.sprite = frames[(int)index];
    }

    public void SetRedBackGround()
    {
        frames = RedBackGrounds;
    }

    public void SetBlueBackGround()
    {
        frames = BlueBackGrounds;
    }
}
