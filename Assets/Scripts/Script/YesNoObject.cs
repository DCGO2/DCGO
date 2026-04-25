using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class YesNoObject : MonoBehaviour
{
    private static readonly int CloseHash = Animator.StringToHash("Close");
    private static readonly int OpenHash = Animator.StringToHash("Open");
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

            if (i < OnClickActions.Count)
            {
                Buttons[i].gameObject.SetActive(true);

                Buttons[i].transform.GetChild(0).GetComponent<Text>().text = CommandTexts[i];

                int k = i;

                Buttons[i].OnClickAction = () => 
                {
                    if (!Buttons[k].GetComponent<Button>().interactable)
                        return;

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
        anim.SafeSetInt(OpenHash, 1);
        anim.SafeSetInt(CloseHash, 0);
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

        anim.SafeSetInt(OpenHash, 0);
        anim.SafeSetInt(CloseHash, 1);
    }
}
