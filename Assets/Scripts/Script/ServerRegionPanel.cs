using System.Collections.Generic;
using UnityEngine;

public class ServerRegionPanel : OffAnimation
{
    private static readonly int CloseHash = Animator.StringToHash("Close");
    private static readonly int OpenHash = Animator.StringToHash("Open");
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

        _anim.SafeSetInt(OpenHash, 0);
        _anim.SafeSetInt(CloseHash, 1);
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
                static void OnToggleChange(string region)
                {
                    ContinuousController.instance.serverRegion = region;
                    ContinuousController.instance.SaveServerRegion();
                }

                serverRegionToggle.OnClickAction = OnToggleChange;

                serverRegionToggle.Toggle.isOn = serverRegionToggle.Region == ContinuousController.instance.serverRegion;
            }
        }

        gameObject.SetActive(true);
        _anim.SafeSetInt(OpenHash, 1);
        _anim.SafeSetInt(CloseHash, 0);
    }
}
