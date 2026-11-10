using ExitGames.Client.Photon;
using System;

public class CardSelection : IPlayerSelection
{
    public int[] CardIDList { get; private set; } = null;

    public CardSelection(int[] cardIDList)
    {
        CardIDList = cardIDList;
    }

    public CardSelection()
    {
    }

    public CardSelection(byte[] bytes)
    {
        Deserialize(bytes);
    }

    //A length of -1 means the list is null (no selection)
    public byte[] Serialize()
    {
        int length = CardIDList != null ? CardIDList.Length : -1;

        byte[] bytes = new byte[sizeof(int) * (1 + Math.Max(length, 0))];
        int index = 0;

        Protocol.Serialize(length, bytes, ref index);

        if (CardIDList != null)
        {
            foreach (int cardID in CardIDList)
            {
                Protocol.Serialize(cardID, bytes, ref index);
            }
        }

        return bytes;
    }

    public void Deserialize(byte[] bytes)
    {
        int index = 0;

        Protocol.Deserialize(out int length, bytes, ref index);

        CardIDList = length >= 0 ? new int[length] : null;

        for (int i = 0; i < length; i++)
        {
            Protocol.Deserialize(out CardIDList[i], bytes, ref index);
        }
    }
}
