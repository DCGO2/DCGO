using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;


public class GraphicsOptionPanel : OffAnimation
{
    [SerializeField] Animator _anim;
    [SerializeField] Toggle _showBackgroundParticleToggle;

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
            OptionUtility.InitToggle(
                toggle: _showBackgroundParticleToggle,
                onToggleChanged: OnShowBackgroundParticleToggleChanged,
                value: ContinuousController.instance.showBackgroundParticle
            );
        }

        gameObject.SetActive(true);
        _anim.SetInteger("Open", 1);
        _anim.SetInteger("Close", 0);
    }

    #region Auto select mode

    public void OnShowBackgroundParticleToggleChanged(bool value)
    {
        if (ContinuousController.instance == null) return;

        OptionUtility.OnToggleChanged(
            value: value,
            toggle: _showBackgroundParticleToggle,
            onToggleChanged: OnShowBackgroundParticleToggleChanged,
            settingRef: ref ContinuousController.instance.showBackgroundParticle,
            saveAction: ContinuousController.instance.SaveShowBackgroundParticle
        );
    }

    public void HandleShowBackgroundParticleToggle()
    {
        OnShowBackgroundParticleToggleChanged(!_showBackgroundParticleToggle.isOn);
    }
    #endregion
}
