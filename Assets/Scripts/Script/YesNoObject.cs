using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class YesNoObject : MonoBehaviour
{
    public Text InfoText;

    public List<CommandButton> Buttons;

    public Animator anim;

    public GameObject CloseButton;

    //public Vector3 defaultPos = Vector3.zero;

    public bool CloseOnButtonClicked = true;

    public void SetUpYesNoObject(List<UnityAction> OnClickActions, List<string> CommandTexts, string _InfoText, bool CanClose)
    {
        //this.transform.localPosition = defaultPos;

        for (int i = 0; i < Buttons.Count; i++)
        {
            Buttons[i].OnClickAction = null;
        }

        for (int i = 0; i < Buttons.Count; i++)
        {
            if (i < OnClickActions.Count)
            {
                Buttons[i].gameObject.SetActive(true);

                Buttons[i].transform.GetChild(0).GetComponent<Text>().text = CommandTexts[i];

                int k = i;

                Buttons[i].OnClickAction = () => 
                {
                    OnClickActions[k]?.Invoke();
                    
                    if(this.CloseOnButtonClicked)
                    {
                        this.Close_(false);
                    }
                };
            }

            else
            {
                Buttons[i].gameObject.SetActive(false);
            }
        }

        InfoText.text = _InfoText;

        this.gameObject.SetActive(true);

        CloseButton.SetActive(CanClose);

        Open();
    }

    public void Off()
    {
        this.gameObject.SetActive(false);
        Close_(false);
    }

    public void Open()
    {
        this.gameObject.SetActive(true);
        anim.SetInteger("Open", 1);
        anim.SetInteger("Close", 0);
    }

    public void Close()
    {
        Close_(true);
    }

    public void Close_(bool playSE)
    {
        if (playSE)
        {
            Opening.instance.PlayCancelSE();
        }

        anim.SetInteger("Open", 0);
        anim.SetInteger("Close", 1);
    }
}
