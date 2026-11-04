using ExitGames.Client.Photon;

public class PermanentSelection : IPlayerSelection
{
    public bool[] IsTurnPlayerList { get; private set; }
    public int[] PermanentIDList { get; private set; }

    public PermanentSelection(bool[] isTurnPlayerList, int[] permanentIDList)
    {
        IsTurnPlayerList = isTurnPlayerList;
        PermanentIDList = permanentIDList;
    }

    public PermanentSelection(bool isTurnPlayer, int permanentID)
    {
        IsTurnPlayerList = new bool[] { isTurnPlayer };
        PermanentIDList = new int[] { permanentID };
    }

    public PermanentSelection(byte[] bytes)
    {
        Deserialize(bytes);
    }

    public byte[] Serialize()
    {
        byte[] bytes = new byte[sizeof(int) * ((PermanentIDList.Length + 1) + (IsTurnPlayerList.Length + 1))];
        int index = 0;

        Protocol.Serialize(PermanentIDList.Length, bytes, ref index);
        Protocol.Serialize(IsTurnPlayerList.Length, bytes, ref index);

        foreach (var PermanentID in PermanentIDList)
        {
            Protocol.Serialize(PermanentID, bytes, ref index);
        }

        foreach (var IsTurnPlayer in IsTurnPlayerList)
        {
            Protocol.Serialize(IsTurnPlayer ? 1 : 0, bytes, ref index);
        }

        return bytes;
    }

    public void Deserialize(byte[] bytes)
    {
        int index = 0;

        Protocol.Deserialize(out int PermanentIDListCount, bytes, ref index);
        Protocol.Deserialize(out int IsTurnPlayerListCount, bytes, ref index);

        PermanentIDList = new int[PermanentIDListCount];
        IsTurnPlayerList = new bool[IsTurnPlayerListCount];

        for (int i = 0; i < PermanentIDListCount; i++)
        {
            Protocol.Deserialize(out PermanentIDList[i], bytes, ref index);
        }

        for (int i = 0; i < IsTurnPlayerListCount; i++)
        {
            Protocol.Deserialize(out int IsTurnPlayerInt, bytes, ref index);
            IsTurnPlayerList[i] = IsTurnPlayerInt == 1;
        }
    }
}
