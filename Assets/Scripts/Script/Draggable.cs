using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Draggable : MonoBehaviour
{
    private Transform root;
    [HideInInspector] public Transform area;
    [HideInInspector] public Transform self;
    public CanvasGroup canvasGroup = null;

    public int oldChildIndex { get; set; } = -1;
    public Transform oldParent { get; set; }





    public void Awake()
    {
        this.self = this.transform;
        this.area = this.self.parent;
        this.root = this.area.parent;
    }

    public virtual void OnPointerEnter()
    {

    }

    public virtual void OnPointerExit()
    {

    }

    public virtual void OnBeginDrag(BaseEventData eventData)
    {

    }

    public virtual void OnDrag(BaseEventData eventData)
    {
        this.self.localPosition = GetLocalPosition(Input.mousePosition, this.transform);
    }

    public static Vector3 GetLocalPosition(Vector3 position, Transform transform)
    {
        Camera MainCamera = null;

        if (GManager.instance != null)
        {
            MainCamera = GManager.instance.camara;
        }

        else if (Opening.instance != null)
        {
            MainCamera = Opening.instance.MainCamera;
        }

        if (MainCamera != null)
        {
            // Converts coordinates on the screen (Screen Point) to local coordinates on the RectTransform
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                transform.parent.GetComponent<RectTransform>(),
                position,
                MainCamera,
                out var result);
            return new Vector3(result.x, result.y, 0);
        }

        return Vector3.zero;
    }

    public virtual void OnEndDrag(BaseEventData eventData)
    {
        if (this != null)
        {
            if (this.canvasGroup != null)
            {
                // Temporarily disable UI functions
                //this.canvasGroup.blocksRaycasts = false;

                // Restore UI functionality
                this.canvasGroup.blocksRaycasts = true;

                if (GetRaycastArea() != null)
                {
                    if (GetRaycastArea().Count == 0)
                    {
                        ReturnDefaultPosition();
                    }
                }

                if (oldParent != null)
                {
                    if (oldParent.GetComponent<GridLayoutGroup>() != null)
                    {
                        oldParent.GetComponent<GridLayoutGroup>().enabled = true;
                    }
                }
            }
        }

    }

    public void ReturnDefaultPosition()
    {
        if (oldParent != null)
        {
            this.transform.SetParent(oldParent);
        }

        if (oldChildIndex >= 0)
        {
            this.transform.SetSiblingIndex(oldChildIndex);
        }
    }

    /// <summary>
    /// Obtains the DropArea at the point where the event occurs.
    /// </summary>
    /// <param name="eventData">event data</param>
    /// <returns>DropArea</returns>
    public static List<DropArea> GetRaycastArea()
    {
        PointerEventData pointer = new PointerEventData(EventSystem.current);

        List<RaycastResult> results = new List<RaycastResult>();
        // Ray skip to mouse pointer position and save hits
        pointer.position = Input.mousePosition;
        EventSystem.current.RaycastAll(pointer, results);

        List<DropArea> DropAreas = new List<DropArea>();

        // Name of the hit UI
        foreach (RaycastResult target in results)
        {
            DropArea dropArea = target.gameObject.GetComponent<DropArea>();
            if (dropArea != null)
            {
                DropAreas.Add(target.gameObject.GetComponent<DropArea>());
            }
        }

        return DropAreas;

    }
}