using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Attach this to a ScrollRect GameObject to convert vertical mouse scroll wheel
/// input into horizontal scrolling. Useful for horizontal card list panels.
/// </summary>
[RequireComponent(typeof(ScrollRect))]
public class HorizontalScrollWheelHandler : MonoBehaviour, IScrollHandler
{
    private ScrollRect _scrollRect;

    [Tooltip("Multiplier applied to the scroll delta. Increase for faster scrolling.")]
    public float scrollSpeed = 1f;

    private void Awake()
    {
        _scrollRect = GetComponent<ScrollRect>();
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (!_scrollRect.horizontal)
            return;

        // Use Y scroll delta (mouse wheel) and apply it as horizontal movement.
        // Negate so scrolling down moves right (conventional direction).
        float delta = eventData.scrollDelta.y * scrollSpeed;

        float contentWidth = _scrollRect.content.rect.width;
        float viewportWidth = _scrollRect.viewport != null
            ? _scrollRect.viewport.rect.width
            : ((RectTransform)_scrollRect.transform).rect.width;

        float scrollableWidth = contentWidth - viewportWidth;

        if (scrollableWidth <= 0f)
            return;

        _scrollRect.horizontalNormalizedPosition =
            Mathf.Clamp01(_scrollRect.horizontalNormalizedPosition - delta / scrollableWidth);
    }
}
