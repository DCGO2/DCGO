using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LanguagePanel : OffAnimation
{
    private static readonly int CloseHash = Animator.StringToHash("Close");
    private static readonly int OpenHash = Animator.StringToHash("Open");
    [SerializeField] Animator _anim;
    [SerializeField] List<LanguageToggle> _languageToggles = new();

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
            foreach (LanguageToggle languageToggle in _languageToggles)
            {
                static void OnToggleChange(Language language)
                {
                    ContinuousController.instance.language = language;
                    ContinuousController.instance.SaveLanguage();
                }

                languageToggle.OnClickAction = OnToggleChange;

                languageToggle.Toggle.isOn = languageToggle.Language == ContinuousController.instance.language;
            }
        }

        gameObject.SetActive(true);
        _anim.SafeSetInt(OpenHash, 1);
        _anim.SafeSetInt(CloseHash, 0);
    }
}
