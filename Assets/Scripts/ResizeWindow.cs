using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Button = UnityEngine.UI.Button;
using Toggle = UnityEngine.UI.Toggle;
using System;

public class ResizeWindow : MonoBehaviour
{
    public Animator anim;
    public Toggle fullScreenToggle;
    public List<Button> switchWindowSizeButtons = new List<Button>();
    public void Init()
    {
        Off();
    }

    public void Open()
    {
        SetButtonsInteractable();
        gameObject.SetActive(true);
        anim.SetInteger("Open", 1);
        anim.SetInteger("Close", 0);

        if (fullScreenToggle != null)
        {
            fullScreenToggle.isOn = Screen.fullScreen;
        }
    }

    public void Off()
    {
        gameObject.SetActive(false);
    }

    public void Close()
    {
        Close_(true);
    }

    public void Close_(bool playSE)
    {
        if (playSE)
        {
            if (Opening.instance != null)
            {
                Opening.instance.PlayCancelSE();
            }

            else if (GManager.instance != null)
            {
                GManager.instance.PlayCancelSE();
            }
        }

        anim.SetInteger("Open", 0);
        anim.SetInteger("Close", 1);
    }

    public void SetUpWindowSize(string resolution)
    {
#if UNITY_EDITOR || !UNITY_STANDALONE
        return;
#endif

        if (Opening.instance != null)
        {
            Opening.instance.PlayDecisionSE();
        }

        else if (GManager.instance != null)
        {
            GManager.instance.PlayDecisionSE();
        }

        //float height = width / 16f * 9f;
        int heightstring = 4;
        if (resolution.Length <= 7)
        {
            heightstring = 3;
        }
        string temp = resolution.Substring(0, 4);
        var width = int.Parse(temp);
        temp = resolution.Substring(4, heightstring);
        var height = int.Parse(temp);
        //Screen.SetResolution((int)width, (int)height, false);
        Screen.SetResolution((int)width, (int)height, UnityEngine.Device.Screen.fullScreen);
        SetButtonsInteractable();
    }

    public async void SetFullScreen(Toggle isFullScreen)
    {
#if UNITY_EDITOR || !UNITY_STANDALONE
        Debug.Log(isFullScreen.isOn);
        return;
#endif
        if (Opening.instance != null)
        {
            Opening.instance.PlayDecisionSE();
        }

        else if (GManager.instance != null)
        {
            GManager.instance.PlayDecisionSE();
        }

        UnityEngine.Device.Screen.fullScreen = isFullScreen.isOn;
        await Task.Delay(TimeSpan.FromSeconds(Time.deltaTime));
        SetButtonsInteractable();
    }

    void SetButtonsInteractable()
    {
        return;

        foreach (Button button in switchWindowSizeButtons)
        {
            button.interactable = !UnityEngine.Device.Screen.fullScreen;
        }
    }
}
