using ExitGames.Client.Photon;
using System;

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

    //A count of -1 means the list is null
    public byte[] Serialize()
    {
        int PermanentIDListCount = PermanentIDList != null ? PermanentIDList.Length : -1;
        int IsTurnPlayerListCount = IsTurnPlayerList != null ? IsTurnPlayerList.Length : -1;

        byte[] bytes = new byte[sizeof(int) * (2 + Math.Max(PermanentIDListCount, 0) + Math.Max(IsTurnPlayerListCount, 0))];
        int index = 0;

        Protocol.Serialize(PermanentIDListCount, bytes, ref index);
        Protocol.Serialize(IsTurnPlayerListCount, bytes, ref index);

        if (PermanentIDList != null)
        {
            foreach (var PermanentID in PermanentIDList)
            {
                Protocol.Serialize(PermanentID, bytes, ref index);
            }
        }

        if (IsTurnPlayerList != null)
        {
            foreach (var IsTurnPlayer in IsTurnPlayerList)
            {
                Protocol.Serialize(IsTurnPlayer ? 1 : 0, bytes, ref index);
            }
        }

        return bytes;
    }

    public void Deserialize(byte[] bytes)
    {
        int index = 0;

        Protocol.Deserialize(out int PermanentIDListCount, bytes, ref index);
        Protocol.Deserialize(out int IsTurnPlayerListCount, bytes, ref index);

        PermanentIDList = PermanentIDListCount >= 0 ? new int[PermanentIDListCount] : null;
        IsTurnPlayerList = IsTurnPlayerListCount >= 0 ? new bool[IsTurnPlayerListCount] : null;

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
