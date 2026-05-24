public class PermanentSelection : IPlayerSelection
{
    public bool[] IsTurnPlayerList { get; private set; }
    public int[] PermanentIDList { get; private set; }

    public PermanentSelection(bool[] isTurnPlayerList, int[] permanentIDList)
    {
        IsTurnPlayerList = isTurnPlayerList;
        PermanentIDList = permanentIDList;
    }
}
