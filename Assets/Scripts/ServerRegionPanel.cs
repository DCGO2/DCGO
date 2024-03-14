using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class ServerRegionPanel : OffAnimation
{
    [SerializeField] Animator _anim;
    [SerializeField] List<ServerRegionToggle> _serverRegionToggles = new();

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

        _anim.SetInteger("Open", 0);
        _anim.SetInteger("Close", 1);
    }

    public void Init()
    {
        Off();
    }

    public void Open()
    {
        if (ContinuousController.instance != null)
        {
            foreach (ServerRegionToggle serverRegionToggle in _serverRegionToggles)
            {
                void OnToggleChange(string region)
                {
                    ContinuousController.instance.serverRegion = region;
                    ContinuousController.instance.SaveServerRegion();
                }

                serverRegionToggle.OnClickAction = OnToggleChange;

                serverRegionToggle.Toggle.isOn = serverRegionToggle.Region == ContinuousController.instance.serverRegion;
            }
        }

        gameObject.SetActive(true);
        _anim.SetInteger("Open", 1);
        _anim.SetInteger("Close", 0);
    }
}
