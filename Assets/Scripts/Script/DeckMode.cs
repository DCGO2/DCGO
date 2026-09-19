using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DeckMode : MonoBehaviour
{
    [Header("DeckButton")]
    public OpeningButton DeckButton;

    [Header("selectDeck")]
    public SelectDeck selectDeck;

    [Header("editDeck")]
    public EditDeck editDeck;

    [Header("デッキ確認")]
    public DeckListPanel deckListPanel;

    [Header("Trial5Draw")]
    public TrialDraw trialDraw;

    bool first = false;

    public void OffDeck()
    {
        selectDeck.OffSelectDeck();

        editDeck.CreateDeckObject.SetActive(false);

        if(!first)
        {
            DeckButton.OnExit();
            first = true;
        }

        trialDraw.Off();

        deckListPanel.Off();
    }

    public void SetUpDeckMode()
    {
        if(selectDeck.isOpen)
        {
            return;
        }

        editDeck.CreateDeckObject.SetActive(false);

        selectDeck.SetUpSelectDeck();

        Opening.instance.optionPanel.CloseOptionPanel();
    }
}
