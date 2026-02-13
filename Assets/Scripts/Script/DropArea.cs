using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropArea : MonoBehaviour
{
	//Whether this DropArea is a child of the object in question
	public bool IsChildThisDropArea(GameObject Parent)
   {
		return IsChild(Parent,  this.gameObject);
   }

	public static bool IsChild(GameObject Parent, GameObject child)
    {
		List<GameObject> list = GetAllChildren.GetAll(Parent);

		list.Add(Parent);

		foreach (GameObject child1 in list)
		{
			if (child1 == child.gameObject)
			{
				return true;
			}
		}

		return false;
	}

	[Header("DropPanel")]
	public GameObject DropPanel;

	[Header("OnPointerPanel")]
	public GameObject OnPointerPanel;

	private void Start()
	{
		if(DropPanel != null)
		{
			DropPanel.SetActive(false);
		}
		
		if(OnPointerPanel != null)
		{
			OnPointerPanel.SetActive(false);
		}
	}

	public void OnDropPanel()
	{
		if (DropPanel != null)
		{
			DropPanel.SetActive(true);
		}

		if (OnPointerPanel != null)
		{
			OnPointerPanel.SetActive(false);
		}
	}

	public void OffDropPanel()
	{
		if (DropPanel != null)
		{
			DropPanel.SetActive(false);
		}
	}

	public void OnPointerEnter()
	{
		if (OnPointerPanel != null)
		{
			OnPointerPanel.SetActive(true);
		}
	}

	public void OnPointerExit()
	{
		if (OnPointerPanel != null)
		{
			OnPointerPanel.SetActive(false);
		}
	}
}

public static class GetAllChildren
{
	public static List<GameObject> GetAll(this GameObject obj)
	{
		List<GameObject> allChildren = new List<GameObject>();
		GetChildren(obj, ref allChildren);
		return allChildren;
	}

	//Retrieve child elements and add them to the list
	public static void GetChildren(GameObject obj, ref List<GameObject> allChildren)
	{
		Transform children = obj.GetComponentInChildren<Transform>();
		//Ends if there are no child elements.
		if (children.childCount == 0)
		{
			return;
		}
		foreach (Transform ob in children)
		{
			allChildren.Add(ob.gameObject);
			GetChildren(ob.gameObject, ref allChildren);
		}
	}
}
