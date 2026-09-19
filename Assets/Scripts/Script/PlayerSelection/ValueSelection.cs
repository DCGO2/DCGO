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

    public int ValueAsInt()
    {
        return _value;
    }

    public bool ValueAsBool()
    {
        return _value != 0;
    }
}
