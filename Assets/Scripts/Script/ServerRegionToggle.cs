using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ServerRegionToggle : MonoBehaviour
{
    [SerializeField] string _region;
    [SerializeField] Toggle _toggle;
    public string Region => _region;
    public Toggle Toggle => _toggle;

    public UnityAction<string> OnClickAction { get; set; } = null;
    public void OnClick()
    {
        OnClickAction?.Invoke(_region);
        _toggle.isOn = true;
    }
}
