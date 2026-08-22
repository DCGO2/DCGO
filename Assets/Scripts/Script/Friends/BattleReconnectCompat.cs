using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// Minimal room-TTL / active-player helpers used by friend duels.
/// Official DCGO does not ship BattleReconnectService.
/// </summary>
public static class BattleReconnectService
{
    public const int PlayerTtlMs = 90000;
    const int MinEmptyRoomTtlMs = 90000;

    public static void ApplyBattleTtl(RoomOptions options)
    {
        if (options == null)
        {
            return;
        }

        options.PlayerTtl = PlayerTtlMs;
        if (options.EmptyRoomTtl < MinEmptyRoomTtlMs)
        {
            options.EmptyRoomTtl = MinEmptyRoomTtlMs;
        }
    }

    public static int CountActivePlayers()
    {
        if (PhotonNetwork.CurrentRoom == null || PhotonNetwork.CurrentRoom.Players == null)
        {
            return 0;
        }

        int n = 0;
        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if (player != null && !player.IsInactive)
            {
                n++;
            }
        }

        return n;
    }
}
