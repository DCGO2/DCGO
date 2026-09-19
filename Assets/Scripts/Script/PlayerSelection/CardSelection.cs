public class CardSelection : IPlayerSelection
{
    public int[] CardIDList { get; private set; }

    public CardSelection(int[] cardIDList)
    {
        CardIDList = cardIDList;
    }
}
