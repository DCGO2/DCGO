using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class TextLink : MonoBehaviour, IPointerClickHandler
{
    private TMP_Text _text;

    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(_text, eventData.position, Camera.main);

        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = _text.textInfo.linkInfo[linkIndex];

            PlayLog.OnLinkPressed?.Invoke(linkInfo.GetLinkID());
        }
    }
}
