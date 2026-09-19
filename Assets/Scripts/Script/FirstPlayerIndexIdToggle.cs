using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class FirstPlayerIndexIdToggle : MonoBehaviour
{
    [SerializeField] Toggle _toggle;
    [SerializeField] int _firstPlayerIndexID;
    public Toggle Toggle => _toggle;
    public int FirstPlayerIndexID => _firstPlayerIndexID;
    public UnityAction<int> OnClickAction { get; set; } = null;

    public void OnClick()
    {
        OnClickAction?.Invoke(FirstPlayerIndexID);
    }
}
