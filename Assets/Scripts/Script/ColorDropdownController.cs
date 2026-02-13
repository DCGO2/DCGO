using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Threading.Tasks;

public class ColorDropdownController : MonoBehaviour
{
    [SerializeField] Dropdown _colorDropdown;

    public void OnColorDropdownChanged(int value)
    {
        if (_colorDropdown == null) return;
        if (_colorDropdown.captionText == null) return;

        _colorDropdown.captionText.color = Color.white;

        int colorIndex = value - 1;

        if (colorIndex < 0) return;
        if (_colorDropdown.options.Count - 1 < colorIndex) return;
        if (Enum.GetValues(typeof(CardColor)).Length - 1 < colorIndex) return;

        CardColor cardColor = (CardColor)Enum.ToObject(typeof(CardColor), colorIndex);

        if (cardColor == CardColor.Yellow || cardColor == CardColor.White)
        {
            _colorDropdown.captionText.color = Color.black;
        }
    }

    public async void OnClickColorDropdown()
    {
        await Task.Delay(TimeSpan.FromSeconds(Time.deltaTime));

        SetBlackItemLabelColor(GameObject.Find("Item 3: Y"));

        SetBlackItemLabelColor(GameObject.Find("Item 5: W"));

        static void SetBlackItemLabelColor(GameObject item)
        {
            if (item != null)
            {
                if (item.transform.childCount >= 3)
                {
                    Transform child = item.transform.GetChild(2);

                    if (child != null)
                    {
                        Text text = child.GetComponent<Text>();

                        if (text != null)
                        {
                            text.color = Color.black;
                        }
                    }
                }
            }
        }
    }
}
