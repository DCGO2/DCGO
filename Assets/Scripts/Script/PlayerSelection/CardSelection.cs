using ExitGames.Client.Photon;

public class CardSelection : IPlayerSelection
{
    public int[] CardIDList { get; private set; } = null;

    public CardSelection(int[] cardIDList)
    {
        CardIDList = cardIDList;
    }

    public CardSelection()
    {
        CardIDList = new int[0]; 
    }

    public CardSelection(byte[] bytes)
    {
        Deserialize(bytes);
    }

    public byte[] Serialize()
    {
        byte[] bytes = new byte[sizeof(int) * (CardIDList.Length + 1)];
        int index = 0;

        Protocol.Serialize(CardIDList.Length, bytes, ref index);

        foreach (int cardID in CardIDList)
        {
            Protocol.Serialize(cardID, bytes, ref index);
        }

        return bytes;
    }

    public void Deserialize(byte[] bytes)
    {
        int index = 0;

        Protocol.Deserialize(out int length, bytes, ref index);

        CardIDList = new int[length];

        for (int i = 0; i < length; i++)
        {
            Protocol.Deserialize(out CardIDList[i], bytes, ref index);
        }
    }
}
