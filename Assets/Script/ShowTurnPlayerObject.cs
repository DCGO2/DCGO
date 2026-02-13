using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowTurnPlayerObject : MonoBehaviour
{
    public Image BackGround;
    public Text TurnPlayerText;

    public bool isClose { get; set; }

    public void Init()
    {
        Off();
    }

    public void ShowTurnPlayer(Player turnPlayer)
    {
        if (turnPlayer.isYou)
        {
            TurnPlayerText.text = "Your Turn";

            BackGround.color = new Color32(121, 153, 255, 222);
        }

        else
        {
            TurnPlayerText.text = "Opponent's Turn";

            BackGround.color = new Color32(255, 131, 121, 222);
        }

        isClose = false;

        this.gameObject.SetActive(true);

        StartCoroutine(CloseCoroutine());
    }

    IEnumerator CloseCoroutine()
    {
        GetComponent<Animator>().SetInteger("Close", 0);

        yield return new WaitForSeconds(0.3f);

        GetComponent<Animator>().SetInteger("Close", 1);
    }

    public void Off()
    {
        this.gameObject.SetActive(false);
        isClose = true;
    }
}