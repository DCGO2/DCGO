using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CircleFill : MonoBehaviour
{
    [SerializeField] Image image;

    [SerializeField] float fillTime = 3f;
    private void OnEnable()
    {
        image.fillAmount = 0;
    }

    private void Update()
    {
        if(fillTime != 0f)
        {
            float speed = 1f / fillTime;

            image.fillAmount += speed * Time.deltaTime;
        }
    }
}
