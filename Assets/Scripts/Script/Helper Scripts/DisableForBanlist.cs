using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class DisableForBanlist : MonoBehaviour
{
    private Button _button;

    // Start is called before the first frame update
    void Awake()
    {
        _button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (ContinuousController.instance == null)
            return;

        _button.interactable = ContinuousController.instance.useBanlist;
    }
}
