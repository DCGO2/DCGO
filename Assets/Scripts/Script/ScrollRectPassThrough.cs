using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Attach this to any interactive UI element (Button, Toggle, etc.) inside a ScrollRect
/// so that mouse wheel scroll events are forwarded to the parent ScrollRect instead of
/// being swallowed by the child element.
/// </summary>
public class ScrollRectPassThrough : MonoBehaviour, IScrollHandler
{
  private ScrollRect _parentScrollRect;

  private void Start()
  {
    _parentScrollRect = GetComponentInParent<ScrollRect>();
  }

  public void OnScroll(PointerEventData eventData)
  {
    if (_parentScrollRect != null)
    {
      _parentScrollRect.OnScroll(eventData);
    }
  }
}
