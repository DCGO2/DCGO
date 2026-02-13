using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PatchBackground : MonoBehaviour
{
    [SerializeField] bool _isLauncher = false;
    async void Start()
    {
        Image image = GetComponent<Image>();

        if (image == null)
        {
            return;
        }

        Sprite sprite = await StreamingAssetsUtility.GetSprite("Background_home", isLauncher: _isLauncher);

        image.sprite = sprite;
    }
}
