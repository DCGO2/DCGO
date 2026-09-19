using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Photon.Pun.UtilityScripts;
using System;

public class SelectRandomDeckButton : MonoBehaviour
{

    public GameObject Outline;

    public ScrollRect deckInfoPrefabParentScroll;

    public DeckInfoPanel deckInfoPanel;

    public List<int> validDeckList;

    public void OnClick()
    {
        validDeckList.Clear();
        haveValidDecks();
        if (validDeckList.Count > 0)
        {
            int randomDeck = getRandomDeck();
            deckInfoPrefabParentScroll.content.GetChild(validDeckList[randomDeck]).GetComponent<DeckInfoPrefab>().OnClick();
        }
    }

    public void haveValidDecks()
    {
        for (int i = 1; i < deckInfoPrefabParentScroll.content.childCount; i++)
        {
            if (deckInfoPrefabParentScroll.content.GetChild(i).GetComponent<DeckInfoPrefab>().thisDeckData.IsValidDeckData() == true)
            {
                validDeckList.Add(i);
            }
        }
    }

    public int getRandomDeck()
    {
        long random = RandomUtility.GetSecureRandom();
        GameRandom.Seed(random);
        int randomDeck = GameRandom.Range(0, validDeckList.Count);
        return randomDeck;
    }

    public void OnEnter()
    {
        if (Opening.instance != null)
            this.gameObject.transform.localScale = Opening.instance.DeckInfoPrefabExpandScale;
    }

    public void OnExit()
    {

        if (Opening.instance != null)
            this.gameObject.transform.localScale = Opening.instance.DeckInfoPrefabStartScale;
    }

    private void OnEnable()
    {
        OnExit();
    }
}