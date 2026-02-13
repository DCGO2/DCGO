using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Threading.Tasks;
using System;
public class PatchNotes : OffAnimation
{
    [SerializeField] Animator _anim;
    [SerializeField] Text _verText;
    [SerializeField] LocalizeTMPro _localizeTMPro;
    [SerializeField][TextArea] string _text_ENG;
    [SerializeField][TextArea] string _text_JPN;
    [SerializeField] ScrollRect _scroll;
    bool _isOpen = false;

    public void ReportBug()
    {
        Application.OpenURL("https://forms.gle/GhZgGVJS1qLeUMcG8");
    }

    public void OpenPatchNotes()
    {
        Application.OpenURL("https://dcgo.online/PatchNotes.html");
    }

    public IEnumerator Open()
    {
        _verText.text = $"Patch Notes ver{ContinuousController.instance.GameVerString}";
        _isOpen = true;
        gameObject.SetActive(true);
        _anim.SetInteger("Open", 1);
        _anim.SetInteger("Close", 0);
        _scroll.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.25f);

        _scroll.gameObject.SetActive(true);
        _scroll.verticalNormalizedPosition = 1;
        _scroll.content.GetComponent<ContentSizeFitter>().SetLayoutVertical();
    }

    public void Close()
    {
        Close_(true);
    }

    public void ClosePatchNotesPanel()
    {
        _isOpen = false;
        bool playSE = false;

        if (gameObject.activeSelf)
        {
            playSE = true;
        }

        Close_(playSE);
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
        if (_localizeTMPro != null)
        {
            _localizeTMPro._text_ENG = _text_ENG;
            _localizeTMPro._text_JPN = _text_JPN;
        }

        Off();
    }
}
