using ExitGames.Client.Photon;

public class ValueSelection : IPlayerSelection
{
    public int _value;

    public ValueSelection(int value)
    {
        _value = value;
    }

    public ValueSelection(bool value)
    {
        _value = value ? 1 : 0;
    }

    public ValueSelection(byte[] bytes)
    {
        Deserialize(bytes);
    }

    public int ValueAsInt()
    {
        return _value;
    }

    public bool ValueAsBool()
    {
        return _value != 0;
    }

    public byte[] Serialize()
    {
        byte[] bytes = new byte[sizeof(int)];
        int index = 0;

        Protocol.Serialize(_value, bytes, ref index);

        return bytes;
    }

    public void Deserialize(byte[] bytes)
    {
        int index = 0;
        Protocol.Deserialize(out _value, bytes, ref index);
    }
}
