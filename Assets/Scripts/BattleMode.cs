using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleMode : MonoBehaviour
{
    [Header("Battle Button")]
    public OpeningButton BattleButton;

    [Header("Battle Mode Selection")]
    public SelectBattleMode selectBattleMode;

    [Header("Battle Deck Selection")]
    public SelectBattleDeck selectBattleDeck;

    [Header("RandomMatch")]
    public LobbyManager_RandomMatch lobbyManager_RandomMatch;

    [Header("Room Screen")]
    public RoomManager roomManager;

    bool first = false;

    public void OffBattle()
    {
        roomManager.Off();

        selectBattleDeck.Off();

        selectBattleMode.OffSelectBattleMode();

        lobbyManager_RandomMatch.OffLobby();

        if (!first)
        {
            BattleButton.OnExit();
            first = true;
        }
    }

    public void SetUpBattleMode()
    {
        selectBattleMode.SetUpSelectBattleMode();
        Opening.instance.optionPanel.CloseOptionPanel();
    }
}
