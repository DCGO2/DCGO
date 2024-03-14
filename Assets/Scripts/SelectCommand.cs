using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class SelectCommand : MonoBehaviour
{
    public UnityEvent OnClickEvent { get; set; } = new UnityEvent();

    [Header("Button List")]
    public List<Button> Buttons = new List<Button>();

    public void OpenSelectCommandButton(string _text, UnityAction OnClickAction, int ButtonIndex)
    {
        this.gameObject.SetActive(true);
        OnClickEvent.RemoveAllListeners();
        OnClickEvent.AddListener(OnClickAction);
        OnClickEvent.AddListener(CloseSelectCommandButton);

        for(int i=0;i<Buttons.Count;i++)
        {
            if(i == ButtonIndex)
            {
                Buttons[i].gameObject.SetActive(true);

                Buttons[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = _text;
            }

            else
            {
                Buttons[i].gameObject.SetActive(false);
            }
        }
    }

    public void CloseSelectCommandButton()
    {
        this.gameObject.SetActive(false);
    }

    public void OnClick()
    {
        GManager.instance.PlayDecisionSE();
        OnClickEvent?.Invoke();
    }
}