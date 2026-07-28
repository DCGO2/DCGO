using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Photon.Pun.UtilityScripts;

public class SelectRandomDeckButton : MonoBehaviour
{
    
    public GameObject Outline;

    public ScrollRect deckInfoPrefabParentScroll;

    public void OnClick()
    {
        long random = RandomUtility.GetSecureRandom();
        GameRandom.Seed(random);
        int randomDeck = GameRandom.Range(1,deckInfoPrefabParentScroll.content.childCount);
        deckInfoPrefabParentScroll.content.GetChild(randomDeck).GetComponent<DeckInfoPrefab>().OnClick();
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
