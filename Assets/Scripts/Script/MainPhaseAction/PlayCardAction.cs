using ExitGames.Client.Photon;

public class PlayCardAction : MainPhaseAction
{
    int CardIndex;
    int TargetFrameID;
    int[] JogressEvoRootsFrameIDs;
    int BurstTamerFrameID;
    int[] AppFusionFrameIDs;

    public PlayCardAction(int cardIndex, int targetFrameID, int[] jogressEvoRootsFrameIDs, int burstTamerFrameID, int[] appFusionFrameIDs)
    {
        CardIndex = cardIndex;
        TargetFrameID = targetFrameID;
        JogressEvoRootsFrameIDs = jogressEvoRootsFrameIDs;
        BurstTamerFrameID = burstTamerFrameID;
        AppFusionFrameIDs = appFusionFrameIDs;

    }

    public PlayCardAction(byte[] bytes)
    {
        CardIndex = -1;
        TargetFrameID = -1;
        JogressEvoRootsFrameIDs = null;
        BurstTamerFrameID = -1;
        AppFusionFrameIDs = null;

        Deserialize(bytes);
    }

    public override void Execute(TurnStateMachine stateMachine)
    {
        stateMachine.SetPlayCard(CardIndex, TargetFrameID, JogressEvoRootsFrameIDs, BurstTamerFrameID, AppFusionFrameIDs);
    }

    public override void Deserialize(byte[] bytes)
    {
        int index = 0;

        Protocol.Deserialize(out CardIndex, bytes, ref index);
        Protocol.Deserialize(out TargetFrameID, bytes, ref index);

        Protocol.Deserialize(out int JogressSize, bytes, ref index);
        if (JogressSize > 0)
        {
            JogressEvoRootsFrameIDs = new int[JogressSize];
            for (int i = 0; i < JogressSize; i++)
            {
                Protocol.Deserialize(out JogressEvoRootsFrameIDs[i], bytes, ref index);
            }
        }

        Protocol.Deserialize(out BurstTamerFrameID, bytes, ref index);

        Protocol.Deserialize(out int AppFuseSize, bytes, ref index);
        if (AppFuseSize > 0)
        {
            AppFusionFrameIDs = new int[AppFuseSize];
            for (int i = 0; i < AppFuseSize; i++)
            {
                Protocol.Deserialize(out AppFusionFrameIDs[i], bytes, ref index);
            }
        }
    }


    public override byte[] Serialize()
    {
        int JogressSize = JogressEvoRootsFrameIDs != null ? JogressEvoRootsFrameIDs.Length : 0;
        int AppFuseSize = AppFusionFrameIDs != null ? AppFusionFrameIDs.Length : 0;

        byte[] bytes = new byte[sizeof(int) * (5 + JogressSize + AppFuseSize)];
        int index = 0;

        Protocol.Serialize(CardIndex, bytes, ref index);
        Protocol.Serialize(TargetFrameID, bytes, ref index);

        Protocol.Serialize(JogressSize, bytes, ref index);
        if (JogressSize > 0)
        {
            for (int i = 0; i < JogressSize; i++)
            {
                Protocol.Serialize(JogressEvoRootsFrameIDs[i], bytes, ref index);
            }
        }

        Protocol.Serialize(BurstTamerFrameID, bytes, ref index);

        Protocol.Serialize(AppFuseSize, bytes, ref index);
        if (AppFuseSize > 0)
        {
            for (int i = 0; i < AppFuseSize; i++)
            {
                Protocol.Serialize(AppFusionFrameIDs[i], bytes, ref index);
            }
        }

        return bytes;
    }
}
