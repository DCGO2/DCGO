using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SimpleCircleLayoutGroup : UIBehaviour, ILayoutGroup
{
	public float radius = 100;
	public float offsetAngle;

	#if UNITY_EDITOR
    protected override void OnValidate()
	{
		base.OnValidate();
		Arrange();
	}
#endif

	// Callbacks called automatically when the number of elements changes
	#region ILayoutController implementation
	public void SetLayoutHorizontal() { }
	public void SetLayoutVertical()
	{
		Arrange();
	}
#endregion

	void Arrange()
	{
		float splitAngle = 360 / transform.childCount;
		var rect = transform as RectTransform;

		for (int elementId = 0; elementId < transform.childCount; elementId++)
		{
			var child = transform.GetChild(elementId) as RectTransform;
			float currentAngle = splitAngle * elementId + offsetAngle;
			child.anchoredPosition = new Vector2(
				Mathf.Cos(currentAngle * Mathf.Deg2Rad),
				Mathf.Sin(currentAngle * Mathf.Deg2Rad)) * radius;
		}
	}
}