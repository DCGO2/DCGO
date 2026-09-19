using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class LanguageToggle : MonoBehaviour
{
    [SerializeField] Language _language;
    [SerializeField] Toggle _toggle;
    public Language Language => _language;
    public Toggle Toggle => _toggle;

    public UnityAction<Language> OnClickAction { get; set; } = null;
    public void OnClick()
    {
        OnClickAction?.Invoke(_language);
        _toggle.isOn = true;
    }
}
