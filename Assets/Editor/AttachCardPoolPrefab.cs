using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using System;
using System.Reflection;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon;
using Photon.Pun;
using System.Linq;

public class AttachCardPoolPrefab : MonoBehaviour
{
	[MenuItem("Window/Attach/AttachCardPoolPrefab")]
	static void Attach_CardPoolPrefab()
	{
		EditDeck editDeck = null;
		List<CardPrefab_CreateDeck> cardPrefab_CreateDecks_all = new List<CardPrefab_CreateDeck>();

		foreach (GameObject obj in Selection.gameObjects)
		{
			if(obj.GetComponent<EditDeck>() != null)
			{
				editDeck = obj.GetComponent<EditDeck>();
			}

			ScrollRect scrollRect = obj.GetComponent<ScrollRect>();
			if (scrollRect != null)
			{
				for(int i=0;i< scrollRect.content.childCount;i++)
				{
					CardPrefab_CreateDeck cardPrefab_CreateDeck = scrollRect.content.GetChild(i).GetComponent<CardPrefab_CreateDeck>();
					if (cardPrefab_CreateDeck != null)
					{
						cardPrefab_CreateDecks_all.Add(cardPrefab_CreateDeck);
					}
				}
			}
		}

		if(editDeck != null)
		{
			if(cardPrefab_CreateDecks_all.Count > 0)
			{
				editDeck.cardPoolPrefabs_all = cardPrefab_CreateDecks_all;
			}
		}
	}
}
