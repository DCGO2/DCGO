using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
public static class OptionUtility
{
    public static void InitToggle(Toggle toggle, UnityAction<bool> onToggleChanged, bool value)
    {
        if (toggle == null) return;

        toggle.onValueChanged.RemoveAllListeners();
        toggle.isOn = value;
        toggle.onValueChanged.AddListener(onToggleChanged);
    }
    public static void OnToggleChanged(bool value, Toggle toggle, UnityAction<bool> onToggleChanged, ref bool settingRef, UnityAction saveAction)
    {
        if (toggle == null) return;
        if (saveAction == null) return;

        toggle.onValueChanged.RemoveAllListeners();
        toggle.isOn = value;
        toggle.onValueChanged.AddListener(onToggleChanged);
        settingRef = toggle.isOn;
        saveAction?.Invoke();

        if (GManager.instance != null)
        {
            GManager.instance.PlayDecisionSE();
        }

        else if (Opening.instance != null)
        {
            Opening.instance.PlayDecisionSE();
        }
    }
}
