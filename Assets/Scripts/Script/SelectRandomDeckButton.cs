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

    public void OnClick()
    {
        if (haveValidDecks())
        {
            if(deckInfoPrefabParentScroll.content.childCount > 1)
            {
                int randomDeck = getRandomDeck();
                deckInfoPrefabParentScroll.content.GetChild(randomDeck).GetComponent<DeckInfoPrefab>().OnClick();
                //If it selects a invalid deck, it will keep randomly tryig decks until it finds a valid one
                while (deckInfoPanel.ShowingDeckData.IsValidDeckData() == false)
                {
                    randomDeck = getRandomDeck();
                    deckInfoPrefabParentScroll.content.GetChild(randomDeck).GetComponent<DeckInfoPrefab>().OnClick();
                }
            }
        }
    }

    //Ensures the player has at least one valid deck
    public Boolean haveValidDecks()
    {
        int validDecks = 0;
        for (int i=1; i < deckInfoPrefabParentScroll.content.childCount; i++)
        {
            if (deckInfoPrefabParentScroll.content.GetChild(i).GetComponent<DeckInfoPrefab>().thisDeckData.IsValidDeckData() == true)
            {
                validDecks++;
            }
        }
        if (validDecks > 0)
        {
            return true;
        }
        return false;
    }

    public int getRandomDeck()
    {
        long random = RandomUtility.GetSecureRandom();
        GameRandom.Seed(random);
        int randomDeck = GameRandom.Range(1,deckInfoPrefabParentScroll.content.childCount);
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
