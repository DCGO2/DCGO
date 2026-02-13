using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.Linq;

//Class to manage the overall game situation
[System.Serializable]
public class GameContext
{
    #region constructor
    public GameContext(Player _You, Player _Opponent)
    {
        You = _You;
        Opponent = _Opponent;

        if (PhotonNetwork.IsConnected)
        {
            SetPlayerID();
        }

        Memory = 0;
    }
    #endregion

    #region メモリー
    public int Memory { get; set; } = 0;
    #endregion

    #region List of cards in scene
    public List<CardSource> ActiveCardList
    {
        get; set;
    } = new List<CardSource>();
    #endregion

    #region Player
    public Player You;
    public Player Opponent;

    public List<Player> Players
    {
        get
        {
            List<Player> players = new List<Player>();

            players.Add(PlayerFromID(0));
            players.Add(PlayerFromID(1));

            return players;
        }
    }

    public List<Player> Players_ForTurnPlayer
    {
        get
        {
            List<Player> players = new List<Player>();

            players.Add(TurnPlayer);
            players.Add(NonTurnPlayer);

            return players;
        }
    }

    public List<Player> Players_ForNonTurnPlayer
    {
        get
        {
            List<Player> players = new List<Player>();

            players.Add(NonTurnPlayer);
            players.Add(TurnPlayer);

            return players;
        }
    }

    public Player TurnPlayer;

    public Player NonTurnPlayer
    {
        get
        {
            Player _player = null;

            foreach (Player player in Players)
            {
                if (player != TurnPlayer)
                {
                    _player = player;
                    break;
                }
            }

            return _player;
        }
    }

    public Player FirstPlayer;

    #endregion

    #region Permanents
    public List<Permanent> PermanentsForTurnPlayer
    {
        get
        {
            return Players_ForTurnPlayer.Map(player => player.GetFieldPermanents()).Flat();
        }
    }
    #endregion

    #region Phase of the turn
    public enum phase
    {
        Active,
        Draw,
        Breeding,
        Main,
        End,
        None,
    }

    public phase TurnPhase;
    #endregion

    #region Player ID Assignment
    public void SetPlayerID()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            You.PlayerID = 0;
            Opponent.PlayerID = 1;
        }

        else
        {
            You.PlayerID = 1;
            Opponent.PlayerID = 0;
        }
    }
    #endregion

    #region Returns the player corresponding to the player ID
    public Player PlayerFromID(int playerID)
    {
        if (You.PlayerID == playerID)
        {
            return You;
        }

        else if (Opponent.PlayerID == playerID)
        {
            return Opponent;
        }

        return null;
    }
    #endregion

    public bool DoSwitchTurnPlayer { get; set; } = true;

    #region turn-player switching
    public void SwitchTurnPlayer()
    {
        if (DoSwitchTurnPlayer)
        {
            foreach (Player player in Players)
            {
                if (player != TurnPlayer)
                {
                    TurnPlayer = player;
                    break;
                }
            }
        }

        DoSwitchTurnPlayer = true;
    }
    #endregion

    public bool IsSecurityLooking { get; set; } = false;
}

