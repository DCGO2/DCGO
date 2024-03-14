using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class OptionPanel : OffAnimation
{
    [SerializeField] VolumePanel _volumePanel;
    [SerializeField] ResizeWindow _resizeWindowPanel;
    [SerializeField] GameplayOption _gameplayOption;
    [SerializeField] GraphicsOptionPanel _graphicsOptionPanel;
    [SerializeField] ServerRegionPanel _serverRegionPanel;
    [SerializeField] LanguagePanel _languagePanel;
    [SerializeField] Animator _anim;
    bool _isOpen = false;

    public void OnClickOpenOptionPanelButton()
    {
        if (_isOpen)
        {
            CloseOptionPanel();
        }

        else
        {
            if (Opening.instance != null)
            {
                Opening.instance.PlayDecisionSE();
            }

            else if (GManager.instance != null)
            {
                GManager.instance.PlayDecisionSE();
            }

            Open();
        }
    }

    public void Open()
    {
        _isOpen = true;
        gameObject.SetActive(true);
        _anim.SetInteger("Open", 1);
        _anim.SetInteger("Close", 0);

        if (_volumePanel != null)
        {
            _volumePanel.Off();
        }

        if (_resizeWindowPanel != null)
        {
            _resizeWindowPanel.Off();
        }

        if (_gameplayOption != null)
        {
            _gameplayOption.Off();
        }

        if (_graphicsOptionPanel != null)
        {
            _graphicsOptionPanel.Off();
        }

        if (_serverRegionPanel != null)
        {
            _serverRegionPanel.Off();
        }

        if (_languagePanel != null)
        {
            _languagePanel.Off();
        }
    }

    public void Close()
    {
        Close_(true);
    }

    public void CloseOptionPanel()
    {
        _isOpen = false;
        bool playSE = false;

        if (gameObject.activeSelf)
        {
            playSE = true;
        }

        if (_resizeWindowPanel != null)
        {
            if (_resizeWindowPanel.gameObject.activeSelf)
            {
                playSE = true;
            }
        }

        if (_volumePanel != null)
        {
            if (_volumePanel.gameObject.activeSelf)
            {
                playSE = true;
            }
        }

        if (_gameplayOption != null)
        {
            if (_gameplayOption.gameObject.activeSelf)
            {
                playSE = true;
            }
        }

        if (_graphicsOptionPanel != null)
        {
            if (_graphicsOptionPanel.gameObject.activeSelf)
            {
                playSE = true;
            }
        }

        if (_serverRegionPanel != null)
        {
            if (_serverRegionPanel.gameObject.activeSelf)
            {
                playSE = true;
            }
        }

        if (_languagePanel != null)
        {
            if (_languagePanel.gameObject.activeSelf)
            {
                playSE = true;
            }
        }

        Close_(playSE);

        if (_resizeWindowPanel != null)
        {
            _resizeWindowPanel.Close_(false);
        }

        if (_volumePanel != null)
        {
            _volumePanel.Close_(false);
        }

        if (_gameplayOption != null)
        {
            _gameplayOption.Close_(false);
        }

        if (_graphicsOptionPanel != null)
        {
            _graphicsOptionPanel.Close_(false);
        }

        if (_serverRegionPanel != null)
        {
            _serverRegionPanel.Close_(false);
        }

        if (_languagePanel != null)
        {
            _languagePanel.Close_(false);
        }
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

        if (_volumePanel != null)
        {
            _volumePanel.Init();
        }

        if (_resizeWindowPanel != null)
        {
            _resizeWindowPanel.Init();
        }

        if (_gameplayOption != null)
        {
            _gameplayOption.Init();
        }

        if (_graphicsOptionPanel != null)
        {
            _graphicsOptionPanel.Init();
        }

        if (_serverRegionPanel != null)
        {
            _serverRegionPanel.Init();
        }

        if (_languagePanel != null)
        {
            _languagePanel.Init();
        }
    }

    public void OnClickExitGameButton()
    {
#if UNITY_EDITOR || !UNITY_STANDALONE
        return;
#endif

        Application.Quit();
    }
}
