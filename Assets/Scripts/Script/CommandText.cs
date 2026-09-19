using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CommandText : MonoBehaviour
{
    public TextMeshProUGUI commandText;
    public GameObject digiXrosObject;
    public GameObject assemblyObject;

    public void Init()
    {
        Off();

        if (digiXrosObject != null)
        {
            digiXrosObject.SetActive(false);
        }
    }

    #region Open command message
    public void OpenCommandText(string Text, bool digiXros = false, bool assembly = false)
    {
        commandText.text = Text;
        this.gameObject.SetActive(true);

        GetComponent<Animator>().SetInteger("Close", 0);

        if (digiXrosObject != null)
        {
            digiXrosObject.SetActive(digiXros);
        }

        if(assemblyObject != null)
        {
            assemblyObject.SetActive(assembly);
        }
    }
    #endregion

    #region Close command message
    public void CloseCommandText()
    {
        if (!this.gameObject.activeSelf)
        {
            return;
        }

        GetComponent<Animator>().SetInteger("Close", 1);

        if (digiXrosObject != null)
        {
            digiXrosObject.SetActive(false);
        }
    }

    public void Off()
    {
        this.gameObject.SetActive(false);
    }
    #endregion
}