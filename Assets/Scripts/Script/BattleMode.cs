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
    // === DCGO-CUSTOM:ranked begin ===
    [Header("RankedMatch")]
    public LobbyManager_RankedMatch lobbyManager_RankedMatch;
    // === DCGO-CUSTOM:ranked end ===

    [Header("Room Screen")]
    public RoomManager roomManager;
    // === DCGO-CUSTOM:tournament begin ===
    [Header("Tournament")]
    public TournamentLobbyManager tournamentLobbyManager;
    // === DCGO-CUSTOM:tournament end ===

    bool first = false;

    public void OffBattle()
    {
        roomManager.Off();
        // === DCGO-CUSTOM:tournament begin ===
        tournamentLobbyManager?.Off();
        // === DCGO-CUSTOM:tournament end ===

        selectBattleDeck.Off();

        selectBattleMode.OffSelectBattleMode();

        lobbyManager_RandomMatch.OffLobby();
        // === DCGO-CUSTOM:ranked begin ===
        if (lobbyManager_RankedMatch != null)
        {
            lobbyManager_RankedMatch.OffLobby();
        }
        // === DCGO-CUSTOM:ranked end ===

        // === DCGO-CUSTOM:friends begin ===
        FriendListPanel.HideIfOpen();
        // === DCGO-CUSTOM:friends end ===

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
