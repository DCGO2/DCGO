using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//Full-screen blocker shown while a player is reconnecting, built at runtime so no scene or prefab changes are needed
public class ReconnectOverlay : MonoBehaviour
{
    GameObject _overlayRoot = null;
    Text _overlayText = null;
    Button _leaveButton = null;
    string _message = "";
    float _deadline = 0f;
    bool _showCountdown = false;
    Action _onLeave = null;

    void Update()
    {
        if (_overlayText != null)
        {
            if (_showCountdown)
            {
                int seconds = Mathf.Max(0, Mathf.CeilToInt(_deadline - Time.unscaledTime));
                _overlayText.text = $"{_message}\n{seconds}s";
            }
            else
            {
                _overlayText.text = _message;
            }
        }
    }

    void OnDisable()
    {
        Hide();
    }

    public void Show(string message, float seconds, Action onLeave)
    {
        _message = message;
        _deadline = Time.unscaledTime + seconds;
        _showCountdown = true;
        _onLeave = onLeave;

        ShowOverlay();
        SetLeaveButtonVisible(true);
    }

    public void ShowMessage(string message)
    {
        _message = message;
        _showCountdown = false;
        _onLeave = null;

        ShowOverlay();
        SetLeaveButtonVisible(false);
    }

    public void Hide()
    {
        if (_overlayRoot != null)
        {
            Destroy(_overlayRoot);
            _overlayRoot = null;
            _overlayText = null;
            _leaveButton = null;
        }

        _onLeave = null;
    }

    void OnDestroy()
    {
        Hide();
    }

    void ShowOverlay()
    {
        if (_overlayRoot == null)
        {
            _overlayRoot = new GameObject("ReconnectOverlay");

            Canvas canvas = _overlayRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32000;

            CanvasScaler scaler = _overlayRoot.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            _overlayRoot.AddComponent<GraphicRaycaster>();

            GameObject blocker = new GameObject("Blocker");
            blocker.transform.SetParent(_overlayRoot.transform, false);
            Image image = blocker.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.65f);
            image.raycastTarget = true;
            Stretch(blocker.GetComponent<RectTransform>());

            GameObject textObject = new GameObject("Text");
            textObject.transform.SetParent(_overlayRoot.transform, false);
            _overlayText = textObject.AddComponent<Text>();
            _overlayText.font = Font.CreateDynamicFontFromOSFont("Arial", 40);
            _overlayText.fontSize = 40;
            _overlayText.alignment = TextAnchor.MiddleCenter;
            _overlayText.color = new Color(1f, 0.67f, 0f, 1f);
            _overlayText.raycastTarget = false;
            Stretch(textObject.GetComponent<RectTransform>());

            _leaveButton = CreateLeaveButton(_overlayRoot.transform);
        }

        _overlayRoot.SetActive(true);

        //Drop keyboard/controller focus so Enter/Space can't press a hidden button
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    Button CreateLeaveButton(Transform parent)
    {
        GameObject buttonObject = new GameObject("LeaveButton");
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(360f, 80f);
        rect.anchoredPosition = new Vector2(0f, -180f);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.55f, 0.1f, 0.1f, 1f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(OnClickLeaveButton);

        GameObject labelObject = new GameObject("Label");
        labelObject.transform.SetParent(buttonObject.transform, false);
        Text label = labelObject.AddComponent<Text>();
        label.font = Font.CreateDynamicFontFromOSFont("Arial", 32);
        label.fontSize = 32;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.raycastTarget = false;
        label.text = LocalizeUtility.GetLocalizedString(
            EngMessage: "Leave match",
            JpnMessage: "対戦から退出");
        Stretch(labelObject.GetComponent<RectTransform>());

        return button;
    }

    void OnClickLeaveButton()
    {
        Action onLeave = _onLeave;

        SetLeaveButtonVisible(false);
        _onLeave = null;

        if (onLeave != null)
        {
            onLeave();
        }
    }

    void SetLeaveButtonVisible(bool visible)
    {
        if (_leaveButton != null)
        {
            _leaveButton.gameObject.SetActive(visible);
            _leaveButton.interactable = visible;
        }
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
