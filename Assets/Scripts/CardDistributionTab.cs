using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class CardDistributionTab : MonoBehaviour
{
    [SerializeField] int MaxCount;
    [SerializeField] TextMeshProUGUI CountText;
    [SerializeField] Image Bar;
    public Func<CEntity_Base, bool> CardCondition = null;

    float maxLength = 22f;

    public void SetCardDistributionTab(DeckData deckData)
    {
        if(CardCondition != null)
        {
            int count = deckData.AllDeckCards().Count(CardCondition);

            float ratio = (float)count / MaxCount;

            if(ratio >= 1)
            {
                ratio = 1;
            }

            CountText.text = count.ToString();

            ((RectTransform)Bar.transform).sizeDelta = new Vector2(((RectTransform)Bar.transform).sizeDelta.x, maxLength * ratio);
        }
    }
}
