using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LanguagePanel : OffAnimation
{
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
            foreach (LanguageToggle languageToggle in _languageToggles)
            {
                void OnToggleChange(Language language)
                {
                    ContinuousController.instance.language = language;
                    ContinuousController.instance.SaveLanguage();
                }

                languageToggle.OnClickAction = OnToggleChange;

                languageToggle.Toggle.isOn = languageToggle.Language == ContinuousController.instance.language;
            }
        }

        gameObject.SetActive(true);
        _anim.SetInteger("Open", 1);
        _anim.SetInteger("Close", 0);
    }
}
