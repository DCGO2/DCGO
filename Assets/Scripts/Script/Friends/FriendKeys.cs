using System;

/// <summary>
/// Photon property keys and constants for friend list / direct duel.
/// </summary>
public static class FriendKeys
{
    public const string ModeProperty = "Mode";
    public const string ModeFriend = "friend";

    public const string TargetUserIdProperty = "FriendTargetUserId";
    public const string ChallengerUserIdProperty = "FriendChallengerUserId";
    public const string ChallengerNameProperty = "FriendChallengerName";
    public const string WinsToTakeProperty = "FriendWinsToTake";
    public const string UseBanlistProperty = "UseBanlist";

    public const string SeriesWinsAProperty = "FriendSeriesWinsA";
    public const string SeriesWinsBProperty = "FriendSeriesWinsB";
    public const string GameIndexProperty = "FriendGameIndex";
    public const string LastLoserProperty = "FriendLastLoser";
    public const string NextFirstUserIdProperty = "FriendNextFirstUserId";
    public const string NextFirstGameIndexProperty = "FriendNextFirstGameIndex";
    public const string OnResultProperty = "FriendOnResult";
    public const string UserIdAProperty = "FriendUserIdA";
    public const string UserIdBProperty = "FriendUserIdB";

    public const string RoomNamePrefix = "fd-";

    public static bool IsFriendDuelRoomName(string roomName)
    {
        return !string.IsNullOrEmpty(roomName) &&
               roomName.StartsWith(RoomNamePrefix, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// True when the local client is in a friend-challenge Photon room (name or Mode prop).
    /// Use this when ContinuousController.isFriendDuel may have been cleared mid-flow.
    /// </summary>
    public static bool IsInFriendDuelRoom()
    {
        if (!Photon.Pun.PhotonNetwork.InRoom || Photon.Pun.PhotonNetwork.CurrentRoom == null)
        {
            return false;
        }

        if (IsFriendDuelRoomName(Photon.Pun.PhotonNetwork.CurrentRoom.Name))
        {
            return true;
        }

        var props = Photon.Pun.PhotonNetwork.CurrentRoom.CustomProperties;
        if (props != null &&
            props.TryGetValue(ModeProperty, out object modeObj) &&
            modeObj is string mode &&
            string.Equals(mode, ModeFriend, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    public const int MaxFriends = 50;
    public const float FindFriendsPollSeconds = 2f;
    public const float InviteTimeoutSeconds = 60f;

    public const string LocalFriendsPrefsKey = "DCGO_FriendList";
    public const string LastOpponentIdPrefsKey = "DCGO_LastOpponentId";
    public const string LastOpponentNamePrefsKey = "DCGO_LastOpponentName";

    /// <summary>Lobby custom properties visible to home-presence clients.</summary>
    public static readonly string[] LobbyProperties =
    {
        ModeProperty,
        TargetUserIdProperty,
        ChallengerUserIdProperty,
        ChallengerNameProperty,
        WinsToTakeProperty,
        UseBanlistProperty,
        "RoomCreator",
    };
}
