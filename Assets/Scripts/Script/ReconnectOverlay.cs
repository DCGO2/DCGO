using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//Full-screen blocker shown while a player is reconnecting, built at runtime so no scene or prefab changes are needed
public class ReconnectOverlay : MonoBehaviour
{
    GameObject _overlayRoot = null;
    Text _overlayText = null;
    string _message = "";
    float _deadline = 0f;

    void Update()
    {
        if (_overlayText != null)
        {
            int seconds = Mathf.Max(0, Mathf.CeilToInt(_deadline - Time.unscaledTime));
            _overlayText.text = $"{_message}\n{seconds}s";
        }
    }

    void OnDisable()
    {
        Hide();
    }

    public void Show(string message, float seconds)
    {
        _message = message;
        _deadline = Time.unscaledTime + seconds;

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
        }

        _overlayRoot.SetActive(true);

        //Drop keyboard/controller focus so Enter/Space can't press a hidden button
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void Hide()
    {
        if (_overlayRoot != null)
        {
            Destroy(_overlayRoot);
            _overlayRoot = null;
            _overlayText = null;
        }
    }

    void OnDestroy()
    {
        Hide();
    }

    static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
