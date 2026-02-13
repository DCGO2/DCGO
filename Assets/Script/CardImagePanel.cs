using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.Events;

public class CardImagePanel : OffAnimation
{
    [SerializeField] TextMeshProUGUI _infoText;
    [SerializeField] TMP_InputField _cardIDsInput;
    [SerializeField] GameObject _lackCardImagesObject;
    [SerializeField] Animator _anim;

    bool _isOpen = false;

    public void OnClickOpenCardImagePanelButton()
    {
        if (_isOpen)
        {
            CloseCardImagePanel();
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

            ContinuousController.instance.StartCoroutine(Open());
        }
    }

    public IEnumerator Open()
    {
        _isOpen = true;
        gameObject.SetActive(true);
        _anim.SetInteger("Open", 1);
        _anim.SetInteger("Close", 0);
        _lackCardImagesObject.gameObject.SetActive(false);
        _infoText.text = "";
        _cardIDsInput.text = "";

        yield return new WaitForSeconds(0.25f);

        List<CEntity_Base> lackCardImageCardEntities = ContinuousController.instance.CardList
        .Filter(cEntity_Base => !StreamingAssetsUtility.IsCardExists(cEntity_Base)).ToList();

        if (lackCardImageCardEntities.Count >= 1)
        {
            _infoText.text = LocalizeUtility.GetLocalizedString(
                                EngMessage: $"The following {lackCardImageCardEntities.Count} card images are not set.",
                                JpnMessage: $"以下の {lackCardImageCardEntities.Count}枚の\nカード画像がセットされていません。"
                            );

            _lackCardImagesObject.gameObject.SetActive(true);

            _cardIDsInput.text = string.Join(", ", lackCardImageCardEntities.Map(cEntity_Base => cEntity_Base.CardSpriteName));
        }

        else
        {
            _infoText.text = LocalizeUtility.GetLocalizedString(
                                EngMessage: "All card images are set.",
                                JpnMessage: "カード画像は全てセットされています。"
                            );
        }
    }

    public void Close()
    {
        Close_(true);
    }

    public void CloseCardImagePanel()
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

    public void OnClickCopyButton()
    {
        GUIUtility.systemCopyBuffer = _cardIDsInput.text;

        List<UnityAction> Commands = new List<UnityAction>()
                {
                    null
                };

        List<string> CommandTexts = new List<string>()
                {
                    "OK"
                };

        Opening.instance.SetUpActiveYesNoObject(
            Commands,
            CommandTexts,
            LocalizeUtility.GetLocalizedString(
            EngMessage: "The list of card image names has been copied to the clipboard!",
            JpnMessage: "カード画像名一覧を\nクリップボードにコピーしました!"
        ),
            true);
    }
}
